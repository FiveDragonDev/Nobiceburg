using System.Collections;
using System.Collections.Generic;
using NSW_CSC;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NSW_CSC
{
    public class NSW_CSC_InputHandler : MonoBehaviour, IInputDataProvider
    {
        public NSW_CSC_InputReactor InputReactor { get; private set; }
        public NSW_CSC_MovementControler MovementControler { get; private set; }
        public NSW_CSC_CameraController CameraController { get; private set; }

        [HideInInspector] public bool IsRun = false;
        [HideInInspector] public bool IsCrouch = false;
        [HideInInspector] public bool isJump = false;

        [HideInInspector] public Vector2 MovementInput;
        [HideInInspector] public Vector2 CameraInput;

        Vector2 IInputDataProvider.MovementInput => MovementInput;
        Vector2 IInputDataProvider.CameraInput => CameraInput;
        bool IInputDataProvider.IsCrouch => IsCrouch;
        bool IInputDataProvider.isJump => isJump;
        bool IInputDataProvider.IsRun => IsRun;
        public bool ConsoleItemRelease => throw new System.NotImplementedException();
        public bool SendComandlette => throw new System.NotImplementedException();

        void Awake()
        {
            InputReactor = ScriptableObject.CreateInstance<NSW_CSC_InputReactor>();
            InputReactor.Define();
            MovementControler = GetComponent<NSW_CSC_MovementControler>();
            MovementControler.InputInit(this);
            CameraController = GetComponent<NSW_CSC_CameraController>();
            CameraController.InputInit(this);
        }

        void OnEnable()
        {
            InputReactor.Enable();
            InputReactor.InputKeymap.PlayerControls.Jump.performed += JumpPerformed;
            InputReactor.InputKeymap.PlayerControls.Jump.canceled += JumpCanceled;
        }

        void OnDisable()
        {
            InputReactor.Disable();
            InputReactor.InputKeymap.PlayerControls.Jump.performed -= JumpPerformed;
            InputReactor.InputKeymap.PlayerControls.Jump.canceled -= JumpCanceled;
        }

        void Update()
        {
            MovementInput = InputReactor.InputKeymap.PlayerControls.WalkRun.ReadValue<Vector2>();
            CameraInput = InputReactor.InputKeymap.PlayerControls.Camera.ReadValue<Vector2>();
            IsCrouch = InputReactor.InputKeymap.PlayerControls.Crouch.ReadValue<float>() == 1 ? true : false;
            IsRun = InputReactor.InputKeymap.PlayerControls.Run.ReadValue<float>() == 1 ? true : false;
        }

        private void JumpPerformed(InputAction.CallbackContext context)
        {
            isJump = true;
        }

        private void JumpCanceled(InputAction.CallbackContext context)
        {
            isJump = false;
        }

        public bool IsGrounded()
        {
            throw new System.NotImplementedException();
        }
    }
}

public interface IInputDataProvider
{
    Vector2 MovementInput { get; }
    Vector2 CameraInput { get; }
    bool IsCrouch { get; }
    bool isJump { get; }
    bool IsRun { get; }
    bool ConsoleItemRelease { get; }
    bool SendComandlette { get; }
    bool IsGrounded();
}
