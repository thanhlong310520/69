using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.EnumHolder;
using Raccoon.Item;
using Raccoon.UI;
using UnityEngine;
using UnityEngine.Events;
namespace Raccoon.Bot
{
    public class ItemCanStaySlot : ItemCtr
    {


        [SerializeField] private GameObject modelContainer;
        private Rigidbody rigid;

        public GameObject vfx;

        public UICanvasInfo infor;

        public override void Init(ItemDataSO data, ItemConfig config, ItemState state, Transform user = null)
        {
            base.Init(data, config, state, user);
            infor.SetUIView(data, 1);

        }
        public virtual void AddTimeInSlot(float time)
        {
            config.AddTimeInSlot(time);
        }

        public void DestroyItem()
        {
            eventRemoveFromSpawner?.Invoke(this);
            GamePlayControl.instance.RespawnItem(gameObject);
        }

        protected UnityAction<ItemCanStaySlot> actionChangeItem;
        public virtual void PutToSlot(UnityAction<ItemCanStaySlot> actionChangeItem)
        {
            this.actionChangeItem = actionChangeItem;
        }
        void Start()
        {
            LoadComponent();
        }

        protected void LoadComponent()
        {

            if (rigid == null)
                rigid = gameObject.GetComponent<Rigidbody>();

        }
        
    }
}