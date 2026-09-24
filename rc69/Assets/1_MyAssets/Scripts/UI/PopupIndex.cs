using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupIndex : PopupCanvas
{
    public InforIndex inforPrefab;
    public Transform content;

    public SnapScrollView snapScrollView;

    public TMP_Text coinRewardTxt;
    public TMP_Text speedRewardTxt;
    public Button btnClaim;
    public TMP_Text totalUnlockTxt;
    public Slider totalUnlockProgress;
    [Header("Preview")]
    public Image previewIcon;
    public TextMeshProUGUI previewNameTxt;
    public TextMeshProUGUI previewIncomeTxt;
    public TextMeshProUGUI previewRarityTxt;
    InforIndex currentInfor;
    ItemIndex currentItemIndex;

    public List<InforIndex> items;
    bool inited = false;



    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        snapScrollView.StartIndex = 0;
        snapScrollView.enabled = false;
        base.Show(p, afterPopup, obj);
        if (!inited)
        {
            InitListInfor();
        }
        ShowTotalUnlock();
    }
    public override void DoneShow()
    {
        base.DoneShow();
        snapScrollView.enabled = true;
    }
    void InitListInfor()
    {
        inited = true;
        foreach (ZoneType type in Enum.GetValues(typeof(ZoneType)))
        {
            SpawnInfor(type);
        }
    }
    protected InforIndex SpawnInfor(ZoneType type)
    {
        var newInfor = Instantiate(inforPrefab, content);
        newInfor.transform.localScale = Vector3.one;
        newInfor.Init(type, OnSelectedInfor, ShowPreview);
        items.Add(newInfor);
        return newInfor;
    }
    public void UpdateItem(ItemDataSO data)
    {
        if (!inited) InitListInfor();
        var itemindex = items.FirstOrDefault(i => i.ZoneType == data.colorType);

        if (itemindex != null) itemindex.OnUnlockItem(data);
        ShowTotalUnlock();
    }

    /// <summary>
    /// Hiển thị tổng số bot đã mở trên tổng số bot của tất cả các zone.
    /// </summary>
    public void ShowTotalUnlock()
    {
        int total = 0;
        int unlocked = 0;
        var data = PlayerController.instance != null ? PlayerController.instance.GetCharactorData : null;
        foreach (ZoneType type in Enum.GetValues(typeof(ZoneType)))
        {
            var listBotInZone = GamePlayControl.instance.GetBotDataInZone(type);
            total += listBotInZone.Count;
            if (data == null || data.listBotUnlock == null) continue;
            if (!data.listBotUnlock.TryGetValue(type, out var botUnlock) || botUnlock == null) continue;
            unlocked += listBotInZone.Count(b => botUnlock.listUnlock.Contains(b.GetId));
        }

        if (totalUnlockTxt != null) totalUnlockTxt.text = $"Unlocked! {unlocked}/{total}";
        if (totalUnlockProgress != null) totalUnlockProgress.value = total > 0 ? (float)unlocked / total : 0;
    }


    /// <summary>
    /// Hiển thị preview của ItemDataSO: icon, tên, income, rarity.
    /// </summary>
    public void ShowPreview(ItemDataSO so, bool isUnlock = true)
    {
        if (so == null) return;
        if (previewIcon != null)
        {
            previewIcon.sprite = so.spriteIcon;
            previewIcon.color = isUnlock ? Color.white : Color.black;
        }
        if (!isUnlock)
        {
            // Chưa unlock: hiển thị giống ItemIndex bị khoá
            if (previewNameTxt != null) previewNameTxt.text = "?????";
            if (previewIncomeTxt != null) previewIncomeTxt.text = "?????";
            if (previewRarityTxt != null)
            {
                previewRarityTxt.text = "?????";
            }
            return;
        }
        if (previewNameTxt != null) previewNameTxt.text = so.itemName;
        if (previewIncomeTxt != null)
        {
            var bot = so as BotDataSO;
            previewIncomeTxt.text = bot != null ? $"{bot.income.ConvertCurrencyToString()}/s" : "";
        }
        previewRarityTxt.text = so.rarityType.ToString();
        
        //GameUtils.SetRarityText(so.rarityType, previewRarityTxt);
    }

    /// <summary>
    /// Preview theo ItemIndex, tự ẩn thông tin nếu chưa unlock.
    /// </summary>
    public void ShowPreview(ItemIndex itemIndex)
    {
        // Tắt highlight ItemIndex đã chọn trước đó, bật cho ItemIndex mới
        if (currentItemIndex != null && currentItemIndex != itemIndex) currentItemIndex.SetSelected(false);
        currentItemIndex = itemIndex;
        if (itemIndex == null) return;
        itemIndex.SetSelected(true);
        ShowPreview(itemIndex.GetData, itemIndex.IsUnlock);
    }

    public void OnSelectedInfor(InforIndex inforIndex)
    {
        currentInfor = inforIndex;
        bool canClaim;
        var data = PlayerController.instance != null ? PlayerController.instance.GetCharactorData : null;
        if (data != null && data.listBotUnlock != null
            && data.listBotUnlock.TryGetValue(inforIndex.ZoneType, out var botUnlock)
            && botUnlock != null && botUnlock.claim)
            canClaim = false;
        else
            canClaim = CheckClaim(inforIndex);

        if (btnClaim != null) btnClaim.interactable = canClaim;
        ShowIndexReward(inforIndex);

        // Chọn ItemIndex đầu tiên của zone để preview
        var firstItem = inforIndex.listItemIndex != null ? inforIndex.listItemIndex.FirstOrDefault() : null;
        ShowPreview(firstItem);
    }

    /// <summary>
    /// Nhận IndexReward của zone đang chọn và đánh dấu đã claim.
    /// </summary>
    public void OnClickClaim()
    {
        if (currentInfor == null || PlayerController.instance == null) return;
        if (!CheckClaim(currentInfor)) return;

        var data = PlayerController.instance.GetCharactorData;
        var botUnlock = data.listBotUnlock[currentInfor.ZoneType];
        if (botUnlock.claim) return;

        var reward = GamePlayControl.instance.GetIndexRewardByZone(currentInfor.ZoneType);
        if (reward != null && reward.coin > 0) PlayerController.instance.Income(reward.coin);
        if (reward != null && reward.speed != 0)
        {
            data.multybuytreaMill += reward.speed;
            var speedUpgrade = GamePlayControl.instance.GetUpgradeDataByType(TypeUpgrade.Speed);
            if (speedUpgrade != null) PlayerController.instance.SetDataUpgrade(speedUpgrade);
        }

        botUnlock.Claim();
        if (btnClaim != null) btnClaim.interactable = false;
    }

    /// <summary>
    /// Hiển thị IndexReward của zone được chọn.
    /// </summary>
    void ShowIndexReward(InforIndex inforIndex)
    {
        if (inforIndex == null) return;
        var reward = GamePlayControl.instance.GetIndexRewardByZone(inforIndex.ZoneType);
        if (reward == null)
        {
            if (coinRewardTxt != null) coinRewardTxt.text = "";
            if (speedRewardTxt != null) speedRewardTxt.text = "";
            return;
        }
        if (coinRewardTxt != null) coinRewardTxt.text = reward.coin.ConvertCurrencyToString();
        if (speedRewardTxt != null) speedRewardTxt.text = $"+{reward.speed}";
    }

    /// <summary>
    /// Kiểm tra số bot đã unlock trong data của player thuộc zone của inforIndex
    /// đã bằng tổng số botDataSO của zone đó chưa.
    /// </summary>
    bool CheckClaim(InforIndex inforIndex)
    {
        if (inforIndex == null || PlayerController.instance == null) return false;
        var data = PlayerController.instance.GetCharactorData;
        if (data == null || data.listBotUnlock == null) return false;

        ZoneType zone = inforIndex.ZoneType;
        var listBotInZone = GamePlayControl.instance.GetBotDataInZone(zone);
        if (listBotInZone.Count == 0) return false;

        if (!data.listBotUnlock.TryGetValue(zone, out var botUnlock) || botUnlock == null) return false;

        int unlocked = listBotInZone.Count(b => botUnlock.listUnlock.Contains(b.GetId));
        return unlocked >= listBotInZone.Count;
    }
}
