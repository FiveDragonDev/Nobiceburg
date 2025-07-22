using System.Collections;
using System.Collections.Generic;
using NSW_CSC;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class NSW_CSC_CameraController : MonoBehaviour, ICameraDataProvider
{
    private IStateDataProvider stateProvider;
    private IInputDataProvider inputProvider;
    private IMovementDataProvider movementDataProvider;
    public void StateInit(IStateDataProvider stateProvider)
    {
        this.stateProvider = stateProvider;
    }
    public void InputInit(IInputDataProvider inputProvider)
    {
        this.inputProvider = inputProvider;
    }
    public void MovementInit(IMovementDataProvider movementDataProvider)
    {
        this.movementDataProvider = movementDataProvider;
    }
    public NSW_CSC_MovementControler movement { get; private set; }

    [Header("Camera Transforms")]
    public Transform CameraHolder;
    public Transform CameraShaker;
    public Transform CameraStabilizer;
    public Transform Camera;

    [Header("Camera")]
    [SerializeField] public bool IsCameraRotate = true;
    private bool _isCameraRotate
    {
        get => IsCameraRotate;
        set => IsCameraRotate = value;
    }
    [SerializeField] public bool IsCursorLocked = true;
    private bool _isCursorLocked
    {
        get => IsCursorLocked;
        set
        {
            IsCursorLocked = value;
            CursorLockState();
        }
    }
    [SerializeField] private float Sensivity = 20f;
    [SerializeField] private float yShift = 0f;
    [SerializeField] private float yTiltShift = 0.15f;
    [SerializeField] private float zTilt = 0f;
    [SerializeField] private float zShift = 0f;
    [SerializeField] private float zShiftOnRunning = 0f;
    [HideInInspector] public float CameraX { get; private set; }
    [HideInInspector] public float CameraY { get; private set; }

    [Header("Camera Shake")]
    [SerializeField] public float CameraSideTilt = 0f;
    [SerializeField] public float LowShakeSpeed = 0.1f;
    [SerializeField] public float RunSpeed = 12f;
    [SerializeField] public float ShakerFrequency = 15f;
    [SerializeField] public AnimationCurve JumpInertion;
    [SerializeField] public float VerticalInertionPower = 0.05f;
    [SerializeField] public bool ForceShake = true;

    private float CurrentShakerAmplitude = 0f;

    Transform ICameraDataProvider.CameraHolder => CameraHolder;
    float ICameraDataProvider.CameraX => CameraX;
    float ICameraDataProvider.CameraY => CameraY;
    bool ICameraDataProvider.IsCameraRotate { get => _isCameraRotate; set => _isCameraRotate = value; }
    bool ICameraDataProvider.IsCursorLocked { get => _isCursorLocked; set => _isCursorLocked = value; }

    private Rigidbody Rigidbody;

    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        CursorLockState();
    }
    // Update is called once per frame
    void Update()
    {
        CameraRotation();
    }

    void FixedUpdate()
    {
        CameraShake();
    }

    void CursorLockState()
    {
        Cursor.lockState = IsCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = IsCursorLocked ? false : true;
    }

    #region Camera Movements
    private void CameraRotation()
    {
        if (!IsCameraRotate) return;

        float mouseX = inputProvider.CameraInput.x * Time.deltaTime * Sensivity;
        float mouseY = -inputProvider.CameraInput.y * Time.deltaTime * Sensivity;

        CameraX += mouseX;
        CameraY += mouseY;
        CameraY = Mathf.Clamp(CameraY, -89f, 89f);

        if (CameraX > 360 || CameraX < -360)
            CameraX = 0;

        float CameraYInverse = Mathf.Clamp((CameraY + 89f) / 178f, 0f, 1f); ;

        float zShiftShifting = inputProvider.IsRun && Rigidbody.linearVelocity.magnitude > Mathf.Epsilon? zShiftOnRunning : zShift;
        float yShiftVerticalShifing = Mathf.Lerp(yShift + yTiltShift, yShift - yTiltShift, CameraYInverse);

        CameraHolder.SetLocalPositionAndRotation(new Vector3(0f, movementDataProvider.CurrentCharacterCapsuleHeight / 2 - yShiftVerticalShifing, 0f) + movementDataProvider.facingVectorForward * zShiftShifting, Quaternion.Euler(CameraY, CameraX, zTilt));
    }

    #endregion

    #region Camera Shaker
    void CameraShake()
    {
        Vector3 speedVector = Rigidbody.linearVelocity;
        float speed = speedVector.magnitude;
        CameraResetShaker();

        if (speedVector.magnitude < Mathf.Epsilon) return;

        if (CameraShaker.localPosition == Vector3.zero)
            CameraShaker.localPosition = Vector3.Lerp(CameraShaker.localPosition, Vector3.zero, 10 * Time.fixedDeltaTime);

        Camera.LookAt(CameraStabilizer.position);
        Camera.localRotation = Camera.localRotation * Quaternion.Euler(new Vector3(0f, 0f, CameraSideTilt));

        float LerpSpeedNormalize = Mathf.Clamp01(speed / stateProvider.SpeedModifier);
        float ShakerAmplitude = Mathf.Lerp(CurrentShakerAmplitude, stateProvider.ShakerAmplitude, LerpSpeedNormalize);
        ShakerLogic(spaceNoiceGenerator(ShakerAmplitude));
    }

    void ShakerLogic(Vector3 noise)
    {
        CameraShaker.localPosition += noise;
    }

    private Vector3 spaceNoiceGenerator(float shakerAmplitude)
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * ShakerFrequency) * shakerAmplitude;
        pos.x += Mathf.Cos(Time.time * ShakerFrequency / 1.5f) * shakerAmplitude * 1.5f;
        return pos;
    }

    private void CameraResetShaker()
    {
        if (CameraShaker.localPosition == Vector3.zero) return;
        CameraShaker.localPosition = Vector3.Lerp(CameraShaker.localPosition, Vector3.zero, 10 * Time.fixedDeltaTime);
    }

    #endregion
}

public interface ICameraDataProvider
{
    Transform CameraHolder { get; }
    float CameraX { get; }
    float CameraY { get; }
    bool IsCameraRotate { get; set; }
    bool IsCursorLocked { get; set; }
}
