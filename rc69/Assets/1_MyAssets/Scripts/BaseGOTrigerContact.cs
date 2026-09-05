using Raccoon.Controller;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Raccoon.Island
{

    public abstract class BaseGOTrigerContact : MonoBehaviour, InteractObjectByTrigger
    {
        public float timeDelayOpen = 1f;
        List<DataCountContact> listContact;
        List<DataCountContact> listAddContact;
        List<DataCountContact> listRemoveContact;


        protected virtual void Start()
        {
            listContact = new List<DataCountContact>();
            listAddContact = new List<DataCountContact>();
            listRemoveContact = new List<DataCountContact>();
        }
        public virtual void HandleStartInteract(Transform contactor)
        {
            DataCountContact contact = new DataCountContact(contactor);
            listAddContact.Add(contact);
        }

        public virtual void HandleStopInteract(Transform contactor)
        {

            DataCountContact data = listContact.FirstOrDefault(c => c.contact == contactor);
            listRemoveContact.Add(data);
        }

        public virtual void Raise()
        {
            if(listAddContact.Count > 0)
            {
                listContact.AddRange(listAddContact);
                listAddContact.Clear();
            }
            if(listContact.Count > 0)
            {
                for (int i = 0; i < listContact.Count; i++)
                {
                    listContact[i].AddTime(Time.deltaTime);

                    if(listContact[i].time > timeDelayOpen)
                    {
                        WaitDone(listContact[i].contact);
                        listRemoveContact.Add(listContact[i]);
                    }
                }
                if(listRemoveContact.Count > 0)
                {
                    foreach (var r in listRemoveContact)
                    {
                        listContact.Remove(r);
                    }
                    listRemoveContact.Clear();
                }
            }

        }

        // Update is called once per frame
        protected virtual void Update()
        {
            Raise();
        }

        void ShowUIWait(bool isShow)
        {
            //ActionShowWaitOpen?.Invoke(isShow);
        }
        void ChangeAmountWait(float curent, float max)
        {
            float amount = Mathf.Clamp01(curent / max);
            //ActionChangeAmountWait?.Invoke(amount);
        }

        protected virtual void WaitDone(Transform contactor)
        {
            //AcitonShowUIOpen?.Invoke(this);
        }

        public abstract void OnInteract();
    }

    class DataCountContact
    {
        public Transform contact;
        public float time;
        public DataCountContact(Transform contact)
        {
            this.contact = contact;
            time = 0;
        }
        public void AddTime(float delta)
        {
            time += delta;
        }
    }
}