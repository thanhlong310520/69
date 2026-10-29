using Raccoon.Controller;
using System.Collections.Generic;
using UnityEngine;

public class HomePlayerContact : MonoBehaviour
{

    public bool isPlayerInHome;

    public List<NpcController> npcsInPlayerHome;

    private void Start()
    {
        npcsInPlayerHome = new List<NpcController>();
    }
    private void OnTriggerEnter(Collider other)
    {
        CheckNpcInPlayerHome(other.transform, true);
        CheckPlayerInHome(other.transform, true);
    }
    private void OnTriggerExit(Collider other)
    {
        CheckPlayerInHome(other.transform, false);
        CheckNpcInPlayerHome(other.transform, false);
    }

    void CheckPlayerInHome(Transform contact,bool inHome)
    {
        if (contact == null) return;
        if (contact.GetComponent<PlayerController>() != null)
        {
            isPlayerInHome = inHome;

            if (inHome)
            {
                OnCollectAllSlot();
            }
        }
    }

    void OnCollectAllSlot()
    {
        var playerHome = HomeCtr.instance.GetHomeByUser(PlayerController.instance.transform);
        if (playerHome != null)
        {
            long income = 0;
            foreach (var s in playerHome.AllSlots)
            {
                if (s.GetSlot_State == Raccoon.Building.Slot_state.HaveItem)
                {
                    if (s.calculateIncome != null)
                    {
                        income += s.calculateIncome.GetIncome();
                        s.calculateIncome.OnCollectIncome();
                    }
                }
            }
            if (income > 0) PlayerController.instance.Income(income);
        }
    }

    void CheckNpcInPlayerHome(Transform contact, bool inHome)
    {
        if (contact == null) return;
        NpcController npc = contact.GetComponent<NpcController>();
        if(npc != null)
        {
            if (inHome)
            {
                if(!npcsInPlayerHome.Contains(npc)) npcsInPlayerHome.Add(npc);
            }
            else npcsInPlayerHome.Remove(npc);
        }
    }

}
