using System.Collections.Generic;
using UnityEngine;
using Raccoon.Controller;
using TMPro;
using Raccoon.Data;
using Raccoon.Bot;
using Raccoon.Utils;
using UnityEngine.Events;

namespace Raccoon.Building
{
    public class SlotController : InteractObject
    {

        public CalculateIncome calculateIncome;
        public CalculateTime count_1_Time;
        public Transform user;
        ItemCanStaySlot itemAtSlot;
        IncomeInSlot incomeInSlot;
        public float xContainer = 0.65f;
        public UnityAction<long> actionChangeIncome;

        public float multiSpeed = 1;
        int direc = 1;

        public GameObject vfxEmpty;
        public void Init(Transform user, string id)
        {
            BoxCollider bc = gameObject.GetComponent<BoxCollider>();
            if(bc != null )
            {
                direc = bc.center.x > 0 ? 1 : -1;
            }
            this.user = user;
            this.id = id;
            container = new GameObject("Constant").transform;
            container.SetParent(transform);
            container.localPosition = new Vector3(xContainer * (-direc), 0, 0);
            container.localRotation = Quaternion.Euler(0, 90 * direc, 0);
            LoadCalculate();

            if(incomeInSlot == null) LoadIncomeShow();

            vfxEmpty = VfxCtrl.instance.SpawnRandomVfx(EnumHolder.TypeVFX.SlotEmpty, container.position, false);
            vfxEmpty.transform.SetParent(transform);
            vfxEmpty.transform.position = container.position + Vector3.up * 0.1f;
            vfxEmpty?.SetActive(true);
        }
        public void HandleCollectIncome(Transform contactor)
        {
            if (contactor != user) return;
            if (slotState != Slot_state.HaveItem) return;
            if (data == null) return;
            if (data.itemType != ItemType.Brainrot) return;

            LoadCalculate();

            user.GetComponent<IIteractSlot>()?.Income(calculateIncome.GetIncome());
            calculateIncome.OnCollectIncome();
        }

        public void SetMultiSpeed(float multiIncome)
        {
            this.multiSpeed = multiIncome;
            if (calculateIncome == null) LoadCalculate();
            calculateIncome.SetBonus(multiIncome);
        }


        //public int order;
        public ItemDataSO data;
        [SerializeField] private Slot_state slotState;
        public Slot_state GetSlot_State => slotState;

        public Transform container;

        private void Start()
        {
            LoadComponent();
        }

        void LoadCalculate()
        {
            if (calculateIncome == null) calculateIncome = GetComponent<CalculateIncome>();
            if (calculateIncome == null) calculateIncome = gameObject.AddComponent<CalculateIncome>();
            calculateIncome.SetActionShowIncome(ChangeIncome);
            if (count_1_Time == null) count_1_Time = GetComponent<CalculateTime>();
            if (count_1_Time == null) count_1_Time = gameObject.AddComponent<CalculateTime>();
            count_1_Time.SetActionCount(UpdateTime);
        }
        void LoadIncomeShow()
        {
            IncomeInSlot incomePrefab = Resources.Load<IncomeInSlot>("Prefabs/InforIncome");
            incomeInSlot = Instantiate(incomePrefab, transform);
            incomeInSlot.transform.localPosition = Vector3.zero;
            incomeInSlot.transform.localRotation = Quaternion.identity;
            incomeInSlot.gameObject.SetActive(false);
            incomeInSlot.SetPosCanvas(direc);
        }

        public void SetEmpty()
        {
            if (itemAtSlot != null) GamePlayControl.instance.RespawnItem(itemAtSlot.gameObject);
            itemAtSlot = null;
            data = null;
            SetStateSlot(Slot_state.Free);
            LoadCalculate();
            count_1_Time.ResetTime();
            SetActionChangeIncome(null);

            incomeInSlot.gameObject.SetActive(false);
            vfxEmpty?.SetActive(true);

        }

        private void SetModel()
        {
            if (itemAtSlot == null) return;
            itemAtSlot.PutToSlot(EggHatching);

            itemAtSlot.SetTarget(null);
            itemAtSlot.gameObject.SetActive(true);
            itemAtSlot.transform.SetParent(container);
            itemAtSlot.transform.localEulerAngles = Vector3.zero;
            itemAtSlot.transform.localPosition = Vector3.zero;
            ReinitializeHighlight();
        }



        public ItemCanStaySlot GetBotAtSlot()
        {
            return itemAtSlot;
        }


        public void PutItem(ItemCanStaySlot item)
        {
            SetEmpty();
            data = item.data;
            itemAtSlot = item;
            SetModel();
            vfxEmpty?.SetActive(false);
            HandleData();
            count_1_Time.OnCaclulate();
            SetStateSlot(Slot_state.HaveItem);
        }
        public void EggHatching(ItemCanStaySlot item)
        {
            Debug.Log("Egg Hatching");
            PutItem(item);
            user.GetComponent<ICharactor>()?.AddItemUnlock(item.data);
            user.GetComponent<ICharactor>()?.Getdata()?.AddBotInSlot(id,item.GetItemConfig);
        }

        void HandleData()
        {
            if(data.itemType == ItemType.Brainrot)
            {
                var brainrotData = data as BotDataSO;
                calculateIncome.OnSetDataIncome(brainrotData.income);
                incomeInSlot.gameObject.SetActive(true);
            }

        }

        public void SetStartIncome(long startIncome)
        {
            calculateIncome.SetStartTotalIncome(startIncome);
        }

        public void SetStateSlot(Slot_state state)
        {
            slotState = state;
        }

        void UpdateTime()
        {
            if (itemAtSlot != null) itemAtSlot.AddTimeInSlot(multiSpeed);
            calculateIncome.Calculate();
        }
        public void ChangeIncome(long incomeNumber)
        {
            actionChangeIncome?.Invoke(incomeNumber);
            incomeInSlot.SetTextIncome(incomeNumber.ConvertCurrencyToString());
        }

        public void SetActionChangeIncome(UnityAction<long> action)
        {
            actionChangeIncome = action;
        }

        public void Stealed()
        {
            SetEmpty();
            VfxCtrl.instance.SpawnRandomVfx(EnumHolder.TypeVFX.Steal, transform.position);
            user.GetComponent<ICharactor>()?.Stealed(this);
        }

        
    }

    public enum Slot_state
    {
        Free = 0,
        HaveItem,
        LockGate = 10,
    }

}