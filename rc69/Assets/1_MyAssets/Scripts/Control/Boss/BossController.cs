using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    public BossDataSO data;
    public GoapAgent goapAgent;
    public WorldState worldState;
    public AiMovement aiMove; 
    public BossHandle bosshandle;

    Animator animator;

    public GameObject colli;

    public void Init(BossDataSO data, Transform stayTransform)
    {
        this.data = data;
        bosshandle = new BossHandle(stayTransform,goapAgent,SetWorldStateData);
        goapAgent.SetStart(true);
        aiMove.SetSpeed(data.speed);
        LoadAnim();
        SetMultiSpeed(data.speed/10);
    }

    void LoadAnim()
    {
        if (animator == null) animator = gameObject.GetComponentInChildren<Animator>();
    }

    public void SetBoolAnimator(string id, bool param)
    {
        if (animator == null) return;
        animator.SetBool(id, param);
    }
    public void SetMultiSpeed(float speed)
    {
        if (animator == null) return;
        animator.SetFloat("MultiSpeed", speed);
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

    public void ResetPos()
    {
        transform.rotation = bosshandle.stayTransform.rotation;
    }
}

public enum BossState
{
    Normal,
    EggStealed,
}
