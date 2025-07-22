using System.Collections;
using System.Collections.Generic;
using NSW_CSC;
using UnityEngine;

public class NSW_CSC_MovementControler : MonoBehaviour, IMovementDataProvider
{
    private IStateDataProvider stateProvider;
    private IInputDataProvider inputProvider;
    private ICameraDataProvider cameraDataProvider;
    public void StatesInit(IStateDataProvider stateProvider)
    {
        this.stateProvider = stateProvider;
    }
    public void InputInit(IInputDataProvider inputProvider)
    {
        this.inputProvider = inputProvider;
    }
    public void CameraInit(ICameraDataProvider cameraDataProvider)
    {
        this.cameraDataProvider = cameraDataProvider;
    }

    [HideInInspector] public Rigidbody Rigidbody { get; private set; }
    [HideInInspector] public CapsuleCollider CapsuleCollider { get; private set; }

    [Header("Capsule Parameters")]
    [SerializeField] public float DefaultCharacterCapsuleHeight = 1.2f;
    [HideInInspector] public float CurrentCharacterCapsuleHeight = 1.2f;

    [Header("Levitation")]
    [SerializeField] private LayerMask Hover_LayerMask;
    [SerializeField] public float Hover_TargetHeight = 0.4f;
    [SerializeField] public float Hover_SpherecastRadius = 0.125f;
    [SerializeField] public float Hover_StepReachForce = 25f;
    [SerializeField] public float Hover_ScanVerticalAdjust = 0.25f;
    [SerializeField] public bool IsHover = true;
    private bool _isHover
    {
        get => IsHover;
        set => IsHover = value;
    }

    [Header("Movement")]
    [SerializeField] public bool IsMove = true;
    private bool _isMove
    {
        get => IsMove;
        set => IsMove = value;
    }

    [HideInInspector] public Vector3 facingVectorForward;
    [HideInInspector] public Vector3 movementSpeedVector { get; private set; }
    [HideInInspector] public Vector3 forwardCachedVector { get; private set; }
    [HideInInspector] public Vector3 velocityCachedVector { get; private set; }
    private float currentSpeedModifier = 0f;

    float IMovementDataProvider.DefaultCharacterCapsuleHeight => DefaultCharacterCapsuleHeight;
    float IMovementDataProvider.CurrentCharacterCapsuleHeight => CurrentCharacterCapsuleHeight;
    Vector3 IMovementDataProvider.facingVectorForward => facingVectorForward;
    Vector3 IMovementDataProvider.movementSpeedVector => movementSpeedVector;
    bool IMovementDataProvider.IsHover { get => _isHover; set => _isHover = value; }
    bool IMovementDataProvider.IsMove { get => _isMove; set => _isMove = value; }

    IStateDataProvider IMovementDataProvider.StateProvider { get => stateProvider; set => stateProvider = value; }
    public bool IsFoothold { get => Hover_IsFoothold(); }

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        CapsuleCollider = GetComponent<CapsuleCollider>();

        facingVectorForward = Vector3.forward;
    }

    private void FixedUpdate()
    {
        Movement();
        Hover();
    }

    #region Locomotion methods
    private void Movement()
    {
        facingVectorForward = GetRotationVector(cameraDataProvider.CameraHolder);
        Move(MovementAccelerate(stateProvider.SpeedModifier, stateProvider.AccelerationModifier));
    }

    private void Move(float playerSpeed)
    {
        if (!IsMove) return;
        velocityCachedVector = GetHorizontalVector();
        Vector3 characterVelocity = GetPlayerVelocity();

        Vector3 movementSpeedDirection = GetMovementDirection();

        movementSpeedVector = Vector3.Lerp(movementSpeedVector, movementSpeedDirection, stateProvider.TurnSlowdownModifier * Time.deltaTime);

        Vector3 currentMovementVector = inputProvider.MovementInput != Vector2.zero ? movementSpeedVector : velocityCachedVector;

        if (Hover_IsFoothold())
            Rigidbody.AddForce(currentMovementVector * playerSpeed - characterVelocity, ForceMode.VelocityChange);
    }

    private float MovementAccelerate(float _speedModifier, float _accelerationModifier)
    {
        //if (!Hover_IsFoothold()) return 0;

        currentSpeedModifier = Mathf.MoveTowards(currentSpeedModifier, _speedModifier, _accelerationModifier * Time.deltaTime);

        return Mathf.Clamp(currentSpeedModifier, 0, Mathf.Infinity);
    }
    #endregion

    #region Hover Methods
    private void Hover()
    {
        Hover_GroundCheck(out bool isGrounded, out RaycastHit hover_rayScanHit);
        if (isGrounded && IsHover) Hover_Levitate(hover_rayScanHit);
    }

    private void Hover_Levitate(RaycastHit rayHit)
    {
        float heightToFloat = Hover_HoverHeight() - rayHit.distance;

        if (heightToFloat == 0f) return;

        float amountToFloat = heightToFloat * Hover_StepReachForce - Rigidbody.linearVelocity.y;
        Vector3 liftForce = new Vector3(0f, amountToFloat, 0f);

        Rigidbody.AddForce(liftForce, ForceMode.VelocityChange);
    }

    float Hover_HoverHeight()
    {
        return Hover_TargetHeight + CurrentCharacterCapsuleHeight / 2 + 0.1f - Hover_SpherecastRadius;
    }

    void Hover_GroundCheck(out bool isGrounded, out RaycastHit rayHit)
    {
        RaycastHit outRay;
        Vector3 scanPos = this.transform.position + (-this.transform.up * Hover_ScanVerticalAdjust);
        Ray rayToGround = new Ray(scanPos, -this.transform.up);
        Debug.DrawRay(scanPos, -this.transform.up * (Hover_TargetHeight + CurrentCharacterCapsuleHeight / 2), Color.blue);
        isGrounded = Physics.SphereCast(rayToGround, Hover_SpherecastRadius, out outRay, Hover_TargetHeight + CurrentCharacterCapsuleHeight / 2, Hover_LayerMask);
        rayHit = outRay;
    }

    public bool Hover_IsFoothold()
    {
        Hover_GroundCheck(out bool isGrounded, out RaycastHit rayHit);
        return isGrounded;
    }

    #endregion

    #region Reusable methods

    public Vector3 GetPlayerVelocity()
    {
        Vector3 playerXZVelocity = Rigidbody.linearVelocity;
        playerXZVelocity.y = 0f;
        return playerXZVelocity;
    }

    public Vector3 GetRotationVector(Transform frontViewObject)
    {
        Vector3 playerFaced = Vector3.Cross(frontViewObject.right, Rigidbody.transform.up);
        return playerFaced;
    }

    private Vector3 GetHorizontalVector()
    {
        return new Vector3(Rigidbody.linearVelocity.normalized.x, 0f, Rigidbody.linearVelocity.normalized.z);
    }

    public Vector3 GetMovementDirection()
    {
        return facingVectorForward * inputProvider.MovementInput.y + Vector3.Cross(facingVectorForward, this.transform.up) * -inputProvider.MovementInput.x;
    }

    #endregion
}

public interface IMovementDataProvider
{
    float DefaultCharacterCapsuleHeight { get; }
    float CurrentCharacterCapsuleHeight { get; }
    Vector3 facingVectorForward { get; }
    Vector3 movementSpeedVector { get; }
    bool IsHover { get; set; }
    bool IsMove { get; set; }
    bool IsFoothold { get; }
    IStateDataProvider StateProvider { get; set; }
}
