using Raccoon.Controller;
using Raccoon.EnumHolder;
using Raccoon.Item;
using UnityEngine;
using UnityEngine.Events;

public class TutLootItem : TutorialAction
{
    ItemCtr item = null;
    Transform startPos;
    public override void Init(UnityAction<Transform> actionSetup)
    {
        base.Init(actionSetup);
        item = null;
        startPos = HomeCtr.instance.GetStartPos(PlayerController.instance.transform);
    }
    public override bool SetUp()
    {

        if(target != null)
        {
            if (target == startPos)
            {
                if(!PlayerController.instance.loot.IsHaveItemLoot()) target = null;
            }
            else
            {
                if (PlayerController.instance.loot.IsHaveItemLoot()) target = null;
                if(target != null)
                {
                    if (item == null) target = null;
                    else if (!item.gameObject.activeSelf || item.GetState == ItemState.Loot) target = null;
                }
            }
        }
        if(target == null)
        {
            if (!PlayerController.instance.loot.IsHaveItemLoot())
            {
                item = GamePlayControl.instance.GetRandomItemInSpawnItem(1);
                if(item != null)
                {
                    target = item.transform;
                }
            }
            else
            {
                target = startPos;
            }
        }

        if(target != null) actionSetup?.Invoke(target);

        return target != null;
    }
}
