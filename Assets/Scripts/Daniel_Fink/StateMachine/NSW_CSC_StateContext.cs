using System;
using System.Collections;
using System.Collections.Generic;
using NSW_Utilities;
using UnityEngine;

namespace NSW_CSC
{
    public class NSW_CSC_StateContext : IStateContext
    {
        private readonly NSW_CSC_PlayerState state;

        public Vector2 MovementInput => state.inputProvider.MovementInput;
        public bool IsCrouch => state.inputProvider.IsCrouch;
        public bool IsRun => state.inputProvider.IsRun;
        public bool IsJump => state.inputProvider.isJump;

        private IMovementDataProvider movementDataProvider => state.movementProvider;

        public NSW_CSC_StateContext(NSW_CSC_PlayerState state)
        {
            this.state = state;
        }

        public bool IsGrounded()
        {
            return state.movementProvider.IsFoothold;
        }

        public void SetState(string name)
        {
            state.stateController.SetState(name);
        }

        public void SetMovementModifiers(float speed, float acceleration, float turnSlowdown)
        {
            state.SpeedModifier = speed;
            state.AccelerationModifier = acceleration;
            state.TurnSlowdownModifier = turnSlowdown;
        }

        public void SetCameraShakeModifiers(float shakerAmplitude)
        {
            state.ShakerAmplitude = shakerAmplitude;
        }

        public bool IsFoothold()
        {
            return movementDataProvider.IsFoothold;
        }
    }
}

