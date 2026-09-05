using Raccoon.Player;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAttack : MonoBehaviour
{
    bool isAttacking;
    PlayerAnim playerAnim;
    public UnityEvent EventDoneAttack;
    public void Init(PlayerAnim anim)
    {
        isAttacking = false;
        playerAnim = anim;
    }
    public void StartAttack()
    {
        if (!isAttacking)
        {
            playerAnim.PlayerAnimAttack(1f);
            isAttacking = true;
        }
    }

    public void DoneAttack()
    {
        ResetAction();
    }

    public void ResetAction()
    {
        isAttacking = false;
        EventDoneAttack?.Invoke();
    }
}
