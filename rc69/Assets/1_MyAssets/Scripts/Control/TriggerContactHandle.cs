using UnityEngine;
using UnityEngine.Events;
using Raccoon.Controller;
public class TriggerContactHandle : MonoBehaviour
{
    public UnityAction<InteractObjectByTrigger> AcitonEnterTrigger;
    public UnityAction<InteractObjectByTrigger> AcitonExitTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (AcitonEnterTrigger == null) return;
        InteractObjectByTrigger interact = other.GetComponent<InteractObjectByTrigger>();
        if (interact != null) AcitonEnterTrigger.Invoke(interact);
    }
    private void OnTriggerExit(Collider other)
    {
        if (AcitonExitTrigger == null) return;
        InteractObjectByTrigger interact = other.GetComponent<InteractObjectByTrigger>();
        if (interact != null) AcitonExitTrigger.Invoke(interact);
    }
}
