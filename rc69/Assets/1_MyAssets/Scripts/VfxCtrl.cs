using Raccoon.Controller;
using Raccoon.Data;
using Raccoon.EnumHolder;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class VfxCtrl : MonoBehaviour
{
    #region singleton
    public static VfxCtrl instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [SerializeField] List<VfxBrainrot> vfxBrainrots; 
    [SerializeField] List<Vfx> vfxs;


    public GameObject GetVfxBrainrot(ColorEffect typeColor)
    {
        var vfxdata = vfxBrainrots.FirstOrDefault(v=> v.typeColor == typeColor);

        return vfxdata.vfxPrefab;
    }

    public GameObject GetVfxByType(TypeVFX type)
    {
        var vfxdata = vfxs.FirstOrDefault(v => v.type == type);

        int index = UnityEngine.Random.Range(0, vfxdata.vfxPrefabs.Count);
        return vfxdata.vfxPrefabs[index];
    }


    public GameObject SpawnRandomVfx(TypeVFX type, Vector3 pos, bool autoRepool = true)
    {
        var prefab = GetVfxByType(type);
        if (prefab == null) return null;
        var vfx = GamePlayControl.instance.SpawnItem(prefab);

        vfx.SetActive(true);
        vfx.transform.SetParent(transform);
        vfx.transform.position = pos;
        if (autoRepool) 
        { 
            DestroyByTime destroy = vfx.GetComponent<DestroyByTime>();
            if (destroy == null) vfx.AddComponent<DestroyByTime>();
        }
        return vfx;
    }
}

[System.Serializable]
struct VfxBrainrot
{
    public ColorEffect typeColor;
    public GameObject vfxPrefab;
}

[System.Serializable]
struct Vfx
{
    public TypeVFX type;
    public List<GameObject> vfxPrefabs;
}
