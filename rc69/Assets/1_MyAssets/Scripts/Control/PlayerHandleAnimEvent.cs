using UnityEngine;
using UnityEngine.Events;

public class PlayerHandleAnimEvent : MonoBehaviour
{

    public Transform holderItem;

    UnityEvent eventAttack;

    public void AddEventAttack(UnityAction attackAction)
    {
        if(eventAttack == null) eventAttack = new UnityEvent();
        eventAttack.AddListener(attackAction);
    }

    public void EventAttack()
    {
        eventAttack?.Invoke();
    }
}
