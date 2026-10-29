

using Raccoon.Controller;
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
        public Transform canvasinfor;
        public override void Init(ItemDataSO data, ItemConfig config, ItemState state, Transform user = null)
        {
            base.Init(data, config, state, user);

            var dt = data as BotDataSO;
            float income = 0;
            if (dt != null) income = dt.income;

            var incomeReward = Mathf.RoundToInt(income);
            string moreTxt = $"{GameUtils.ConvertCurrencyToString(incomeReward)}/s";
            if (infor != null)
            {
                infor.ShowTextBot();
                infor.ShowMoreText(moreTxt);
            }
            Vector3 modelScale = holderModel != null ? holderModel.localScale : Vector3.one;
            if(vfx == null && VfxCtrl.instance != null && GamePlayControl.instance != null)
            {
                var prefab = VfxCtrl.instance.GetVfxBrainrot(data.rarityType);
                if (prefab != null) vfx = GamePlayControl.instance.SpawnItem(prefab);
                if (vfx != null)
                {
                    vfx.transform.SetParent(transform);
                    vfx.transform.localPosition = Vector3.zero;
                    vfx.transform.localScale = modelScale / 2;
                }
                else
                {
                    Debug.LogWarning($"[BotControl] Missing VFX for rarity {data.rarityType} ({data.name})");
                }
            }
            if (canvasinfor != null) canvasinfor.localPosition = Vector3.up * 2 * modelScale.x;
        }
        public override void AddTimeInSlot(float time)
        {
        }
    }

}
