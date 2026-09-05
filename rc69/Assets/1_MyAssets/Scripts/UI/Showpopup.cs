using Raccoon.Controller;
using UnityEngine;
using UnityEngine.UI;

public class Showpopup : MonoBehaviour
{
    Button button;
    public PopupCanvas.PopupType popupType;

    private void Start()
    {
        if(button == null) { button = GetComponent<Button>(); }
        button.onClick.AddListener(Show);
    }

    void Show()
    {
        GamePlayControl.instance.ShowPopup(popupType, PlayerController.instance.GetCharactorData);
    }
}
