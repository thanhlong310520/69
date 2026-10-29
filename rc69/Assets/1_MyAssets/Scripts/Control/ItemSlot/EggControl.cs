using DG.Tweening;
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

    [Header("Contact shake")]
    [SerializeField] float shakeDuration = 0.3f;
    [SerializeField] float shakeStrength = 0.15f;
    [SerializeField] int shakeVibrato = 10;
    [SerializeField] float popHeight = 0.2f;
    [SerializeField] float popDuration = 0.25f;

    Vector3 holderModelDefaultLocalPos;
    bool holderModelPosCached;

    void CacheHolderModelPos()
    {
        if (holderModel == null) return;
        holderModelDefaultLocalPos = holderModel.localPosition;
        holderModelPosCached = true;
    }

    public override void Init(ItemDataSO data, ItemConfig config, ItemState state, Transform user = null)
    {
        base.Init(data, config, state, user);

        if (holderModel != null)
        {
            holderModel.DOKill();
            if (holderModelPosCached) holderModel.localPosition = holderModelDefaultLocalPos;
            else CacheHolderModelPos();
        }

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

        Vector3 modelScale = holderModel != null ? holderModel.localScale : Vector3.one;
        if (vfx == null && VfxCtrl.instance != null && GamePlayControl.instance != null)
        {
            var prefab = VfxCtrl.instance.GetVfxEgg(data.rarityType);
            if (prefab != null) vfx = GamePlayControl.instance.SpawnItem(prefab);
            if (vfx != null)
            {
                vfx.transform.SetParent(transform);
                vfx.transform.localPosition = Vector3.up * 0.15f;
                vfx.transform.localScale = modelScale / 10;
            }
            else
            {
                Debug.LogWarning($"[BotControl] Missing VFX for rarity {data.rarityType} ({data.name})");
            }
        }
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
        holderModel.DOKill();
        if (holderModelPosCached) holderModel.localPosition = holderModelDefaultLocalPos;
    }

    public override void StealFail(Transform stealer)
    {
        base.StealFail(stealer);
        countTime?.SetCanCount(true);
        infor.SetActiveTxtCountTime(true);
    }


    public override void StartConttact()
    {
        base.StartConttact();

        if (holderModel == null) return;
        if (!holderModelPosCached) CacheHolderModelPos();

        holderModel.DOKill();
        holderModel.localPosition = holderModelDefaultLocalPos;

        Sequence seq = DOTween.Sequence();
        seq.Append(holderModel.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato));
        seq.AppendCallback(() => holderModel.localPosition = holderModelDefaultLocalPos);
        seq.Append(holderModel.DOPunchPosition(Vector3.up * popHeight, popDuration, 1, 0f));
        seq.OnComplete(() => holderModel.localPosition = holderModelDefaultLocalPos);
    }

}
