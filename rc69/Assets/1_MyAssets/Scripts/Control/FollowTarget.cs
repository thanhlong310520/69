using UnityEngine;


namespace Raccoon.Controller
{
    public class FollowTarget : MonoBehaviour
    {
        public float hightOffset = 2.0f;
        public Transform targetTransform;

        public void Init(Transform target)
        {
            targetTransform = target;
        }

        // Update is called once per frame
        void Update()
        {
            if(targetTransform != null)
            {
                Vector3 targetPos = targetTransform.position;
                targetPos.y += hightOffset;
                transform.position = targetPos;
                transform.rotation = targetTransform.rotation;
            }
        }
    }
}