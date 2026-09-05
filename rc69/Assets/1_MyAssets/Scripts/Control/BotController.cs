//using cakeslice;
using Raccoon.Building;
using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.EnumHolder;
using Raccoon.Helpers;
using Raccoon.Item;
using Raccoon.UI;
using Raccoon.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Raccoon.Bot
{
    public class BotController : ItemCtr
    {

        //new
        public CountTime countTime;
        [SerializeField] private ColorOverlayController colorRainbow;



        [Header("Old value")]
        public Transform effectBody;

        [SerializeField] private GameObject modelContainer;
        public UICanvasInfo infor;
        private Animator[] animators;


        private Rigidbody rigid;

        public GameObject vfx;



        private bool cachedDefaultMaterials;


        private Dictionary<Material, Color> originalColors = new Dictionary<Material, Color>();
        private Dictionary<Renderer, Material[]> originalMats = new Dictionary<Renderer, Material[]>();

        public override void Spawned(UnityAction<ItemCtr> action)
        {
            base.Spawned(action);
            countTime.Init(GetTimeCount(), ShowTimeCount, DestroyBot);
            infor.SetActiveTxtCountTime(true);
        }

        int GetTimeCount()
        {
            return UnityEngine.Random.Range(20, 31);
        }
        public override void Init(ItemDataSO data, ItemConfig config, ItemState state , Transform user = null)
        {
            base.Init(data, config, state, user);
            CacheDefaultMaterials();
            SetMatColor();
            infor.SetUIView(data, config.level);
            infor.SetActiveTxtCountTime(false);

            SetVfx();
        }
        public void DestroyBot()
        {
            eventRemoveFromSpawner?.Invoke(this);
            GamePlayControl.instance.RespawnItem(gameObject);
        }


        void ShowTimeCount(float time)
        {
            infor.SetTimeCount(time);
        }


        public override void Drop(bool showInfor = true, bool activeAbility = false)
        {
            base.Drop(showInfor, activeAbility);
            countTime.SetCanCount(showInfor);
            infor.SetActiveTxtCountTime(showInfor);
        }
        public override void Pick(Transform holder)
        {
            base.Pick(holder);
            countTime.SetCanCount(false);
            infor.SetActiveTxtCountTime(false);
        }


        public void PutToSlot()
        {
            infor.SetActiveInfoInSlot();
        }



        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            LoadComponent();
        }

        protected void LoadComponent()
        {

            if (animators == null)
                animators = gameObject.GetComponentsInChildren<Animator>();

            if (rigid == null)
                rigid = gameObject.GetComponent<Rigidbody>();

        }

        private void CacheDefaultMaterials()
        {
            if (modelContainer == null) return;
            if (cachedDefaultMaterials) return;

            cachedDefaultMaterials = true;
            originalColors.Clear();
            originalMats.Clear();

            var renderers = modelContainer.GetComponentsInChildren<Renderer>();

            foreach (var rend in renderers)
            {
                if (rend.gameObject.layer == LayerMask.NameToLayer("VFX")) continue;

                rend.gameObject.layer = LayerMask.NameToLayer("Item");
                originalMats[rend] = rend.materials;

                foreach (var mat in rend.materials)
                {
                    if (!originalColors.ContainsKey(mat) && mat.HasProperty("_Color"))
                        originalColors[mat] = mat.color;
                }
            }
        }
        private void SetMatColor()
        {
            if (data == null || modelContainer == null) return;

            var renderers = modelContainer.GetComponentsInChildren<Renderer>();
            colorRainbow.SetOverlayActive(data.colorType == ColorType.Rainbow);

            foreach (var rend in renderers)
            {
                if (colorRainbow.IsOverlayRender(rend)) continue;
                if (rend.gameObject.layer == LayerMask.NameToLayer("VFX")) continue;

                rend.sharedMaterials = originalMats[rend];
                var currentMatsForColor = rend.sharedMaterials;

                switch (data.colorType)
                {
                    case ColorType.Normal:
                        // Restore original materials (cleans up rainbow)
                        foreach (var mat in currentMatsForColor)
                        {
                            if (mat != null && originalColors.TryGetValue(mat, out var originalColor) && mat.HasProperty("_Color"))
                                mat.color = originalColor;
                        }

                        break;

                    case ColorType.Diamond:
                    case ColorType.Golden:
                        var colorTint = GameUtils.GetColorByColorType(data.colorType);
                        foreach (var mat in currentMatsForColor)
                        {
                            if (mat == null || mat.name.Contains("_static") || !mat.HasProperty("_Color"))
                                continue;
                            // Apply
                            mat.color = GameUtils.ApplyColorTint(mat.color, colorTint, 0.75f);
                        }
                        break;

                    case ColorType.Rainbow:
                        foreach (var mat in currentMatsForColor)
                        {
                            if (mat != null && originalColors.TryGetValue(mat, out var originalColor) && mat.HasProperty("_Color"))
                                mat.color = originalColor;
                        }

                        break;
                }
            }

        }

        void SetVfx()
        {
            if (vfx != null) return;
            if (data == null) return;
            ColorEffect color = ColorEffect.None;

            if(data.rarityType == RarityType.Godly)
            {
                color = ColorEffect.Green;
            }
            else if(data.rarityType == RarityType.Secret)
            {

                switch (data.colorType)
                {
                    case ColorType.Normal:
                        {
                            color = ColorEffect.Yellow;
                            break;
                        }
                    case ColorType.Golden:
                        {
                            color = ColorEffect.Red;
                            break;
                        }
                    case ColorType.Diamond:
                        {
                            color = ColorEffect.Blue;
                            break;
                        }
                    case ColorType.Rainbow:
                        {
                            color = ColorEffect.Purple;
                            break;
                        }
                }
            }

            var vfxPrefab = VfxCtrl.instance.GetVfxBrainrot(color);
            vfx = GamePlayControl.instance.SpawnItem(vfxPrefab);
            if(vfx != null)
            {
                vfx.transform.SetParent(transform);
                vfx.transform.localPosition = Vector3.up * 0.5f;
            }
        }
    }
}