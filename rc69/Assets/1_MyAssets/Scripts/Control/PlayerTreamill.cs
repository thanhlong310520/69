using Raccoon.Data;
using Raccoon.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Raccoon.Player
{
    public class PlayerTreamill : MonoBehaviour
    {
        [SerializeField] float timeRefresh = 0.5f;
        [SerializeField] bool running;

        CharactorData data;
        Treamill treamill;
        TreamillDataSO treamillData;
        UnityAction actionChangeSpeed;
        float currentTime;


        public bool IsRunning => running;

        public void StartRun(Treamill treamill, CharactorData data, UnityAction actionChangeSpeed)
        {
            if (treamill == null || treamill.GetData == null || data == null) return;
            this.treamill = treamill;
            this.data = data;
            this.treamillData = treamill.GetData;
            this.actionChangeSpeed = actionChangeSpeed;
            currentTime = 0;
            running = true;
        }

        public void StopRun()
        {
            if (!running) return;
            running = false;
            actionChangeSpeed?.Invoke();
            data = null;
            treamill = null;
            treamillData = null;
            actionChangeSpeed = null;
        }

        Vector3 GetPopupSpawnPos()
        {
            if (treamill != null && treamill.holderPos != null) return treamill.holderPos.position;
            return transform.position;
        }

        private void Update()
        {
            if (!running) return;

            currentTime += Time.deltaTime;

            if (SpeedPopupSpawner.instance != null)
                SpeedPopupSpawner.instance.Spawn(GetPopupSpawnPos(), treamillData.speedPerSecond);

            if (currentTime >= timeRefresh)
            {
                float add = treamillData.speedPerSecond;
                data.multybuytreaMill += add;

                currentTime = 0;
                actionChangeSpeed?.Invoke();
            }
        }
    }
}
