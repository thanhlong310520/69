using Raccoon;
using Raccoon.Controller;
using Raccoon.Item;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : ItemCtr
{
    public Transform pointAttack;
    public int dame;


    public UnityEvent<Vector3,int,Transform> handleAttack;
    
    public void Attack()
    {
        ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.BaseBatAttack);
        handleAttack?.Invoke(pointAttack.position,dame,user);
    }

    public float radius = 1f;
    CheckIReceiver check;
    private void Start()
    {
        check = new CheckIReceiver();
    }
    public void AttackKnockback(Vector3 attackPos, int dame, Transform user)
    {
        var listReceiver = check.CheckAreaPoint(attackPos, radius, user);
        listReceiver.ForEach(
            r => {
                if (r != null)
                {
                    r.KnockBack(dame, user);
                }
            });
    }

}
