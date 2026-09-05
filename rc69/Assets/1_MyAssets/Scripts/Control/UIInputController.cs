using Raccoon.InputCtr;
using UnityEngine;

public class UIInputController : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public GameObject runningSelected;
    private void Start()
    {
        if(canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if(canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.enabled = true; 
        canvasGroup.alpha = 1.0f;

        SetRunning();
    }
    public void HandleJoystickMove(Vector2 valueJoystick)
    {
        InputController.instance.UpdateMovePlayer(valueJoystick);
    }

    public void HandleTouchToLook(float mouseX, float mouseY)
    {
        InputController.instance.UpdateInputCamera(mouseX, mouseY);
    }

    public void HandleButtonAttack(bool buttonState)
    {
        InputController.instance.SetPlayerAttack(buttonState);
    }

    public void HandleButtonJump(bool buttonState)
    {
        InputController.instance.SetPlayerJump(buttonState);
    }

    public void HandleButtonRunning()
    {
        InputController.instance.SetIsRunning();
        SetRunning();
    }

    void SetRunning()
    {
        runningSelected.SetActive(InputController.instance.GetIsRunning());
    }
    public void HandleActionClickToScreen()
    {
        InputController.instance.SetIsDropBox();
    }
}
