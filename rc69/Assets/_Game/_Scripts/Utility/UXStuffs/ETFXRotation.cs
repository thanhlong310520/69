using UnityEngine;
using System.Collections;
 
namespace EpicToonFX
{
    public class ETFXRotation : MonoBehaviour
    {
 
        [Header("Rotate axises by degrees per second")]
        public Vector3 rotateVector = Vector3.zero;
 
        public enum spaceEnum { Local, World };
        public spaceEnum rotateSpace;

        [Header("Bám VỊ TRÍ của một transform khác (không bám hướng)")]
        [Tooltip("Gán lúc chạy bằng Follow(). Để trống thì component chạy y hệt như trước.\n\n" +
                 "Chỉ chép VỊ TRÍ, tuyệt đối không chép rotation — đó là cả lý do nó tồn tại. " +
                 "Effect gắn vào xương đầu ragdoll bằng cách parent sẽ thừa hưởng luôn cái lắc của " +
                 "cái đầu khi xác bị bắn đổ, và chùm sao xoay mòng mòng theo. Tách rời ra thì cái " +
                 "xoay duy nhất còn lại là rotateVector bên trên — đúng thứ effect này vẽ ra.")]
        public Transform followTarget;

        [Tooltip("Cộng thêm vào vị trí followTarget, tính theo trục THẾ GIỚI.\n\n" +
                 "Thế giới chứ không phải local của target: đầu NPC ngoẹo sang một bên thì chùm sao " +
                 "vẫn phải nằm ngang trên đỉnh đầu, không nghiêng theo.")]
        public Vector3 followOffset = Vector3.zero;

        /// <summary>Đã từng được giao target — để phân biệt "không dùng follow" với "target đã chết".</summary>
        private bool hadTarget;

        /// <summary>
        /// Cách thu dọn khi target chết. null thì <c>Destroy</c>.
        ///
        /// Có nó để component này không phải biết gì về pool: instance lấy ra từ đâu thì chỗ đó tự
        /// đưa cách trả lại.
        /// </summary>
        private System.Action<Transform> onTargetLost;
 
        // Use this for initialization
        void Start()
        {
 
        }
 
        // Update is called once per frame
        void Update()
        {
            if (rotateSpace == spaceEnum.Local)
                transform.Rotate(rotateVector * Time.deltaTime);
            if (rotateSpace == spaceEnum.World)
                transform.Rotate(rotateVector * Time.deltaTime, Space.World);
        }

        /// <summary>
        /// Bật chế độ bám vị trí: đặt ngay một lần rồi bám mỗi frame trong <see cref="LateUpdate"/>.
        /// </summary>
        /// <param name="target">Transform cần bám. null là tắt chế độ bám.</param>
        /// <param name="heightOffset">Nâng lên bấy nhiêu mét theo trục Y THẾ GIỚI.</param>
        /// <param name="onTargetLost">
        /// Gọi khi target chết, nhận chính transform này. Để trống thì <c>Destroy</c>.
        /// Instance lấy từ pool phải truyền hàm trả về pool vào đây.
        /// </param>
        public void Follow(Transform target, float heightOffset,
                           System.Action<Transform> onTargetLost = null)
        {
            followTarget = target;
            followOffset = Vector3.up * heightOffset;
            hadTarget = target != null;
            this.onTargetLost = onTargetLost;

            // Đặt ngay tại đây chứ không đợi LateUpdate: không làm thì effect hiện đúng một frame ở
            // chỗ Instantiate rồi mới nhảy lên đầu.
            if (hadTarget)
                transform.position = target.position + followOffset;
        }

        /// <summary>
        /// LateUpdate chứ không phải Update: xương ragdoll do physics ghi ra, đọc trong Update là đọc
        /// vị trí của frame trước, và chùm sao sẽ lệt bệt phía sau cái đầu đang rơi.
        /// </summary>
        void LateUpdate()
        {
            if (!hadTarget)
                return;

            // Target chết thì effect hết chỗ bám. Đây là toàn bộ phần dọn dẹp: nó cố tình không phải
            // con của NPC (xem followTarget) nên không thể chết theo cha được.
            if (followTarget == null)
            {
                var release = onTargetLost;

                // Xoá trạng thái TRƯỚC khi trả về pool: instance tái chế mà còn hadTarget thì ngay
                // frame nó được spawn lại, LateUpdate thấy followTarget null và despawn nó lần nữa —
                // PoolManager bắn LogError "already been despawned".
                followTarget = null;
                hadTarget = false;
                onTargetLost = null;

                if (release != null)
                    release(transform);
                else
                    Destroy(gameObject);

                return;
            }

            transform.position = followTarget.position + followOffset;
        }
    }
}