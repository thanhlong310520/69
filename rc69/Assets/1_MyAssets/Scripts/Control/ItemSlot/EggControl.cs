using NUnit.Framework.Interfaces;
using Raccoon.Bot;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.EnumHolder;
using Raccoon.Item;
using Raccoon.Utils;
using UnityEngine;

public class EggControl : ItemCanStaySlot
{
    bool canAdd = true;
    EggDataSO eggData;
    public override void Init(ItemDataSO data, ItemConfig config, ItemState state, Transform user = null)
    {
        base.Init(data, config, state, user);

        eggData = data as EggDataSO;
        string moreTxt = $"";
        if (eggData != null)
        {
            moreTxt = eggData.growTime.ToSeconds().FormatTimeSecond(true);
        }

        infor.SetActiveInfoInSlot();
        infor.ShowMoreText(moreTxt);
        canAdd = true;
    }
    public override void AddTimeInSlot(float time)
    {
        config.AddTimeInSlot(time);
        if (!canAdd) return;
        if (CheckOver())
        {
            canAdd = false;
            Hatching();
        }
        else
        {
            float t = eggData.growTime.ToSeconds() - config.timeInSlot;
            infor.ShowMoreText(t.FormatTimeSecond(true));
        }
    }

    bool CheckOver()
    {
        if(eggData == null) return false;

        if (eggData.growTime.ToSeconds() <= config.timeInSlot) return true;
        return false;
    }

    void Hatching()
    {
        GameObject item = GamePlayControl.instance.SpawnItem(eggData.monsterSO.prefab);
        item.SetActive(true);
        item.gameObject.SetActive(true);
        var itemCtr = item.GetComponent<ItemCtr>();

        itemCtr.Init(eggData.monsterSO, new ItemConfig(eggData.monsterSO.GetId, eggData.monsterSO.priority), Raccoon.EnumHolder.ItemState.Normal);
        itemCtr.ChangeEnvironment(config.environment);

        actionChangeItem?.Invoke(itemCtr as ItemCanStaySlot);
    }

}
