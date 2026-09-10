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

        public Transform user;
        BotController botAtSlot;
        IncomeInSlot incomeInSlot;
        public float xContainer = 0.65f;
        public UnityAction<long> actionChangeIncome;

        public float multiIncome = 1;
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
            if (calculateIncome == null) LoadCalculateIncome();

            if(incomeInSlot == null) LoadIncomeShow();

            vfxEmpty = VfxCtrl.instance.SpawnRandomVfx(EnumHolder.TypeVFX.SlotEmpty, container.position, false);
            vfxEmpty.transform.SetParent(transform);
            vfxEmpty.transform.position = container.position + Vector3.up * 0.1f;
            vfxEmpty?.SetActive(true);
        }
        public void HandleCollectIncome(Transform contactor)
        {
            if (contactor != user) return;
            if (slotState != Slot_state.HaveBot) return;

            if (calculateIncome == null) LoadCalculateIncome();

            //ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.ContactSlot);
            user.GetComponent<IIteractSlot>()?.Income(calculateIncome.GetIncome());
            calculateIncome.OnCollectIncome();
        }

        public void SetMultiIncome(float multiIncome)
        {
            this.multiIncome = multiIncome;
            if (calculateIncome == null) LoadCalculateIncome();
            calculateIncome.SetBonus(multiIncome);
        }


        //public int order;
        public BotDataSO data;
        [SerializeField] private Slot_state slotState;
        public Slot_state GetSlot_State => slotState;

        public Transform container;
        ///// Canvas UI
        //public GameObject uiCanvas;
        //public TextMeshProUGUI txtIncome;

        //[SerializeField] private GameObject vfxMerge;

        ////income
        //private float incomeTimer;
        //private float incomeInterval = 1f; // every 1 second
        //private long totalIncome;

        //public bool CanStealBot => slotState == Slot_state.Incoming && botAtSlot != null;
        //public bool CanSellBot => slotState == Slot_state.Incoming && botAtSlot != null;
        //public bool IsFreeSlot => slotState == Slot_state.Free;
        //public string GetSlotIdOrder => $"slot_{order}";

        private void Start()
        {
            LoadComponent();
            LoadCalculateIncome();
        }

        void LoadCalculateIncome()
        {
            if (calculateIncome == null) calculateIncome = GetComponent<CalculateIncome>();
            if (calculateIncome == null) calculateIncome = gameObject.AddComponent<CalculateIncome>();
            calculateIncome.SetActionShowIncome(ChangeIncome);
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
            if (botAtSlot != null) GamePlayControl.instance.RespawnItem(botAtSlot.gameObject);
            botAtSlot = null;
            data = null;
            SetStateSlot(Slot_state.Free);
            if (calculateIncome == null) LoadCalculateIncome();
            calculateIncome.ResetIncom();
            SetActionChangeIncome(null);

            incomeInSlot.gameObject.SetActive(false);
            vfxEmpty?.SetActive(true);

        }

        private void SetModel()
        {
            if (botAtSlot == null) return;
            botAtSlot.SetTarget(null);
            botAtSlot.gameObject.SetActive(true);
            botAtSlot.transform.SetParent(container);
            botAtSlot.transform.localEulerAngles = Vector3.zero;
            botAtSlot.transform.localPosition = Vector3.zero;
            botAtSlot.PutToSlot();
            ReinitializeHighlight();
        }



        public BotController GetBotAtSlot()
        {
            return botAtSlot;
        }


        public void PutBrainrot(BotController bot)
        {

            if (data == null) data = bot.data as BotDataSO;
            botAtSlot = bot;
            SetModel();
            SetStateSlot(Slot_state.HaveBot);
            if (calculateIncome == null) LoadCalculateIncome();
            calculateIncome.OnSetDataIncome(data.income);
            incomeInSlot.gameObject.SetActive(true);

            vfxEmpty?.SetActive(false);

        }

        public void SetStartIncome(long startIncome)
        {
            calculateIncome.SetStartTotalIncome(startIncome);
        }

        public void SetStateSlot(Slot_state state)
        {
            slotState = state;
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
        HaveBot,
        HaveEgg,
        LockGate = 10,
    }

    public class SlotMerge
    {
        public List<SlotController> slotSameData;
    }
}