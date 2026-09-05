using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raccoon.Data
{
    [CreateAssetMenu(fileName = "rebirth_", menuName = "Raccoon/Rebirth/RebirthDataSO")]
    public class RebirthDataSO : ScriptableObject
    {
        public string id;
        public float moneyMult;
        public int slotUnlock;
        public double requireMoneyBase;
        public int requireMoneyExponent;
        public int requireWorkerCount;
        public List<BotDataSO> requireWorkers = new();

        public string GetId => string.IsNullOrEmpty(id) ? name : id;

        public long GetRequireMoney()
        {
            double result = requireMoneyBase * Math.Pow(1000, requireMoneyExponent);
            return result > long.MaxValue ? long.MaxValue : (long)result;
        }

        public bool HasRequiredWorkers(List<string> slottedBotIds)
        {
            if (requireWorkers == null || requireWorkers.Count == 0)
                return slottedBotIds.Count >= requireWorkerCount;
            return requireWorkers.All(bot => bot != null && slottedBotIds.Contains(bot.GetId));
        }

        public bool MeetRequirement(long playerCoin, List<string> slottedBotIds)
        {
            return HasRequiredWorkers(slottedBotIds) && playerCoin >= GetRequireMoney();
        }
    }
}
