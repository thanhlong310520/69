using Raccoon.Controller;
using Raccoon.Utils;
using TMPro;
using UnityEngine;

public class UpdateTreamill : GoContact
{
    public TMP_Text txtPrice;
    public TMP_Text txtLevel;
    public Color colorEnough = Color.white;
    public Color colorNotEnough = Color.red;

    CharactorData data;
    long nextPrice = -1;

    protected override void Start()
    {
        base.Start();
        ActionShowWaitOpen = MainUiControl.instance.ShowWaitToUpdate;
        ActionChangeAmountWait = MainUiControl.instance.ShowAmountWait;
    }
    public void SetPrice(CharactorData data)
    {
        if (data == null) return;
        SetLevel(data);
        if (txtPrice == null) return;

        if (this.data != data)
        {
            if (this.data != null) this.data.onChangeCoin -= OnChangeCoin;
            this.data = data;
            this.data.onChangeCoin += OnChangeCoin;
        }

        var listTreamill = GamePlayControl.instance.GetTreamillDataSOs;
        int nextIndex = data.GetIndexTreamill() + 1;
        if (listTreamill == null || nextIndex >= listTreamill.Count)
        {
            nextPrice = -1;
            txtPrice.text = "MAX";
            txtPrice.color = colorEnough;
            return;
        }
        nextPrice = listTreamill[nextIndex].price;
        txtPrice.text = nextPrice.ConvertCurrencyToString();
        OnChangeCoin(data.GetCoin);
    }

    void SetLevel(CharactorData data)
    {
        if (txtLevel == null) return;
        var listTreamill = GamePlayControl.instance.GetTreamillDataSOs;
        int nextIndex = data.GetIndexTreamill() + 1;
        if (listTreamill == null || nextIndex >= listTreamill.Count || listTreamill.Count == 0)
        {
            txtLevel.text = $"Max";
        }
        else
        {
            int index = Mathf.Clamp(data.GetIndexTreamill(), 0, listTreamill.Count - 1);
            txtLevel.text = $"Lv.{nextIndex} => Lv.{nextIndex + 1}";
        }
    }

    void OnChangeCoin(long coin)
    {
        if (txtPrice == null || nextPrice < 0) return;
        txtPrice.color = coin >= nextPrice ? colorEnough : colorNotEnough;
    }

    private void OnDestroy()
    {
        if (data != null) data.onChangeCoin -= OnChangeCoin;
    }

    public override void HandleStartInteract(Transform contactor)
    {
        if (contactor != user) return;

        PlayerController player = user.GetComponent<PlayerController>();
        if (player == null || !player.EnoughCoinUpdateTreamill()) return;

        base.HandleStartInteract(contactor);
    }

    protected override void DoneWait()
    {
        PlayerController player = user.GetComponent<PlayerController>();
        if (player == null) return;
        player.UpdateTreamill();
    }
}
