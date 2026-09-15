using Raccoon.Data;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TabColorUI : MonoBehaviour
{
    public ZoneType type;
    public GameObject select;
    public GameObject unselect;
    Button button;

    public Action<TabColorUI> ActionClick;
    private void Start()
    {
        if(button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        ActionClick?.Invoke(this);
    }

    public void SetSelect(bool isselect)
    {
        select.SetActive(isselect);
        unselect.SetActive(!isselect);
    }
}
