using Raccoon.Ads;
using Raccoon.EnumHolder;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemEventEnvironment : MonoBehaviour
{
    public Image bg;
    public Image icon;
    public TMP_Text textName;
    public TMP_Text textDescription;
    EnvironmentType type;
    UnityAction<EnvironmentType> actionClick;
    public void Init(EnvironmentType type ,Sprite sbg, Sprite sicon, string sname, string sdescription,UnityAction<EnvironmentType> acitonClick)
    {
        this.type = type;
        this.actionClick = acitonClick;
        bg.sprite = sbg;
        icon.sprite = sicon;
        textName.text = sname;
        textDescription.text = sdescription;
    }

    public void OnClick()
    {
        AdsActionManager.instance.ShowRewardAds((success) =>
        {
            if (success)
            {
                actionClick?.Invoke(type);
            }
        });
    }
}
