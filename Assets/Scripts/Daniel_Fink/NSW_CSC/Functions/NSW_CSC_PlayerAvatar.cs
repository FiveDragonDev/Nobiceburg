using System.Collections;
using System.Collections.Generic;
using NSW_CSC;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace NSW_CSC
{
    public class NSW_CSC_PlayerAvatar : MonoBehaviour
    {
        private IStateDataProvider stateProvider;
        private IInputDataProvider inputProvider;
        private ICameraDataProvider cameraDataProvider;
        private IMovementDataProvider movementDataProvider;
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
        public void MovementInit(IMovementDataProvider movementDataProvider)
        {
            this.movementDataProvider = movementDataProvider;
        }

        public float ScanDistance = 1.2f;
        public float AdjustAvatarGround;
        public LayerMask WorldScan;

        public GameObject AvatarModel;
        public Animator Animator;
        private Rigidbody Rigidbody;

        public float WalkSpeedAdjustment = 1f;

        public float TurnAngle = 45f;
        public float TurnIdleSpeed = 2f;

        public float RunSpeed = 3f;

        private float _animSpeedMul = 0f;
        private float AnimationSpeedMultiplier { get => _animSpeedMul; set => _animSpeedMul = Mathf.Clamp01(value); }
        private float _feetProceduralSpeedMul = 0f;
        private float FeetProceduralSpeedMultiplier { get => _feetProceduralSpeedMul; set => _feetProceduralSpeedMul = Mathf.Clamp01(value); }
        public float AnimationAcceleration;
        public float AnimationDecceleration;
        public float ProceduralFeetAnimationAcceleration;
        public float ProceduralFeetAnimationDecceleration;

        public GameObject RigCore;
        public GameObject RightFeetTransform, LeftFeetTransform;
        private TwoBoneIKConstraint RightFeetIK, LeftFeetIK;
        public Vector3 RightFootInitialPosition, LeftFootInitialPosition;
        private Vector3 RightFootLastPosition, LeftFootLastPosition;
        private bool IsFeetRec = false;

        [SerializeField]
        [Range(0f, 1f)] public float FeetOverlapLerp = 0.5f;

        public float FeetMaxHeightOverlap;
        public AnimationCurve FeetAliginigCurve;
        public float FeetSwitchingPrecision = 0.05f;

        bool farIsRight = false;
        bool farIsLeft = false;

        private void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();

            RightFeetIK = RightFeetTransform.GetComponent<TwoBoneIKConstraint>();
            LeftFeetIK = LeftFeetTransform.GetComponent<TwoBoneIKConstraint>();

            RightFeetIK.weight = 0f; LeftFeetIK.weight = 0f;

            RightFootInitialPosition = RigCore.transform.InverseTransformPoint(RightFeetIK.data.tip.position);
            LeftFootInitialPosition = RigCore.transform.InverseTransformPoint(LeftFeetIK.data.tip.position);

            RightFeetIK.transform.position = RigCore.transform.TransformPoint(RightFootInitialPosition);
            LeftFeetIK.transform.position = RigCore.transform.TransformPoint(LeftFootInitialPosition);

            RightFeetIK.transform.rotation = RightFeetIK.data.tip.rotation;
            LeftFeetIK.transform.rotation = LeftFeetIK.data.tip.rotation;
        }

        void Update()
        {
            Avatar_WorldAlign();
            Avatar_Animation();

            Debug.DrawRay(RigCore.transform.TransformPoint(RightFootInitialPosition), transform.up, Color.green);
        }

        private void FixedUpdate()
        {
            Avatar_FeetPlacement();
            Avatar_AnimationSpeedMultiplierEval();
        }

        private void Avatar_WorldAlign()
        {
            RaycastHit AvatarAlignHit;
            Physics.SphereCast(this.transform.position, 0.24f, -this.transform.up, out AvatarAlignHit, ScanDistance, WorldScan);
            Debug.DrawRay(this.transform.position, -this.transform.up * ScanDistance);
            Debug.DrawRay(this.transform.position, Vector3.Cross(cameraDataProvider.CameraHolder.right, this.transform.up));

            if(movementDataProvider.IsFoothold == true)
                AvatarModel.transform.localPosition = (AvatarAlignHit.point - this.transform.position) + new Vector3(0f, AdjustAvatarGround, 0f);
            else
                AvatarModel.transform.localPosition = AvatarModel.transform.localPosition + new Vector3(0f, AdjustAvatarGround, 0f);

            if (movementDataProvider.movementSpeedVector.magnitude > Mathf.Epsilon && Rigidbody.linearVelocity.magnitude > 0.1f)
                AvatarModel.transform.localRotation = Quaternion.LookRotation(movementDataProvider.movementSpeedVector);
            else
            {
                Vector3 fwd = Vector3.Cross(cameraDataProvider.CameraHolder.right, this.transform.up);
                Vector3 avi_fwd = AvatarModel.transform.forward;
                Debug.DrawRay(this.transform.position, avi_fwd);
                float fwd_diff = Vector3.Angle(avi_fwd, fwd);

                if (fwd_diff > TurnAngle)
                    AvatarModel.transform.localRotation = Quaternion.Lerp(AvatarModel.transform.localRotation, Quaternion.LookRotation(Vector3.Cross(cameraDataProvider.CameraHolder.right, this.transform.up)), Time.deltaTime * TurnIdleSpeed);
            }
        }

        private void Avatar_Animation()
        {
            Animator.speed = Mathf.Lerp(0, WalkSpeedAdjustment, AnimationSpeedMultiplier);

            Animator.SetFloat("Idle2Motion", Mathf.Clamp01(AnimationSpeedMultiplier));
            Animator.SetFloat("Walk2Run", inputProvider.IsRun && Rigidbody.linearVelocity.magnitude > RunSpeed ? Mathf.Clamp01(AnimationSpeedMultiplier) : 0);
        }

        private void Avatar_AnimationSpeedMultiplierEval()
        {
            float movementSpeed = GetPlayerVelocity().magnitude;
            AnimationSpeedMultiplier += movementSpeed > Mathf.Epsilon ? AnimationAcceleration : -AnimationDecceleration;
            FeetProceduralSpeedMultiplier += movementSpeed > Mathf.Epsilon ? ProceduralFeetAnimationAcceleration : -ProceduralFeetAnimationDecceleration;
        }

        public Vector3 GetPlayerVelocity()
        {
            Vector3 playerXZVelocity = Rigidbody.linearVelocity;
            playerXZVelocity.y = 0f;
            return playerXZVelocity;
        }

        private void Avatar_FeetPlacement()
        {
            float movementSpeed = GetPlayerVelocity().magnitude;
            float rightFeetDist = 0f;
            float leftFeetDist = 0f;

            if (inputProvider.MovementInput.magnitude < Mathf.Epsilon && IsFeetRec == false)
            {
                RightFootLastPosition = RigCore.transform.InverseTransformPoint(RightFeetIK.data.tip.position);
                LeftFootLastPosition = RigCore.transform.InverseTransformPoint(LeftFeetIK.data.tip.position);

                rightFeetDist = Vector3.Distance(RightFootInitialPosition, RightFootLastPosition);
                leftFeetDist = Vector3.Distance(LeftFootInitialPosition, LeftFootLastPosition);

                if (!farIsRight || !farIsLeft)
                {
                    farIsRight = rightFeetDist > leftFeetDist;
                    farIsLeft = leftFeetDist > rightFeetDist;
                }

                IsFeetRec = true;
            }

            if (inputProvider.MovementInput.magnitude > Mathf.Epsilon)
            {
                IsFeetRec = false;
                farIsRight = false; farIsLeft = false;
            }

            Vector3 rightFootMiddlePoint = Vector3.Lerp(RightFootInitialPosition, RightFootLastPosition, FeetOverlapLerp) + this.transform.up * FeetMaxHeightOverlap;
            Vector3 leftFootMiddePoint = Vector3.Lerp(LeftFootInitialPosition, LeftFootLastPosition, FeetOverlapLerp) + this.transform.up * FeetMaxHeightOverlap;

            Debug.DrawRay(RigCore.transform.TransformPoint(RightFootLastPosition), transform.up, Color.red);

            RightFeetIK.weight = 1.0f - AnimationSpeedMultiplier; LeftFeetIK.weight = 1.0f - AnimationSpeedMultiplier;

            void FeetFiniting(TwoBoneIKConstraint feetIK, Vector3 initialPosition, Vector3 middlePosition, Vector3 finitePosition)
            {
                feetIK.data.target.position = RigCore.transform.TransformPoint(EvaluateQuadraticCurve(initialPosition, middlePosition, finitePosition, FeetAliginigCurve.Evaluate(FeetProceduralSpeedMultiplier)));
            }

            bool FeetDistanceCompare(Vector3 feetTarget, Vector3 initialPosition)
            {
                return Vector3.Distance(initialPosition, RigCore.transform.InverseTransformPoint(feetTarget)) < FeetSwitchingPrecision;
            }

            if (farIsRight)
            {
                FeetFiniting(RightFeetIK, RightFootInitialPosition, rightFootMiddlePoint, RightFootLastPosition);
                farIsLeft = FeetDistanceCompare(RightFeetIK.data.target.position, RightFootInitialPosition);
            }

            if (farIsLeft)
            {
                FeetFiniting(LeftFeetIK, LeftFootInitialPosition, leftFootMiddePoint, LeftFootLastPosition);
                farIsRight = FeetDistanceCompare(LeftFeetIK.data.target.position, LeftFootInitialPosition);
            }
        }

        private Vector3 EvaluateQuadraticCurve(Vector3 StartPoint, Vector3 middleShift, Vector3 EndPoint, float LerpTime)
        {
            Vector3 ac = Vector3.Lerp(StartPoint, middleShift, LerpTime);
            Vector3 cb = Vector3.Lerp(middleShift, EndPoint, LerpTime);
            return Vector3.Lerp(ac, cb, LerpTime);
        }

    }
}

