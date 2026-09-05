using Raccoon.Controller;
using Raccoon.Item;
using UnityEngine;
using UnityEngine.Events;
using Raccoon.Building;

namespace Raccoon.Player
{
    public class PlayerContact : MonoBehaviour
    {
        public Transform transformContact;
        public Transform pointCheckItem;
        public TriggerContactHandle triggerContact;

        InteractObjectByTrigger currentIoByTrigger;

        public LayerMask layerSaveZone;
        public LayerMask layerProtect;
        public LayerMask LayerItem;
        public LayerMask LayerSlotBot;

        [SerializeField] bool inSavezone = false;
        [SerializeField] bool inProtect = false;

        public bool InSaveZone => inSavezone;
        public bool InProtect => inProtect;

        public ItemCtr item;
        public ItemCtr ItemContact => item;

        public SlotController SlotController => slotController;
        public SlotController slotController;


        private void Start()
        {
            if (triggerContact != null)
            {
                triggerContact.AcitonEnterTrigger = HandleEnterContact;
                triggerContact.AcitonExitTrigger = HandleExitContact;
            }
            inSavezone = false;

        }

        private void Update()
        {
            CheckSaveZone();
            CheckBot();
            CheckSlot();
            CheckProtect();
        }
        void CheckSaveZone()
        {
            if (transformContact == null) return;
            inSavezone = Physics.Raycast((transformContact.position + Vector3.up * 0.5f), -Vector3.up, 2.5f, layerSaveZone);
        }

        void CheckProtect()
        {
            if (transformContact == null) return;
            inProtect = Physics.Raycast((transformContact.position + Vector3.up), -Vector3.up, 2.5f, layerProtect);
        }
        void CheckSlot()
        {
            if (transformContact == null) return;
            RaycastHit hit;
            if (Physics.Raycast((transformContact.position + Vector3.up * 0.5f), -Vector3.up, out hit, 2.5f, LayerSlotBot))
            {
                var contactSlot = hit.collider.GetComponent<SlotController>();
                if (contactSlot != null)
                {
                    slotController = contactSlot;   
                }
            }
            else
            {
                slotController = null;
            }
        }
        Collider[] colliders;
        float max = 100;
        void CheckBot()
        {
            if (pointCheckItem == null) return;
            max = 100;
            colliders = Physics.OverlapSphere(pointCheckItem.position, 0.5f, LayerItem);
            if (colliders.Length > 0)
            {
                foreach (var c in colliders)
                {
                    ItemCtr i = c.GetComponent<ItemCtr>();
                    if (i != null)
                    {
                        var dis = Vector3.Distance(i.transform.position, transform.position);
                        if (max > dis)
                        {
                            item = i;
                            max = dis;
                        }
                    }
                }
            }
            else
            {
                item = null;
            }

        }

        void HandleEnterContact(InteractObjectByTrigger interactIO)
        {
            currentIoByTrigger?.HandleStopInteract(transformContact);
            currentIoByTrigger = interactIO;
            currentIoByTrigger?.HandleStartInteract(transformContact);
        }
        void HandleExitContact(InteractObjectByTrigger interactIO)
        {

            if (currentIoByTrigger == interactIO)
            {
                interactIO.HandleStopInteract(transformContact);
                currentIoByTrigger = null;
            }

        }
    }
}