using TMPro;
using UnityEngine;

public class IncomeInSlot : MonoBehaviour
{
    public TextMeshProUGUI txtIncome;
    public Transform holderCanvas;
    public float x = 1.5f;
    public void SetTextIncome(string text)
    {
        txtIncome.text = text + "$"; 
    }

    public void SetPosCanvas(int direc)
    {
        holderCanvas.localPosition = new Vector3(x * direc, 0, 0);
    }
}
