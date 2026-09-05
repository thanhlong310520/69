using Raccoon.Building;
using Raccoon.Controller;
using UnityEngine;

public class SlotContact : MonoBehaviour, InteractObjectByTrigger
{

    [SerializeField] SlotController slotCtr;

    private void Start()
    {
        slotCtr = gameObject.GetComponent<SlotController>();
    }
    public void HandleStartInteract(Transform contactor)
    {
        slotCtr.HandleCollectIncome(contactor);
    }

    public void HandleStopInteract(Transform contactor)
    {

    }

    public void Raise()
    {
    }

}
