using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public BossDataSO data;
    public GoapAgent goapAgent;
    public WorldState worldState;
    public AiMovement aiMove; 
    public BossHandle bosshandle;

    public void Init(BossDataSO data, Transform stayTransform)
    {
        this.data = data;
        bosshandle = new BossHandle(stayTransform,goapAgent,SetWorldStateData);
        goapAgent.SetStart(true);
        aiMove.SetSpeed(data.speed);
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
