
using Raccoon.Controller;
using Raccoon.EnumHolder;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupEventEnvironment : PopupCanvas
{
    public ItemEventEnvironment itemPrefab;
    public Transform content;
    [SerializeField] List<EventEnvironmentData> events;

    private void Start()
    {
        foreach (var e in events)
        {
            var it = Spawn();
            it.Init(e.type, e.bg, e.icon, e.name, e.description, OnClick);
            it.gameObject.SetActive(true);
        }
    }

    protected ItemEventEnvironment Spawn()
    {
        var newItem = Instantiate(itemPrefab, content);
        return newItem;
    }

    public override void Show(PopupType p, UnityAction afterPopup, object obj)
    {
        base.Show(p, afterPopup, obj);
    }
    void OnClick(EnvironmentType type)
    {
        GamePlayControl.instance.OnChangeMapByUI(type);
        Hide();
    }

    [System.Serializable]
    struct EventEnvironmentData
    {
        public EnvironmentType type;
        public Sprite bg;
        public Sprite icon;
        public string name;
        public string description;
    }

}
