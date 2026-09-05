using UnityEngine;

public class SpawnCtrl : MonoBehaviour
{
    public static SpawnCtrl instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


}
