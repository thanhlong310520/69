using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Raccoon.Data
{
    [CreateAssetMenu(fileName = "Rebirth Database", menuName = "Raccoon/Rebirth/RebirthDatabaseSO")]
    public class RebirthDatabaseSO : ScriptableObject
    {
        public List<RebirthDataSO> datas = new();

        public RebirthDataSO GetFirstAvailable(List<string> completedIds, long playerCoin, List<string> slottedBotIds)
        {
            foreach (var d in datas)
            {
                if (!completedIds.Contains(d.GetId) && d.MeetRequirement(playerCoin, slottedBotIds))
                    return d;
            }
            return null;
        }

        public RebirthDataSO GetNextPending(List<string> completedIds)
        {
            return datas.FirstOrDefault(d => !completedIds.Contains(d.GetId));
        }

        public RebirthDataSO GetLastOwned(List<string> completedIds)
        {
            return datas.LastOrDefault(d => completedIds.Contains(d.GetId));
        }

        public bool IsMaxed(List<string> completedIds)
        {
            return datas.All(d => completedIds.Contains(d.GetId));
        }

        public float GetTotalMoneyMult(List<string> completedIds)
        {
            return datas.Where(d => completedIds.Contains(d.GetId)).Sum(d => d.moneyMult);
        }

        public int GetTotalSlotUnlock(List<string> completedIds)
        {
            return datas.Where(d => completedIds.Contains(d.GetId)).Sum(d => d.slotUnlock);
        }


        public RebirthDataSO GetCurrentRebirth(int currentIndex)
        {
            if (currentIndex < 0) return null;
            if (currentIndex >= datas.Count) return null;
            return datas[currentIndex];
        }
    }
}
