using System;
using System.Collections;
using UnityEngine;

namespace Raccoon.Controller
{
    /// <summary>
    /// Component dùng chung cho mọi project để chuẩn hóa flow "show ads":
    ///   1) Nếu chắc chắn không thể load ads (chưa init, mất mạng, ads disable...) -> KHÔNG hiện loading, fail ngay.
    ///   2) Luôn hiện loading trước khi show ads, tối đa MaxWaitSeconds (mặc định 10s, có thể remote config).
    ///      - Nếu đã có ad sẵn (cache từ lần load trước) -> show ngay lập tức.
    ///      - Nếu đang chờ load mà có ad trước khi hết giờ -> show luôn.
    ///      - Nếu hết giờ mà vẫn chưa có -> ẩn loading, quay lại game (ad vẫn tiếp tục load nền cho lần sau).
    ///   3) Mọi ad load thành công đều được cache lại (project tự cache trong field, gate chỉ poll isReady()).
    ///
    /// Component này KHÔNG phụ thuộc bất kỳ SDK ads nào (Google Mobile Ads, Bacon, Applovin, ...),
    /// chỉ nhận callback nên có thể gắn vào project nào cũng được, miễn project đó tự định nghĩa
    /// 3 hàm: canPossiblyLoad / isReady / requestLoad / showAd theo đúng SDK của mình.
    /// </summary>
    public class AdLoadingGate : MonoBehaviour
    {
        public static AdLoadingGate Get;

        // Thời gian chờ tối đa trước khi bỏ qua ads, quay lại game.
        // Sau này thay bằng giá trị lấy từ Remote Config (Firebase / backend riêng).
        public static float MaxWaitSeconds = 10f;

        // Safety-net: nếu SDK ad (native overlay, mediation...) không bắn callback đóng ad
        // (đã từng xảy ra: "webview is destroyed", app bị kill/crash native trong lúc show...),
        // ResumeGame() có thể không bao giờ được gọi -> game bị đứng (timeScale = 0) vĩnh viễn.
        // Watchdog này force-resume sau tối đa ngần này giây kể từ lúc PauseGame(), bất kể ad có
        // đóng đúng cách hay không.
        public static float MaxPausedSeconds = 240f;

        [Header("Panel loading hiển thị khi đang chờ ads (gán theo từng project)")]
        public GameObject loadingPanel;

        // Đảm bảo tại 1 thời điểm chỉ có 1 request show ads đang chạy,
        // tránh hiện 2 loading panel chồng nhau nếu game gọi show ads liên tiếp.
        private bool _isRequesting = false;

        // Pause game ngay khi request show ads (không phải khi ad thật sự hiện),
        // để gameplay không chạy ngầm phía sau loading panel.
        private bool _pausedByGate = false;
        private float _cachedTimeScale = 1f;
        private Coroutine _watchdog;

        private void Awake()
        {
            Get = this;
            loadingPanel.SetActive(false);
        }

        /// <summary>
        /// Pause gameplay (timeScale + audio) ngay tại thời điểm request show ads.
        /// An toàn với việc load/show ad vì: (1) load ads chạy ở native SDK, không phụ thuộc Time.timeScale,
        /// (2) vòng lặp chờ trong CoRequestShowAd dùng Time.unscaledDeltaTime.
        /// </summary>
        private void PauseGame()
        {
            if (_pausedByGate) return;
            _pausedByGate = true;
            _cachedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            // _watchdog = StartCoroutine(CoWatchdogResume());
        }

        /// <summary>
        /// Fallback: nếu không có ai gọi ResumeGame() (ad SDK không bắn callback đóng ad)
        /// trong MaxPausedSeconds, tự resume để game không bị đứng vĩnh viễn.
        /// </summary>
        private IEnumerator CoWatchdogResume()
        {
            yield return new WaitForSecondsRealtime(MaxPausedSeconds);
            if (_pausedByGate)
            {
                Debug.LogWarning("[AdLoadingGate] Ad không đóng sau " + MaxPausedSeconds +
                                  "s, force resume game để tránh đứng game.");
                loadingPanel?.SetActive(false);
                ResumeGame();
            }
        }

        /// <summary>
        /// Resume gameplay. Gọi khi: (a) request timeout/fail (ad chưa từng hiện),
        /// hoặc (b) ad đã đóng xong (gọi từ GameAdsNew sau khi unit.OnAdClosedOrFailed bắn ra).
        /// Luôn set giá trị đã cache thay vì tin tưởng trạng thái hiện tại, vì NativeOverlay
        /// (isPauseOnShow) cũng tự restore timeScale khi đóng ad và có thể chạy trước lệnh này.
        /// </summary>
        public void ResumeGame(bool force = false)
        {
            if (!_pausedByGate && !force) return;
            _pausedByGate = false;
            if (_cachedTimeScale <= 0) _cachedTimeScale = 1f;
            Time.timeScale = _cachedTimeScale;
            AudioListener.pause = false;

            if (_watchdog != null)
            {
                StopCoroutine(_watchdog);
                _watchdog = null;
            }
        }

        /// <summary>
        /// Yêu cầu show 1 ad theo flow chuẩn (xem mô tả class).
        /// </summary>
        /// <param name="canPossiblyLoad">
        /// Check đồng bộ, nhanh: ads đã init? có internet? ads có bị tắt (remote config / no-ads IAP)?
        /// Trả về false thì gate sẽ KHÔNG hiện loading panel, fail ngay lập tức.
        /// </param>
        /// <param name="isReady">Trả về true khi instance ad hiện đang cache sẵn sàng để show.</param>
        /// <param name="requestLoad">
        /// Yêu cầu load (phải tự idempotent bên trong: nếu đang load rồi hoặc đã có ad rồi thì không load lại).
        /// </param>
        /// <param name="showAd">Gọi Show() thật sự lên SDK ads.</param>
        /// <param name="onResult">
        /// true = ad đã được show. false = timeout / không thể load -> game tự xử lý (vào lại game bình thường).
        /// </param>
        public void RequestShowAd(
            Func<bool> canPossiblyLoad,
            Func<bool> isReady,
            Action requestLoad,
            Action showAd,
            Action<bool> onResult,
            Action showFail
            )
        {
            if (_isRequesting)
            {
                Debug.LogWarning("[AdLoadingGate] Đang có 1 request show ads khác, bỏ qua request mới.");
                onResult?.Invoke(false);
                return;
            }

            StartCoroutine(CoRequestShowAd(canPossiblyLoad, isReady, requestLoad, showAd, onResult,showFail));
        }

        private IEnumerator CoRequestShowAd(
            Func<bool> canPossiblyLoad,
            Func<bool> isReady,
            Action requestLoad,
            Action showAd,
            Action<bool> onResult,
            Action showFail
            )
        {
            _isRequesting = true;

            // 1) Chắc chắn không thể có ads -> không hiện loading, fail ngay. Không pause vì ad chắc chắn không hiện.
            if (canPossiblyLoad != null && !canPossiblyLoad.Invoke())
            {
                _isRequesting = false;
                onResult?.Invoke(false);
                showFail?.Invoke();
                yield break;
            }

            // Pause ngay tại thời điểm request (không đợi ad thật sự hiện), để gameplay không
            // chạy ngầm phía sau loading panel. Resume ở nhánh timeout bên dưới nếu ad không hiện được;
            // nếu show thành công thì resume được gọi từ GameAdsNew khi ad đóng (ForceHidePanelShowAd).
            PauseGame();

            // 2) Đã có sẵn ad (cache từ trước) -> show ngay, không cần chờ.
            if (isReady != null && isReady.Invoke())
            {
                Debug.Log("AdLoadingGate da co");
                _isRequesting = false;
                showAd?.Invoke();
                onResult?.Invoke(true);
                yield break;
            }

            // 3) Chưa có ad -> luôn hiện loading, request load, đợi tối đa MaxWaitSeconds.
            loadingPanel?.SetActive(true);
            requestLoad?.Invoke();

            float waited = 0f;
            while (waited < MaxWaitSeconds)
            {
                if (isReady != null && isReady.Invoke())
                {
                    Debug.Log("AdLoadingGate load done");

                    //loadingPanel?.SetActive(false);//
                    _isRequesting = false;
                    showAd?.Invoke();
                    onResult?.Invoke(true);
                    yield break;
                }
                Debug.Log("AdLoadingGate loading");
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            // Hết giờ chờ -> ẩn loading, quay lại game.
            // requestLoad đã được gọi ở trên nên ad vẫn tiếp tục load nền,
            // sẵn sàng cho lần gọi show kế tiếp (đáp ứng yêu cầu "load thành công thì dùng cho lần sau").
            Debug.Log("AdLoadingGate khong load duoc");
            showFail?.Invoke();
            loadingPanel?.SetActive(false);
            _isRequesting = false;
            ResumeGame();
            onResult?.Invoke(false);
        }

        public bool IsAdShowing => loadingPanel?.activeSelf ?? false;
    }
}
