using Raccoon.Building;
using Raccoon.Controller;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TutUpgrade : TutorialAction
{
    List<UpgradeDataSO> upgrades;
    bool have = false;
    public override void Init(UnityAction<Transform> actionSetup)
    {
        base.Init(actionSetup);
        upgrades = GamePlayControl.instance.GetUpgradeDataSOs;
        target = GamePlayControl.instance.GetUpgradeShop;
        have = false;
    }
    public override bool SetUp()
    {
        if (target == null)
        {
            target = GamePlayControl.instance.GetUpgradeShop;
        }
        if (target == null) return false;
        if (!have) GetHaveUpgrade();
        if (have) actionSetup?.Invoke(target);
        return have;
    }

    public void GetHaveUpgrade()
    {
        foreach (var u in upgrades)
        {
            var listId = PlayerController.instance.GetCharactorData.upgrade.GetDicUpgraded(u.type);
            foreach (var item in u.data)
            {
                if (!listId.Contains(item.id))
                {
                    if (u.GetPriceUpgrade(item.id) <= PlayerController.instance.GetCharactorData.GetCoin)
                    {
                        have = true;
                        break;
                    }
                }
            }
        }

    }
}
