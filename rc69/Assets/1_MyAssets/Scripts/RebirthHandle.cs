using Raccoon;
using Raccoon.Building;
using Raccoon.Controller;
using Raccoon.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RebirthHandle : MonoBehaviour
{

    public float timeRebirth = 2f;
    float currentTimeRebirth = 0;
    public GameObject vfx;
    public UnityEvent<bool> eventSetup;
    bool hasRebirth = false;
    public bool GetHasRebirth => hasRebirth;

    bool isChangeCam = false;
    private void Start()
    {
        isChangeCam = false;
        vfx.SetActive(false);

    }

    public void Rebirth(CharactorData data, bool isAds)
    {
        if (!isAds)
        {
            if (!hasRebirth) return;
        }

        if (isChangeCam) return;

        RebirthDataSO rebirthSO = GameData.Get.GetNextRebirth(data.GetIndexRebirth());
        print("index rebirth " + data.GetIndexRebirth());
        if (rebirthSO == null) return;
        float multi = 1;
        if (rebirthSO != null)
        {
            multi = rebirthSO.moneyMult;
        }
        Home home = HomeCtr.instance.GetHomeByUser(PlayerController.instance.transform);
        HomeCtr.instance.OnSetMultiIncome(home, multi);

        SlotController slotLock = home.GetSlotLock();
        if (slotLock != null)
        {
            HomeCtr.instance.UnlockSlotInGate2(PlayerController.instance.transform, slotLock.id);
            data.UnlockSlotGate2(slotLock.id);
            ChangeCam(slotLock.transform);
            ShowVfx(slotLock.transform.position);
        }

        long price = rebirthSO.GetRequireMoney();
        List<string> id = new List<string>();
        foreach (var so in rebirthSO.requireWorkers)
        {
            if (so != null) id.Add(so.GetId);
        }
        PlayerController.instance.Rebirth(price, id, isAds);


        CheckHaveRebirth(data);
    }


    void ChangeCam(Transform target)
    {
        GamePlayControl.instance.SetCanAction(false);
        isChangeCam = true;
        GamePlayControl.instance.mainCam.enabled = false;
        GamePlayControl.instance.rebirthCam.enabled = true;
        GamePlayControl.instance.LookatTarget(GamePlayControl.instance.rebirthCam.transform, target, Vector3.up);
        currentTimeRebirth = 0;
    }

    void DoneChangeCam()
    {
        isChangeCam = false;
        GamePlayControl.instance.mainCam.enabled = true;
        GamePlayControl.instance.rebirthCam.enabled = false;
        currentTimeRebirth = 0;
        GamePlayControl.instance.SetCanAction(true);
        vfx.SetActive(false);
    }
    void ShowVfx(Vector3 pos)
    {
        vfx.SetActive(true);
        vfx.transform.position = pos;
    }
    public void CheckHaveRebirth(CharactorData data)
    {
        hasRebirth = false;
        if (GameData.Get == null) return;
        RebirthDataSO rebirthSO = GameData.Get.GetNextRebirth(data.GetIndexRebirth());
        if (rebirthSO == null) return;
        hasRebirth = rebirthSO.MeetRequirement(data.GetCoin, data.GetAllIdItem());

        eventSetup?.Invoke(hasRebirth);
    }

    private void Update()
    {
        if (!isChangeCam) return;

        currentTimeRebirth += Time.deltaTime;
        if (currentTimeRebirth >= timeRebirth) DoneChangeCam();
    }
}
