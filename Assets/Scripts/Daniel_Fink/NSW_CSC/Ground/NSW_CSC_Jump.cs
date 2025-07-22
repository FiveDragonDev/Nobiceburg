using NSW_CSC;
using NSW_Utilities;
using UnityEngine;

namespace NSW_CSC
{
    public class NSW_CSC_Jump : IState
    {
        private readonly IStateContext context;
        private readonly ICommandDispatcher commandDispatecher;

        public NSW_CSC_Jump(IStateContext context, ICommandDispatcher commandDispatecher)
        {
            this.context = context;
            this.commandDispatecher = commandDispatecher;
        }

        bool isJumped = false;

        public void OnEnter()
        {
            context.SetMovementModifiers(0f, 0f, 0f);
            context.SetCameraShakeModifiers(0.05f);
            
            commandDispatecher.Call("Jump");
            isJumped = true;
        }

        public void OnFixedUpdate() { }

        public void OnUpdate() { }

        public void OnExit() 
        {
            isJumped = false;
        }

        public string CheckTransitions()
        {
            if (isJumped && context.IsJump)
                return "Falling";

            return null;
        }
    }
}
