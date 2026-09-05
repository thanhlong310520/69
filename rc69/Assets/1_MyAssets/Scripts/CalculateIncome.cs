using UnityEngine;
using UnityEngine.Events;
public class CalculateIncome : MonoBehaviour
{
    ////income
    private float incomeTimer;
    private float incomeInterval = 1f; // every 1 second
    private long totalIncome;

    public UnityAction<long> EventUpdateIncome;
    float bonus = 1;
    long income;

    bool canCalculate = false;
    public void OnSetDataIncome(long income)
    {
        ResetIncom();
        this.income = income;
        canCalculate = true;
    }
    public void SetBonus(float bonus)
    {
        this.bonus = bonus; 
    }

    public void ResetIncom()
    {
        canCalculate = false;
        incomeTimer = 0;
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
    // Update is called once per frame
    void Update()
    {
        if (!canCalculate) return;
        Calculate();
    }

    private void Calculate()
    {
        // accumulate timer
        incomeTimer += Time.deltaTime;

        if (incomeTimer >= incomeInterval)
        {
            incomeTimer = 0f;

            totalIncome += Mathf.RoundToInt(income * bonus);
            // Update UI

            UpdateIncome();
        }
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
