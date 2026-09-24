

using Raccoon.Data;
using Raccoon.EnumHolder;
using Raccoon.Item;
using Raccoon.UI;
using Raccoon.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Raccoon.Bot
{
    public class BotControl : ItemCanStaySlot
    {

        public override void Init(ItemDataSO data, ItemConfig config, ItemState state, Transform user = null)
        {
            base.Init(data, config, state, user);

            var dt = data as BotDataSO;
            float income = 0;
            if (dt != null) income = dt.income;

            var incomeReward = Mathf.RoundToInt(income);
            string moreTxt = $"{GameUtils.ConvertCurrencyToString(incomeReward)}/s";
            infor.ShowTextBot();
            infor.ShowMoreText(moreTxt);
        }
        public override void AddTimeInSlot(float time)
        {
        }
    }

}
