using NSW_CSC;
using UnityEngine;

namespace NSW_CSC
{
    public class NSW_CSC_Run : IState
    {
        private readonly IStateContext context;

        public NSW_CSC_Run(IStateContext context)
        {
            this.context = context;
        }

        public void OnEnter()
        {
            context.SetMovementModifiers(13f, 8f, 8f);
            context.SetCameraShakeModifiers(0.01f);
        }

        public void OnFixedUpdate() { }

        public void OnUpdate() { }

        public void OnExit() { }

        public string CheckTransitions()
        {
            if (!context.IsRun)
                return "Walk";

            if (context.MovementInput == Vector2.zero)
                return "Idle";

            if (context.MovementInput.y <= 0.99f)
                return "Walk";

            if (!context.IsFoothold())
                return "Falling";

            if (context.IsJump)
                return "Jump";

            return null;
        }
    }
}



