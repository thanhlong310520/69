using NUnit.Framework.Interfaces;
using Raccoon.Bot;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.EnumHolder;
using Raccoon.Item;
using Raccoon.Utils;
using UnityEngine;
using UnityEngine.Events;

public class EggControl : ItemCanStaySlot
{
    bool canAdd = true;
    EggDataSO eggData;
    UnityAction<EggControl> resetTransformAction;
    CountTime countTime;
    float timeReset = 60f;
    public override void Init(ItemDataSO data, ItemConfig config, ItemState state, Transform user = null)
    {
        base.Init(data, config, state, user);

        eggData = data as EggDataSO;
        string moreTxt = $"";
        if (eggData != null)
        {
            moreTxt = eggData.growTime.ToSeconds().FormatTimeSecond(true);
        }
        if(GetIsSave)
        {
            infor.ShowTextEggSave();
            infor.ShowMoreText(moreTxt);
        }
        else
        {
            infor.ShowTextEggSpawn();
        }
        canAdd = true;
    }

    public void SetEventReset(UnityAction<EggControl> resetTransformAction)
    {
        this.resetTransformAction = resetTransformAction;
        InitCountTime();
    }

    void InitCountTime()
    {
        if(countTime == null) countTime = gameObject.GetComponent<CountTime>();
        if(countTime == null) countTime = gameObject.AddComponent<CountTime>();
        countTime.Init(timeReset, ActionShowTime, ActionDoneShow);
        countTime.SetCanCount(false);
    }

    void ActionShowTime(float time)
    {
        infor.ShowTextCountEgg(time);
    }

    void ActionDoneShow()
    {
        if (GetIsSave) return;
        resetTransformAction?.Invoke(this);
        InitCountTime();
        infor.SetActiveTxtCountTime(false);
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


    public override void Stealed(Transform target, Transform stealer)
    {
        base.Stealed(target, stealer);
        countTime?.SetCanCount(false);
        infor.SetActiveTxtCountTime(false);
    }

    public override void StealFail(Transform stealer)
    {
        base.StealFail(stealer);
        countTime?.SetCanCount(true);
        infor.SetActiveTxtCountTime(true);
    }


}
