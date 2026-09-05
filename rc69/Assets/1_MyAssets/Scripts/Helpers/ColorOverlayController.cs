using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Raccoon.Helpers
{


    public class ColorOverlayController : MonoBehaviour
    {
        [SerializeField] private Shader overlayShader;
        private Renderer overlayRenderer;
        
        private bool overlayActive;

        void Start()
        {
            if (overlayShader == null)
                overlayShader = Shader.Find("Custom/Standard_TimeColor_Overlay");

            TryAddOverlay();
        }

        void TryAddOverlay()
        {
            // Skinned mesh
            var smr = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr != null)
            {
                AddOverlaySkinned(smr);
            }
            // Static mesh
            else
            {
                var mr =  gameObject.GetComponentInChildren<MeshRenderer>();
                if (mr != null)
                {
                    AddOverlayStatic(mr);
                }
            } 
        }

        void AddOverlaySkinned(SkinnedMeshRenderer original)
        {
            if(overlayShader == null) return;
            GameObject overlay = new GameObject("ColorOverlay");
            overlay.transform.SetParent(original.transform.parent, false);
            original.receiveShadows = false;
            var smr = overlay.AddComponent<SkinnedMeshRenderer>();
            smr.gameObject.layer = original.gameObject.layer;
            // Mesh + animation binding
            smr.sharedMesh = original.sharedMesh;
            smr.bones = original.bones;
            smr.rootBone = original.rootBone;
            smr.updateWhenOffscreen = original.updateWhenOffscreen;
            // smr.material = new Material(overlayShader);
            // Create overlay materials (same count)
            var originalMats = original.sharedMaterials;
            List<Material> overlayMats = new();

            foreach (var mat in originalMats)
            {
                if(mat.name.ToLower().Contains("outline")) continue;
                overlayMats.Add(new Material(overlayShader));
            }
            // for (int i = 0; i < originalMats.Length; i++)
            // {
            //     overlayMats[i] = new Material(overlayShader);
            // }
            
            smr.materials = overlayMats.ToArray();

            // Render settings
            smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            smr.receiveShadows = false;
            smr.sortingOrder = original.sortingOrder + 1;
            overlayRenderer = smr;
            
            if (overlayRenderer != null)
                overlayRenderer.enabled = overlayActive;
        }

        void AddOverlayStatic(MeshRenderer original)
        {
            GameObject overlay = new GameObject("ColorOverlay");
            overlay.transform.SetParent(original.transform, false);

            var mf = overlay.AddComponent<MeshFilter>();
            mf.sharedMesh = original.GetComponent<MeshFilter>().sharedMesh;

            var mr = overlay.AddComponent<MeshRenderer>();
            mr.material = new Material(overlayShader);

            SetupRenderer(mr, original);
            overlayRenderer = mr;
            
            if (overlayRenderer != null)
                overlayRenderer.enabled = overlayActive;
        }

        void SetupRenderer(Renderer overlay, Renderer original)
        {
            overlay.shadowCastingMode = ShadowCastingMode.Off;
            overlay.receiveShadows = false;
            overlay.sortingOrder = original.sortingOrder + 1;
        }

        // Public control
        public void SetOverlayActive(bool value)
        {
            // Debug.Log("rainbow enabled: " + overlayActive);
            overlayActive = value;
            if (overlayRenderer != null)
            {
                overlayRenderer.enabled = value;
                overlayRenderer.gameObject.layer = gameObject.layer;
            }
            
        }

        public bool IsOverlayRender(Renderer compare)
        {
            return compare == overlayRenderer;
        }
    }
}
