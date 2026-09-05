using Raccoon.EnumHolder;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowChangeMap : MonoBehaviour
{
    [SerializeField] List<DataEnvironment> datas;
    public ChangeEnvironment ce;
    public Image bgImg;
    public Image iconImg;

    public TMP_Text textTime;
    public TMP_Text textName;
    public TMP_Text textStart;

    public void Change(EnvironmentType type)
    {
        if (type == EnvironmentType.Normal)
        {
            ChangeNormal();
            textStart.text = "Start in:";
        }
        else
        {
            textStart.text = "End in:";
            ChangeBG(type);
        }
    }

    void ChangeNormal()
    {
        var environment = ce.GetNextEvironmentNotEquarNormal();

        ChangeBG(environment);
    }

    void ChangeBG(EnvironmentType type)
    {
        DataEnvironment dt = datas.FirstOrDefault(d => d.type == type);
        bgImg.sprite = dt.bg;
        iconImg.sprite = dt.icon;
        textName.text = dt.nameEvent;
    }

    public void ChangeTime(float time)
    {
        string stime = FormatTime(time);
        textTime.text = stime;
    }

    public string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    [System.Serializable]
    struct DataEnvironment
    {
        public EnvironmentType type;
        public string nameEvent;
        public Sprite bg;
        public Sprite icon;
    }
}
