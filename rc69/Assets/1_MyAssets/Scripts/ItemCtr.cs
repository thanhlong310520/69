using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.EnumHolder;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
namespace Raccoon.Item
{

    public class ItemCtr : MonoBehaviour
    {

        public ItemDataSO data;
        public Transform user;
        public ItemState state;
        public ItemState GetState => state;
        ItemConfig config;
        public ItemConfig GetItemConfig => config;
        public FollowTarget ftarget;
        public UnityAction<ItemCtr> eventRemoveFromSpawner;

        public Collider colli;
        public Collider GetCollider{
            get
            {
                if (colli == null) colli = GetComponent<Collider>();
                if (colli == null) colli = gameObject.AddComponent<Collider>();
                return colli;
            }
        }
        bool isSave;
        public bool GetIsSave => isSave;

        public virtual void Init(ItemDataSO data, ItemConfig config, ItemState state, Transform user = null)
        {
            this.data = data;
            this.config = config;
            this.user = user;
            SetState(state);
            SetIsSave(true);
            GetCollider.enabled = false;
        }

        public void SetIsSave(bool isSave)
        {
            this.isSave = isSave;
        }
        public void SetTarget(Transform target)
        {
            if (this.ftarget == null) ftarget = gameObject.GetComponent<FollowTarget>();
            if (this.ftarget == null) ftarget = gameObject.AddComponent<FollowTarget>();
            this.ftarget.Init(target);
        }

        public virtual void Pick(Transform holder)
        {
            SetTarget(holder);
            GetCollider.enabled = false;
            SetState(ItemState.Loot);
        }

        public virtual void Drop(bool showInfor = true, bool activeAbility = false)
        {
            float angle = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
            SetTarget(null);
            SetState(ItemState.Normal);
            GetCollider.enabled = true;
        }
        public virtual void Spawned(UnityAction<ItemCtr> action)
        {
            SetIsSave(false);
            GetCollider.enabled = true;
            SetEventRemoteFromSpawner(action);
            SetTarget(null);
        }
        void SetEventRemoteFromSpawner(UnityAction<ItemCtr> action)
        {
            eventRemoveFromSpawner = action;
        }

        public virtual void Save()
        {
            SetIsSave(true);
            eventRemoveFromSpawner?.Invoke(this);
            SetState(state);
            SetTarget(null);
        }
        public void ChangeEnvironment(EnvironmentType type)
        {
            if(config != null)
            {
                config.SetEnvironmentType(type);
            }

        }
        private void OnDisable()
        {
            SetEventRemoteFromSpawner(null);
        }

        public void SetState(ItemState state)
        {
            this.state = state;
        }
    }
}