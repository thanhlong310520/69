using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public BossDataSO data;
    public GoapAgent goapAgent;
    public WorldState worldState;

    public BossInSteal bossInSteal;

    public void Init(Transform stayTransform)
    {
        bossInSteal = new BossInSteal(stayTransform,goapAgent,SetWorldStateData);
        goapAgent.SetStart(true);
        
    }

    
    private void Update()
    {
        bossInSteal.Update();
    }

    void SetWorldStateData(GoapWorldStateEnum key, bool value)
    {
        worldState.Data.Set(key.ToString(), value);
    }

    public List<Transform> GetListChase()
    {
        return bossInSteal.listStealer;
    }
}

public enum BossState
{
    Normal,
    EggStealed,
}
