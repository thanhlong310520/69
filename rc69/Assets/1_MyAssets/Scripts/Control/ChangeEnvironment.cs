using Raccoon;
using Raccoon.Controller;
using Raccoon.EnumHolder;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ChangeEnvironment : MonoBehaviour
{
    [SerializeField] List<EnvironmentData> datas = new List<EnvironmentData>();

    public UnityEvent<float> eventCountTime;
    public UnityEvent<EnvironmentType> eventChange;

    EnvironmentData currentEnvironment;
    PlayShowInMap playShow;
    GameObject vfx;

    [SerializeField] private float duration = 10f;

    private float currentTime;
    private bool isRunning = false;
    int index;

    bool playAudio;
    bool playAudio1;
    bool shakeCam;
    public void Init(EnvironmentType startType,UnityAction<EnvironmentType> eventChange)
    {

        if (playShow == null) playShow = gameObject.GetComponent<PlayShowInMap>();
        if(playShow == null ) playShow = gameObject.AddComponent<PlayShowInMap>();

        ChangeByType(startType);
        this.eventChange.AddListener(eventChange);
        isRunning = true;

        playAudio = false;
        playAudio1 = false;
        shakeCam = false;
    }

    // Update is called once per frame
    void Update()
    {
        CountTime();
    }

    void CountTime()
    {
        if (!isRunning)
            return;

        if (MainUiControl.instance.IsPopupVipActive()) return;

        currentTime -= Time.deltaTime;
        eventCountTime?.Invoke(currentTime);

        if (currentTime <= 3f)
        {
            if (!playAudio)
            {
                PlayAudio();
            }
        }
        if (currentTime <= 1f)
        {
            if (!playAudio1)
            {
                PlayAudio1();
            }
        }
        if (currentTime <= 0.25f)
        {
            if (!shakeCam)
            {
                CameraController.instance.camShake.Shake(0.5f, 0.3f);
                shakeCam = true;
            }
        }
        if (currentTime <= 0)
        {
            currentTime = 0;
            StartChange();
        }
    }

    void PlayAudio()
    {
        // play;
        ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.ChangeMap);

        playAudio = true;
    }
    void PlayAudio1()
    {
        // play;
        ObserverEventManager.Instance.Publish<SoundType>(EventObserverName.PlaySfx.ToString(), SoundType.ChangeMap1);

        playAudio1 = true;
    }

    void StartChange()
    {
        // change

        if (currentEnvironment.type == EnvironmentType.Normal)
        {
            index++;
            if(index < 0 || index >= datas.Count) index = 0;
            if (datas[index].type == EnvironmentType.Normal) index++;

            Change(datas[index]);
        }
        else
        {
            EnvironmentData data = datas.FirstOrDefault(x => x.type == EnvironmentType.Normal);
            
            Change(data);
        }
        playAudio = false;
        playAudio1 = false;
        shakeCam = false;

    }

    void ResetTime()
    {
        currentTime = duration;
    }
     
    public void ChangeByType(EnvironmentType startType)
    {
        EnvironmentData data = datas.FirstOrDefault(x => x.type == startType);
        index = datas.IndexOf(data);
        if (index < 0 || index >= datas.Count) index = 0;

        Change(datas[index]);
    }
    void Change(EnvironmentData data)
    {
        currentEnvironment = data;

        if(currentEnvironment == null)
        {
            return;
        }
        if (vfx != null)
        {
            GamePlayControl.instance.RespawnItem(vfx);
            vfx = null;
        }

        eventChange?.Invoke(currentEnvironment.type);
        duration = currentEnvironment.timeInmap;
        ChangeSkyBox(currentEnvironment.skyboxMaterial);
        SetEnvironmentColor(currentEnvironment.color);
        SpawnVfx(currentEnvironment.vfx);

        currentEnvironment.ChangeMat();

        playShow.StartPlay(data.clip,data.vol);

        ResetTime();
    }
    void ChangeSkyBox(Material skyboxMaterial)
    {
        RenderSettings.skybox = skyboxMaterial;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        DynamicGI.UpdateEnvironment();

    }
    public static void SetEnvironmentColor(Color color)
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = color;
        // cập nhật lại environment lighting
        DynamicGI.UpdateEnvironment();
    }

    

    void SpawnVfx(GameObject prefab)
    {
        if (prefab == null) return;

        vfx = GamePlayControl.instance.SpawnItem(prefab);
        vfx.transform.SetParent(transform);
        vfx.transform.localPosition = Vector3.zero;
        vfx.SetActive(true);
    }

    public EnvironmentType GetNextEvironmentNotEquarNormal()
    {
        int nextIndex = this.index + 1;
        if (nextIndex < 0 || nextIndex >= datas.Count) nextIndex = 0;
        if (datas[nextIndex].type == EnvironmentType.Normal) nextIndex++;

        return datas[nextIndex].type;
    }

    [System.Serializable]
    class EnvironmentData
    {
        public EnvironmentType type;
        public float timeInmap = 90f;
        public GameObject vfx;
        public AudioClip clip;
        public float vol;
        public Color color;
        public Material skyboxMaterial;
        [SerializeField] List<RenderAndMat> listRenderAndMat;

        public void ChangeMat()
        {
            if (listRenderAndMat == null) return;
            foreach (var rm in listRenderAndMat)
            {
                rm.ChangeMatRender();
            }
        }
    }
}

[System.Serializable]
class RenderAndMat
{
    public Renderer render;
    public List<Material> materials;
    public void ChangeMatRender()
    {
        // Bỏ qua slot lỗi (renderer bị mất khi thay map) thay vì NullReference
        if (render == null || materials == null || materials.Count == 0) return;
        // sharedMaterials: không tạo bản sao material mỗi lần đổi môi trường
        render.sharedMaterials = materials.ToArray();
    }
}
