using System;
using System.Collections;
using System.Collections.Generic;
using NSW_Utilities;
using UnityEngine;

namespace NSW_CSC
{
    public class NSW_CSC_StateController
    {
        private Dictionary<string, IState> states = new();
        private IState currentState;
        [HideInInspector] public string currentStateName { get; private set; }

        public void AddState(string name, IState state)
        {
            states[name] = state;
        }

        public void SetState(string name)
        {
            if (states.TryGetValue(name, out var nextState))
            {
                currentState?.OnExit();
                currentState = nextState;
                currentStateName = name;
                currentState.OnEnter();
            }
        }

        public void Update()
        {
            currentState?.OnUpdate();
            string next = currentState?.CheckTransitions();
            if (!string.IsNullOrEmpty(next) && next != currentStateName)
            {
                SetState(next);
            }
        }

        public void FixedUpdate()
        {
            currentState?.OnFixedUpdate();
        }
    }


    public interface IState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
        void OnFixedUpdate();
        string CheckTransitions();
    }

    public interface IStateContext
    {
        Vector2 MovementInput { get; }
        bool IsCrouch { get; }
        bool IsRun { get; }
        bool IsJump { get; }
        bool IsFoothold();
        void SetState(string state);
        void SetMovementModifiers(float speed, float acceleration, float turnSlowdown);
        void SetCameraShakeModifiers(float shakerAmplitude);
    }
}

