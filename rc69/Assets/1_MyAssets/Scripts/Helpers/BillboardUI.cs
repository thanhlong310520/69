using UnityEngine;

namespace Raccoon.UI
{
    public class BillboardUI : MonoBehaviour
    {
        public enum BillboardMode
        {
            Full,       // Rotate in all directions (default)
            YAxisOnly   // Only rotate around Y axis
        }

        [Header("Billboard Settings")]
        public BillboardMode mode = BillboardMode.Full;

        private Camera mainCam;

        private void Start()
        {
            CacheCamera();
        }

        private void LateUpdate()
        {
            if (mainCam == null)
                CacheCamera();

            if (mainCam == null)
                return;

            switch (mode)
            {
                case BillboardMode.Full:
                    FaceFull();
                    break;

                case BillboardMode.YAxisOnly:
                    FaceYAxisOnly();
                    break;
            }
        }

        private void FaceFull()
        {
            Vector3 camForward = mainCam.transform.rotation * Vector3.forward;
            Vector3 camUp = mainCam.transform.rotation * Vector3.up;
            transform.LookAt(transform.position + camForward, camUp);
        }

        private void FaceYAxisOnly()
        {
            Vector3 targetPos = mainCam.transform.position;
            targetPos.y = transform.position.y; // Lock Y axis
            
            transform.LookAt(targetPos);
            
            // Apply yaw offset (rotate around the Y axis)
            transform.Rotate(Vector3.up, 0, Space.Self);
        }

        private void CacheCamera()
        {
            if (Camera.main != null)
            {
                mainCam = Camera.main;
                return;
            }

            // var controller = VoxelPlay.VoxelPlayThirdPersonController.instance;
            // if (controller != null)
            //     mainCam = controller.m_Camera;
        }
    }
}