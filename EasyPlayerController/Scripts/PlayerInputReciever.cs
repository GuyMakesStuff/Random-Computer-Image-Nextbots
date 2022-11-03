using System.Collections;
using Experiments.CubeArt.Managers;
using UnityEngine;

namespace EasyPlayerController
{
    public class PlayerInputReciever : MonoBehaviour
    {
        [Header("Mouse")]
        public string MouseXAxis = "Mouse X";
        [HideInInspector]
        public float MouseX;
        public string MouseYAxis = "Mouse Y";
        [HideInInspector]
        public float MouseY;
        public bool LockMouse;
        public bool InvertMouse;

        [Header("Movement")]
        public bool Smooth;
        public string HoriAxis = "Horizontal";
        [HideInInspector]
        public float Hori;
        public string VertAxis = "Vertical";
        [HideInInspector]
        public float Vert;
        public string AltHoriAxis = "Alt Horizontal";
        [HideInInspector]
        public float AltHori;

        [Header("Keybinds")]
        public KeyCode JumpKey = KeyCode.Space;
        [HideInInspector]
        public bool RequestingJump;
        public KeyCode SprintKey = KeyCode.LeftShift;
        [HideInInspector]
        public bool SprintKeyHold;

        // Update is called once per frame
        void Update()
        {
            bool MouseVisible = LockMouse && !InMenu();
            Cursor.lockState = (MouseVisible) ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !MouseVisible;

            MouseX = (InvertMouse) ? -Input.GetAxis(MouseXAxis) : Input.GetAxis(MouseXAxis);
            MouseY = (InvertMouse) ? -Input.GetAxis(MouseYAxis) : Input.GetAxis(MouseYAxis);

            Hori = (!Smooth) ? Input.GetAxisRaw(HoriAxis) : Input.GetAxis(HoriAxis);
            Vert = (!Smooth) ? Input.GetAxisRaw(VertAxis) : Input.GetAxis(VertAxis);
            AltHori = (!Smooth) ? Input.GetAxisRaw(AltHoriAxis) : Input.GetAxis(AltHoriAxis);

            RequestingJump = Input.GetKeyDown(JumpKey);
            SprintKeyHold = Input.GetKey(SprintKey);
        }

        public static PlayerInputReciever GetPlayerInputRecieverInstance()
        {
            PlayerInputReciever InputReciever = null;
            PlayerInputReciever FoundInstance = FindObjectOfType<PlayerInputReciever>();
            if(FoundInstance == null) { InputReciever = new GameObject("Player Input Manager").AddComponent<PlayerInputReciever>(); }
            else { InputReciever = FoundInstance; }

            return InputReciever;
        }
    }
}