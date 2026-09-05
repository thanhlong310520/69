using Raccoon;
using Raccoon.Ads;
using Raccoon.Controller;
using UnityEngine;

public class ShieldHomePlayer : MonoBehaviour
{
    #region singleton
    public static ShieldHomePlayer instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public HomePlayerContact homePlayerContact; 
    public GameObject vfx;
    public CountTime countTime;
    public float timeShield = 60f;
    public bool IsShield => isShield;
    bool isShield;
    private void Start()
    {
        vfx.SetActive(false);
        isShield = false;
    }

    public void OnShied()
    {

        AdsActionManager.instance.ShowRewardAds((success) =>
        {
            if (success)
            {
                NpcBackBase();
                StartShield();
                
            }
        });
    }

    void NpcBackBase()
    {
        foreach (var npc in homePlayerContact.npcsInPlayerHome)
        {
            npc.BackBase();
        }
    }
    void StartShield()
    {
        ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.Shield);
        isShield = true;
        countTime.Init(timeShield,null,EndShield);
        vfx.SetActive(isShield);
    }

    void EndShield()
    {
        isShield = false;
        vfx.SetActive(false);
    }
}
