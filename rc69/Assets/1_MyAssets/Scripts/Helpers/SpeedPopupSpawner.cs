using Raccoon.Controller;
using Raccoon.Data;
using UnityEngine;

namespace Raccoon.UI
{
    public class SpeedPopupSpawner : MonoBehaviour
    {
        public static SpeedPopupSpawner instance;

        [SerializeField] GameObject speedPopupPrefab;

        [Header("Random spawn area (local offset from base position)")]
        [SerializeField] Vector2 rangeX = new Vector2(-0.5f, 0.5f);
        [SerializeField] Vector2 rangeZ = new Vector2(-0.5f, 0.5f);

        void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
        }
        float currentTime = 0;
        public float timeRefresh =0.5f;
        public void Spawn(Vector3 basePos, float amount)
        {

            currentTime += Time.deltaTime;
            if (currentTime >= timeRefresh)
            {
                currentTime = 0;

                if (speedPopupPrefab == null || GamePlayControl.instance == null) return;

                Vector3 offset = new Vector3(
                    Random.Range(rangeX.x, rangeX.y),
                    0f,
                    Random.Range(rangeZ.x, rangeZ.y));

                GameObject obj = GamePlayControl.instance.SpawnItem(speedPopupPrefab);
                if (obj == null) return;

                obj.SetActive(true);
                obj.transform.SetPositionAndRotation(basePos + offset, Quaternion.identity);

                SpeedPopup popup = obj.GetComponent<SpeedPopup>();
                if (popup != null) popup.Show(amount);
            }

            
        }
    }
}
