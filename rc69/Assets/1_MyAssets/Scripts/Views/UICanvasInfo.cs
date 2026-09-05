using System;
using DG.Tweening;
using Raccoon.Bot;
using Raccoon.Data;
using Raccoon.Helper;
using Raccoon.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Raccoon.UI
{
    public class UICanvasInfo : MonoBehaviour
    {
        public TextMeshProUGUI txtName, txtRarity, txtIncome, txtCost, txtRainbow;
        public RainbowScrollText rainbowEffect;

        public TextMeshProUGUI txtCountTime;
        public void SetUIView(ItemDataSO dt, float scale)
        {
            txtCost.gameObject.SetActive(true);
            txtIncome.gameObject.SetActive(true);
            txtRarity.gameObject.SetActive(true);
            txtName.gameObject.SetActive(true);
            txtRainbow.gameObject.SetActive(true);
            var data = dt as BotDataSO;
            txtName.text = txtCost.text = txtIncome.text = txtRarity.text = string.Empty;
            if (data == null)
            {
                gameObject.SetActive(false);
                txtName.text = txtCost.text = txtIncome.text = txtRarity.text = string.Empty;
                return;
            }
            gameObject.SetActive(true);
            txtName.text = data.itemName;
            GameUtils.SetRarityText(data.rarityType, txtRarity);
            rainbowEffect.enabled = data.colorType == ColorType.Rainbow;
            txtName.color = GameUtils.GetColorByColorType(data.colorType);
            txtRainbow?.gameObject.SetActive(data.colorType == ColorType.Rainbow);

            txtCost.text = $"{GameUtils.ConvertCurrencyToString(data.cost)}/s";

            var incomeReward = Mathf.RoundToInt(data.income * scale);
            txtIncome.text = $"{GameUtils.ConvertCurrencyToString(incomeReward)}/s";

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
            txtCost.gameObject.SetActive(false);
            txtIncome.gameObject.SetActive(true);
            txtRarity.gameObject.SetActive(false);
            txtName.gameObject.SetActive(true);
            txtRainbow.gameObject.SetActive(false);
            SetActiveTxtCountTime(false);
        }



    }
}