using System;
using System.Collections;
using System.Collections.Generic;
using NSW_CSC;
using NSW_Utilities;
using Unity.VisualScripting;
using UnityEngine;

namespace NSW_CSC
{
    [RequireComponent(typeof(NSW_CSC_InputHandler))]
    public class NSW_CSC_PlayerState : MonoBehaviour, IStateDataProvider
    {
        public IInputDataProvider inputProvider;
        public void InputInit(IInputDataProvider inputProvider)
        {
            this.inputProvider = inputProvider;
        }

        public IMovementDataProvider movementProvider;
        public void MovementInit(IMovementDataProvider movementProvider)
        {
            this.movementProvider = movementProvider;
        }

        public NSW_CSC_StateController stateController { get; private set; }
        private NSW_CSC_StateContext context;
        public NSW_CommandDispatcher commandDispatcher { get; private set; }

        [HideInInspector] public float SpeedModifier = 0f;
        [HideInInspector] public float AccelerationModifier = 0f;
        [HideInInspector] public float TurnSlowdownModifier = 0f;
        [HideInInspector] public float ShakerAmplitude = 0f;

        float IStateDataProvider.SpeedModifier => SpeedModifier;
        float IStateDataProvider.AccelerationModifier => AccelerationModifier;
        float IStateDataProvider.TurnSlowdownModifier => TurnSlowdownModifier;
        float IStateDataProvider.ShakerAmplitude => ShakerAmplitude;
        NSW_CommandDispatcher IStateDataProvider.CommandDispatcher => commandDispatcher;

        void Awake()
        {
            context = new NSW_CSC_StateContext(this);
            commandDispatcher = new NSW_CommandDispatcher();

            stateController = new NSW_CSC_StateController();
            StateCache();
            stateController.SetState("Idle");
        }

        private void StateCache()
        {
            stateController.AddState("Idle", new NSW_CSC_Idle(context));
            stateController.AddState("Walk", new NSW_CSC_Walk(context));
            stateController.AddState("Run", new NSW_CSC_Run(context));

            stateController.AddState("Landing", new NSW_CSC_Landing(context));
            stateController.AddState("Falling", new NSW_CSC_Falling(context));

            stateController.AddState("Jump", new NSW_CSC_Jump(context, commandDispatcher));
        }

        void Update()
        {
            stateController?.Update();
        }

        void FixedUpdate()
        {
            stateController?.FixedUpdate();
            commandDispatcher.ExecutePending();
        }
    }
}

public interface IStateDataProvider
{
    NSW_CommandDispatcher CommandDispatcher { get; }
    float SpeedModifier { get; }
    float AccelerationModifier { get; }
    float TurnSlowdownModifier { get; }
    float ShakerAmplitude { get; }
}

