using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
namespace Raccoon.InputCtr
{

    public class DuaTouchController : PlayerInputController
    {
        public float dragThreshold = 1f;
        public float rotationSpeed = 0.1f;
        float pressTime;
        float pressStationary;
        Vector2 lastTouchPos;
        bool dragged;

        public int idTouch = -1;
        public override void SetCanAcion(bool canAction)
        {
            base.SetCanAcion(canAction);
            idTouch = -1;
        }
        public void SetIdTouchByScreenPos(Vector2 pos)
        {
            if (!canAction) return;

            int touchCount = Touch.activeTouches.Count;
            for (int k = 0; k < touchCount; k++)
            {
                Vector2 sub = Touch.activeTouches[k].screenPosition - pos; 
                if (sub.magnitude < 1f)
                {
                    idTouch = Touch.activeTouches[k].touchId;
                    break;
                }
            }
        }


        private void Awake()
        {
            EnhancedTouchSupport.Enable();
        }
        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }
        protected override void UpdateInputState()
        {
            //if (!canAction)
            //    mouseX = mouseY = 0;


            focused = true;
            int touchCount = Touch.activeTouches.Count;
            for (int k = 0; k < touchCount; k++)
            {
                //if(idTouch == Touch.activeTouches[k].touchId)
                {
                    screenPos = Touch.activeTouches[k].screenPosition;
                    ManageTouch(k);
                    button1_dragged = dragged;
                }
            }
            if (idTouch == -1 || Touch.activeTouches.Count == 0)
            {
                horizontalAxis = verticalAxis = 0;
                anyAxisButtonPressed = false;
                joystickPos = Vector2.zero;
            }
        }

        void ManageTouch(int touchIndex)
        {
            var t = Touch.activeTouches[touchIndex];
            switch (t.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    mouseX = 0;
                    mouseY = 0;
                    buttons[(int)InputButtonNames.Button1].pressStartTime = Time.time;
                    pressTime = Time.time;
                    dragged = false;
                    lastTouchPos = t.screenPosition;
                    break;
                case UnityEngine.InputSystem.TouchPhase.Moved:

                    pressTime = Time.time;
                    float deltaX = t.delta.x;
                    if (deltaX > 0)
                    {
                        deltaX -= dragThreshold;
                        if (deltaX < 0)
                            deltaX = 0;
                    }
                    else if (deltaX < 0)
                    {
                        deltaX += dragThreshold;
                        if (deltaX > 0)
                            deltaX = 0;
                    }
                    deltaX *= rotationSpeed * 3000f / Screen.width;
                    mouseX = mouseX * 0.75f + deltaX * 0.25f;

                    float deltaY = t.delta.y;
                    if (deltaY > 0)
                    {
                        deltaY -= dragThreshold;
                        if (deltaY < 0)
                            deltaY = 0;
                    }
                    else if (deltaY < 0)
                    {
                        deltaY += dragThreshold;
                        if (deltaY > 0)
                            deltaY = 0;
                    }
                    deltaY *= rotationSpeed * 1500f / Screen.height;
                    mouseY = mouseY * 0.75f + deltaY * 0.25f;
                    buttons[(int)InputButtonNames.Button1].pressState = InputButtonPressState.Pressed;
                    if (Vector2.Distance(t.screenPosition, lastTouchPos) > 3f)
                        dragged = true;
                    lastTouchPos = t.screenPosition;
                    print("input " + mouseX + " " + mouseY);
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:

                    mouseX = mouseY = 0;
                    if (!dragged && Time.time - pressTime < 0.3f)
                    {
                        buttons[(int)InputButtonNames.Button1].pressState = InputButtonPressState.Down; // click
                    }
                    else
                    {
                        buttons[(int)InputButtonNames.Button1].pressState = InputButtonPressState.Up;
                    }
                    break;

                case UnityEngine.InputSystem.TouchPhase.Stationary:
                    if (dragged)
                    {
                        pressStationary += Time.deltaTime;
                        if (pressStationary > 0.5f)
                        {
                            dragged = false;
                            pressStationary = 0;
                        }
                    }
                    if (!dragged && Time.time - pressTime > 0.3f)
                    {
                        pressStationary = 0;
                        mouseX = mouseY = 0;
                        buttons[(int)InputButtonNames.Button1].pressState = InputButtonPressState.Down; // long press
                                                                                                        //Debug.Log("long press");
                    }
                    lastTouchPos = t.screenPosition;
                    break;
            }
        }
    }
}