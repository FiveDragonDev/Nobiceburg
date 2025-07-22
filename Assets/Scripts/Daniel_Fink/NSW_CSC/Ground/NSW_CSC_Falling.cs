using NSW_CSC;
using UnityEngine;

namespace NSW_CSC
{
    public class NSW_CSC_Falling : IState
    {
        private readonly IStateContext context;

        public NSW_CSC_Falling(IStateContext context)
        {
            this.context = context;
        }

        public void OnEnter()
        {
            context.SetMovementModifiers(0f, 8f, 1f);
            context.SetCameraShakeModifiers(0.01f);
        }

        public void OnFixedUpdate() { }

        public void OnUpdate() { }

        public void OnExit() { }

        public string CheckTransitions()
        {
            if(!context.IsJump && context.IsFoothold())
                return "Landing";

            return null;
        }
    }
}