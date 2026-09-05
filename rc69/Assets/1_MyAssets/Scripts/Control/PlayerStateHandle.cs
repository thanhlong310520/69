using Raccoon.Building;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Item;
using Raccoon.Player;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStateHandle : MonoBehaviour
{
    EPlayerState state;
    public EPlayerState GetPlayerState => state;


    //
    public PlayerHandleSlot playerSlot;
    public PlayerAttack attack;
    public PlayerAnim playerAnim;

    public Transform holderItem;
    ItemCtr currentItem;
    GameObject model;
    public ItemCtr GetCurrentItem => currentItem;

    public UnityEvent EventResetItem;

    UnityAction<SlotController, ItemCtr> actionPickupFromSlot;
    UnityAction<SlotController, ItemCtr> actionPutToSlot;
    UnityAction<SlotController, ItemCtr> actionStealSlot;

    public UnityEvent eventDoneAttack;

    UnityAction<ItemType> actionChangeItem;
    public void Init(Rigidbody rb, UnityAction<SlotController, ItemCtr> actionPickupFromSlot , UnityAction<SlotController, ItemCtr> actionPutToSlot,UnityAction<SlotController, ItemCtr> actionSteal, UnityAction<ItemType> actionChangeItem)
    {
        this.actionPickupFromSlot = actionPickupFromSlot;
        this.actionPutToSlot = actionPutToSlot;
        this.actionChangeItem = actionChangeItem;
        this.actionStealSlot = actionSteal;

        playerAnim?.Init(rb);
        playerSlot.Init(PickUpFormSlot, PutToSlot,Steal);
        attack.Init(playerAnim);

        playerAnim.ActionDoneAttack = DoneAttack;
    }

    public void ChangeState(EPlayerState state)
    {
        if (this.state == state) return;

        this.state = state;
    }

    public void ChangeItem(ItemConfig config)
    {
        if (currentItem != null) GamePlayControl.instance.RespawnItem(currentItem.gameObject);

        currentItem = null;
        if (config != null)
        {
            ItemDataSO data = GamePlayControl.instance.GetItemDataSO(config.id);
            if (data != null)
            {
                GameObject itemObj = GamePlayControl.instance.SpawnItem(data.prefab);
                itemObj.transform.localPosition = Vector3.zero;
                itemObj.transform.localRotation = Quaternion.identity;
                currentItem = itemObj.GetComponent<ItemCtr>();
                currentItem.Init(data, config, Raccoon.EnumHolder.ItemState.Save, transform);
                currentItem.gameObject.SetActive(true);
            }
        }
        ChangeStateWhenChangeItem();
    }

    void ChangeStateWhenChangeItem()
    {
        EPlayerState state = EPlayerState.Normal;
        ItemType typeItem = ItemType.None;
        if(currentItem != null)
        {
            typeItem = currentItem.data.itemType;
            if (currentItem.data.itemType == ItemType.Brainrot || currentItem.data.itemType == ItemType.OtherItem) state = EPlayerState.HoldItemBot;

            currentItem.SetTarget(holderItem);
        }
        actionChangeItem?.Invoke(typeItem);
        ChangeLayerItem(typeItem);
        ChangeState(state);
    }
    public void ChangeLayerItem(ItemType type)
    {
        int layer = 0; 
        if(type == ItemType.Weapon || type == ItemType.OtherWeapon) layer = 1;
        if(type == ItemType.Brainrot || type == ItemType.OtherItem) layer = 2;
        playerAnim.SetWeightAnimator(layer, 1);
    }
    public void ResetItem()
    {
        ChangeItem(null);
        EventResetItem?.Invoke();
    }
    public bool DropItem(bool activeAbility, UnityAction<ItemCtr> actionDropItem)
    {
        if (currentItem == null) return false;
        if (currentItem.data.itemType == ItemType.OtherItem || currentItem.data.itemType == ItemType.Brainrot)
        {
            var item = currentItem;
            currentItem.Drop(false, activeAbility);
            item.transform.position = transform.position;

            currentItem = null;
            ResetItem();
            actionDropItem?.Invoke(item);
            HolderItemDropped.instance.AddItem(item);
            return true;
        }
        return false;
    }

    public void Attack()
    {
        if (currentItem == null) return;
        if (currentItem.data.itemType == ItemType.Weapon) attack.StartAttack();

    }
    void DoneAttack()
    {
        attack.DoneAttack();
        eventDoneAttack?.Invoke();
    }

    public void ChangeModel(GameObject modelPrefab)
    {
        if (model != null) Destroy(model);
        model = Instantiate(modelPrefab, transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        Animator animator = model.GetComponentInChildren<Animator>();

        playerAnim.ChangedAnimator(animator, ActionAttack);
        holderItem = playerAnim.GetHolderItem();

        ChangeStateWhenChangeItem();
    }
    void ActionAttack()
    {
        if (currentItem == null) return;
        currentItem.GetComponent<Weapon>()?.Attack();
    }

    public void HandleSlot(SlotController curentSlot,TypeHandleSlot type)
    {
        playerSlot.Handle(curentSlot, currentItem, type);
    }
    void PickUpFormSlot(SlotController slot,ItemCtr item)
    {
        actionPickupFromSlot?.Invoke(slot,item);
    }
    void PutToSlot(SlotController slot, ItemCtr item)
    {
        actionPutToSlot?.Invoke(slot,item);
        currentItem = null;
        ResetItem();
    }
    void Steal(SlotController slot, ItemCtr item)
    {
        actionStealSlot?.Invoke(slot, item);
    }

    public void LootHandle()
    {
        ResetItem();
        ChangeLayerItem(ItemType.Brainrot);
    }

    public void Die()
    {
        ResetItem();
        ChangeState(EPlayerState.Dead);
        playerAnim.SetAnimationKnockback();
    }

}
