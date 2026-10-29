using DG.Tweening;
using Raccoon.Controller;
using TMPro;
using UnityEngine;

namespace Raccoon.UI
{
    // Attach to a World Space Canvas prefab (TMP_Text child) pooled via GamePool.
    public class SpeedPopup : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] TMP_Text txtValue;
        [SerializeField] CanvasGroup canvasGroup;

        [Header("Animation")]
        [SerializeField] float flyHeight = 1.5f;
        [SerializeField] float duration = 1f;
        [SerializeField] float fadeDuration = 0.4f;
        [SerializeField] float punchScale = 0.15f;

        Sequence sequence;

        public void Show(float amount)
        {
            sequence?.Kill();

            transform.localScale = Vector3.one;
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            if (txtValue != null) txtValue.text = $"+{amount:0.##}";

            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Vector3.up * flyHeight;

            sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(endPos, duration).SetEase(Ease.OutQuad));
            sequence.Join(transform.DOPunchScale(Vector3.one * punchScale, duration * 0.3f, 1, 0f));

            if (canvasGroup != null)
                sequence.Insert(duration - fadeDuration, canvasGroup.DOFade(0f, fadeDuration));

            sequence.SetUpdate(true);
            sequence.OnComplete(() =>
            {
                if (GamePlayControl.instance != null) GamePlayControl.instance.RespawnItem(gameObject);
                else gameObject.SetActive(false);
            });
        }

        void OnDisable()
        {
            sequence?.Kill();
        }
    }
}
