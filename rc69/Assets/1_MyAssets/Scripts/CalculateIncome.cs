using UnityEngine;
using UnityEngine.Events;
public class CalculateIncome : MonoBehaviour
{
    private long totalIncome;

    public UnityAction<long> EventUpdateIncome;
    float bonus = 1;
    long income;

    public void OnSetDataIncome(long income)
    {
        ResetIncome();
        this.income = income;
    }
    public void SetBonus(float bonus)
    {
        this.bonus = bonus; 
    }

    public void ResetIncome()
    {
        totalIncome = 0;
        SetBonus(1);

    }
    public void SetStartTotalIncome(long startIncome)
    {
        totalIncome = startIncome;
    }
    public void SetActionShowIncome(UnityAction<long> action)
    {
        EventUpdateIncome = action; 
    }

    public void Calculate()
    {
        totalIncome += Mathf.RoundToInt(income * bonus);
        // Update UI

        UpdateIncome();
    }

    public long GetIncome() => totalIncome;
    public void OnCollectIncome()
    {
        totalIncome = 0;
        UpdateIncome();
    }

    void UpdateIncome()
    {
        EventUpdateIncome?.Invoke(totalIncome);
    }
}
