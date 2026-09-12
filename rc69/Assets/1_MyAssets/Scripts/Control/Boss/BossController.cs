using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public BossDataSO data;
    public GoapAgent goapAgent;
    public WorldState worldState;

    public BossHandle bosshandle;

    public void Init(Transform stayTransform)
    {
        bosshandle = new BossHandle(stayTransform,goapAgent,SetWorldStateData);
        goapAgent.SetStart(true);
        
    }

    
    private void Update()
    {
        bosshandle.Update();
    }

    void SetWorldStateData(GoapWorldStateEnum key, bool value)
    {
        worldState.Data.Set(key.ToString(), value);
    }

    public List<Transform> GetListChase()
    {
        return bosshandle.listTarget;
    }
}

public enum BossState
{
    Normal,
    EggStealed,
}
