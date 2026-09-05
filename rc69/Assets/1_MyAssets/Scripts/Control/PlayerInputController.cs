using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Raccoon.InputCtr
{

    public enum InputButtonNames
    {
        Button1,
        Button2,
        Jump,
        Up,
        Down,
        LeftControl,
        LeftShift,
        LeftAlt,
        Build,
        Fly,
        Crouch,
        Inventory,
        Light,
        ThrowItem,
        Action,
        MiddleButton,
        SeeThroughUp,
        SeeThroughDown,
        Escape,
        DebugWindow,
        Console,
        Thrust,
        MicroVoxels,
        Rotate,
        Joystick,
        Custom2,
        Custom3,
        Custom4,
        Custom5,
        Custom6,
        Custom7,
        Custom8,
        Custom9
    }

    public enum InputButtonPressState
    {
        Idle,
        Down,
        Up,
        Pressed
    }

    public struct InputButtonState
    {
        public InputButtonPressState pressState;
        public float pressStartTime;
    }

    public abstract class PlayerInputController : MonoBehaviour
    {
        [NonSerialized]
        public float horizontalAxis;

        /// <summary>
        /// Vertical input axis
        /// </summary>
        [NonSerialized]
        public float verticalAxis;

        /// <summary>
        /// Vertical input axis
        /// </summary>
        [NonSerialized]
        public bool anyAxisButtonPressed;

        /// <summary>
        /// Horizontal mouse axis
        /// </summary>
        [NonSerialized]
        public float mouseX;

        /// <summary>
        /// Vertical mouse axis
        /// </summary>
        [NonSerialized]
        public float mouseY;

        /// <summary>
        /// Vertical mouse axis
        /// </summary>
        [NonSerialized]
        public float mouseScrollWheel;

        /// <summary>
        /// Location of cursor on screen
        /// </summary>
        [NonSerialized]
        public Vector3 screenPos;

        /// <summary>
        /// If cursor is inside screen
        /// </summary>
        [NonSerialized]
        public bool focused = true;

        /// <summary>
        /// Returns true if any button or key is pressed
        /// </summary>
        [NonSerialized]
        public bool anyKey;

        /// <summary>
        /// Maximum time in seconds between button press and release to inform a click
        /// </summary>
        public float clickTime = 0.2f;

        /// <summary>
        /// Disabled move/rotation controls but rest of input buttons are still processed like console, Esc, etc.
        /// </summary>
        public virtual new bool enabled { get; set; }

        /// <summary>
        /// Returns true if button1 is dragging;
        /// </summary>
        [NonSerialized]
        public bool button1_dragged;

        [NonSerialized]
        public Vector2 joystickPos;

        [NonSerialized]
        public bool initialized;


        protected bool canAction = true;

        public bool CanAction => canAction;

        protected InputButtonState[] buttons;

        protected virtual bool Initialize()
        {
            return true;
        }
        public virtual void SetCanAcion(bool canAction)
        {
            this.canAction = canAction;
        }
        protected abstract void UpdateInputState();

        public bool GetButton(InputButtonNames button)
        {
            return initialized && buttons[(int)button].pressState == InputButtonPressState.Pressed;
        }

        public bool GetButtonDown(InputButtonNames button)
        {
            return initialized && buttons[(int)button].pressState == InputButtonPressState.Down;
        }

        public bool GetButtonUp(InputButtonNames button)
        {
            return initialized && buttons[(int)button].pressState == InputButtonPressState.Up;
        }

        public bool GetButtonClick(InputButtonNames button)
        {
            return initialized && buttons[(int)button].pressState == InputButtonPressState.Up && (Time.time - buttons[(int)button].pressStartTime) < clickTime;
        }


        bool ignoreThisFrame;
        public void Init()
        {
            int buttonCount = Enum.GetNames(typeof(InputButtonNames)).Length;
            buttons = new InputButtonState[buttonCount];
            initialized = Initialize();
            enabled = true;
            ignoreThisFrame = true;
        }

        void Update()
        {
            if (!initialized)
                return;
            //anyKey = Input.anyKey;
            anyKey = Keyboard.current.anyKey.isPressed;
            for (int k = 0; k < buttons.Length; k++)
            {
                buttons[k].pressState = InputButtonPressState.Idle;
            }
            if (!enabled)
            {
                mouseScrollWheel = mouseX = mouseY = horizontalAxis = verticalAxis = 0;
            }

            if (ignoreThisFrame)
            {
                ignoreThisFrame = false;
            }
            else
            {
                UpdateInputState();
            }
            if (!anyKey)
            {
                for (int k = 0; k < buttons.Length; k++)
                {
                    if (buttons[k].pressState != InputButtonPressState.Idle)
                    {
                        anyKey = true;
                        break;
                    }
                }
            }
        }


    }
}