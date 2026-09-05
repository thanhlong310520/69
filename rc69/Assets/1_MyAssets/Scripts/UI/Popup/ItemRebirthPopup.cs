using Raccoon.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemRebirthPopup : MonoBehaviour
{
    public Sprite uncollectCheck;
    public Sprite collectCheck;
    public Image check;
    public Image bg;
    public Image icon;
    public TMP_Text nameBot;

    public Sprite uncollectBG;
    public Sprite collectBG;

    public void Setup(BotDataSO data, bool collect)
    {
        if (collect)
        {
            check.sprite = collectCheck;
            bg.sprite = collectBG;
        }
        else
        {
            check.sprite = uncollectCheck;
            bg.sprite = uncollectBG;
        }
        icon.sprite = data.spriteIcon;
        nameBot.text = data.itemName;
    }
}
