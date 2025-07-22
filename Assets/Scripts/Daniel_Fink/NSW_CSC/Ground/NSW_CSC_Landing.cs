using NSW_CSC;
using UnityEngine;

namespace NSW_CSC
{
    public class NSW_CSC_Landing : IState
    {
        private readonly IStateContext context;

        public NSW_CSC_Landing(IStateContext context)
        {
            this.context = context;
        }

        public void OnEnter()
        {
            context.SetMovementModifiers(0f, 7f, 1f);
            context.SetCameraShakeModifiers(0.1f);
        }

        public void OnFixedUpdate() { }

        public void OnUpdate() { }

        public void OnExit() { }

        public string CheckTransitions()
        {
            if (context.IsFoothold())
                return "Idle";

            return null;
        }
    }
}
