using UnityEngine;
using UnityEngine.Events;
public class CalculateTime : MonoBehaviour
{
    ////income
    private float incomeTimer;
    private float timeInterval = 1f; // every 1 second=

    public UnityAction EventUpdateTime;
    float bonus = 1;

    bool canCalculate = false;
    public void OnCaclulate()
    {
        ResetTime();

        canCalculate = true;
    }
    public void SetMultiSpeed(float multy)
    {
        this.bonus = multy; 
    }

    public void ResetTime()
    {
        canCalculate = false;
        incomeTimer = 0;

        SetMultiSpeed(1);

    }
    public void SetActionCount(UnityAction action)
    {
        EventUpdateTime = action; 
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

        if (incomeTimer >= timeInterval)
        {
            incomeTimer = 0f;
            UpdateIncome();
        }
    }

    void UpdateIncome()
    {
        EventUpdateTime?.Invoke();
    }
}
