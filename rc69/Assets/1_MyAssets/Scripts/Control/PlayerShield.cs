using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    public float timeShield = 15; 
    float currentTimeShield = 0;

    public GameObject vfxShield;

    bool isShield = false;
    public bool IsShield => isShield;

    private void Start()
    {
        isShield = false;
        vfxShield?.SetActive(false);
    }

    public void StartShield()
    {
        currentTimeShield = timeShield;
        vfxShield?.SetActive(true);
        isShield = true;
    }

    private void Update()
    {
        if (isShield)
        {
            currentTimeShield -= Time.deltaTime;
            if(currentTimeShield <= 0)
            {
                EndShield();
            }
        }
    }

    void EndShield()
    {
        isShield = false;
        vfxShield?.SetActive(false);

    }

}
