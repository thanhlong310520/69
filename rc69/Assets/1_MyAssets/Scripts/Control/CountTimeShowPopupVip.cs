using Raccoon;
using Raccoon.Controller;
using UnityEngine;

public class CountTimeShowPopupVip : MonoBehaviour
{
    public float timeShow = 600;
    public CountTime ct;

    public void StartCountShow()
    {
        //if (PlayerData.Get.isVipIAP) return;

        //if (ct == null) ct = GetComponent<CountTime>();
        //if (ct == null) ct = gameObject.AddComponent<CountTime>();
        //ct.Init(timeShow, null, Show);
    }

    public void Show()
    {
        if (PlayerData.Get.isVipIAP) return;
        GameData.Get.isShowVip = true;
        GamePlayControl.instance.ShowPopup(PopupCanvas.PopupType.VipSub);
    }
}
