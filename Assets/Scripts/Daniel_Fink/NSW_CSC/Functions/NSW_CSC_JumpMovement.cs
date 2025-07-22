using System.Collections;
using System.Collections.Generic;
using NSW_CSC;
using NSW_Utilities;
using UnityEngine;

public class NSW_CSC_JumpMovement : MonoBehaviour
{
    private IStateDataProvider stateProvider;
    public void StatesInit(IStateDataProvider stateProvider)
    {
        this.stateProvider = stateProvider;
    }

    private Rigidbody Rigidbody;

    public float JumpUpValue = 0f;
    public float JumpForwardValue = 0f;

    public Transform CameraHolder;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        stateProvider.CommandDispatcher.RegisterCommand("Jump", () => DoJump());
    }

    public void DoJump()
    {   
        Vector3 characterForward = Vector3.Cross(CameraHolder.right, this.transform.up);

        Vector3 upVector = Rigidbody.transform.up * JumpUpValue;
        Vector3 forwardVector = characterForward * (Rigidbody.linearVelocity.magnitude * JumpForwardValue);

        Rigidbody.AddForce(upVector + forwardVector, ForceMode.VelocityChange);
    }
}