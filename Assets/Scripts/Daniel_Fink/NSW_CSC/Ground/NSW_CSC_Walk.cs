using NSW_CSC;
using UnityEngine;

namespace NSW_CSC
{
    public class NSW_CSC_Walk : IState
    {
        private readonly IStateContext context;

        public NSW_CSC_Walk(IStateContext context)
        {
            this.context = context;
        }

        public void OnEnter()
        {
            context.SetMovementModifiers(4f, 7f, 6f);
            context.SetCameraShakeModifiers(0.006f);
        }

        public void OnFixedUpdate() { }

        public void OnUpdate() { }

        public void OnExit() { }

        public string CheckTransitions()
        {
            if (context.MovementInput == Vector2.zero)
                return "Idle";

            if (context.IsRun && (context.MovementInput.y >= 0.99f))
                return "Run";

            if (!context.IsFoothold())
                return "Falling";

            return null;
        }
    }
}



