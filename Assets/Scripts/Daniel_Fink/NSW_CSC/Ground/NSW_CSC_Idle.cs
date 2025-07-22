using UnityEngine;

namespace NSW_CSC
{
    public class NSW_CSC_Idle : IState
    {
        private readonly IStateContext context;

        public NSW_CSC_Idle(IStateContext context)
        {
            this.context = context;
        }

        public void OnEnter()
        {
            context.SetMovementModifiers(0f, 25f, 8f);
            context.SetCameraShakeModifiers(0);
        }

        public void OnFixedUpdate() { }

        public void OnUpdate() { }

        public void OnExit() { }

        public string CheckTransitions()
        {
            if (!context.IsFoothold())
                return "Falling";

            if (context.IsCrouch)
                return "Crouch_Idle";

            if (context.MovementInput != Vector2.zero)
                return context.IsRun ? "Run" : "Walk";

            return null;
        }
    }
}

