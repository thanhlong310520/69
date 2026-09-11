using DG.Tweening;
using Raccoon.Bot;
using Raccoon.Data;
using Raccoon.Helper;
using Raccoon.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Raccoon.UI
{
    public class UICanvasInfo : MonoBehaviour
    {
        public TextMeshProUGUI txtName, txtRarity, txtMoreInfor, txtCost, txtRainbow;
        public RainbowScrollText rainbowEffect;

        public TextMeshProUGUI txtCountTime;
        public void SetUIView(ItemDataSO dt, float scale)
        {
            txtName.text = txtCost.text = txtMoreInfor.text = txtRarity.text = string.Empty;
            
            gameObject.SetActive(true);
            txtName.text = dt.itemName;
            GameUtils.SetRarityText(dt.rarityType, txtRarity);

            //rainbowEffect.enabled = data.colorType == ColorType.Rainbow;

            txtName.color = GameUtils.GetColorByColorType(dt.colorType);
            //txtRainbow?.gameObject.SetActive(data.colorType == ColorType.Rainbow);

            txtCost.text = $"{GameUtils.ConvertCurrencyToString(dt.cost)}/s";

            

        }

        public void SetTimeCount(float time)
        {
            int t = Mathf.RoundToInt(time);
            txtCountTime.text = t.ToString() + "s";
        }

        public void SetActiveTxtCountTime(bool isActive)
        {
            txtCountTime.gameObject.SetActive(isActive);
        }

        public void SetActiveInfoInSlot()
        {
            txtCost.gameObject.SetActive(true);
            txtMoreInfor.gameObject.SetActive(true);
            txtRarity.gameObject.SetActive(true);
            txtName.gameObject.SetActive(true);
            txtRainbow.gameObject.SetActive(false);
            SetActiveTxtCountTime(false);
        }


        public void ShowMoreText(string infor)
        {
            txtMoreInfor.text = infor;

        }



    }
}