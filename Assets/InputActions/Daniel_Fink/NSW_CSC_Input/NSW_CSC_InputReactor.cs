using UnityEngine;

namespace NSW_CSC
{
    public class NSW_CSC_InputReactor : ScriptableObject
    {
        public NSW_CSC_InputKeymap InputKeymap;
        public NSW_CSC_InputKeymap.PlayerControlsActions PlayerControls;

        public void Define()
        {
            InputKeymap = new NSW_CSC_InputKeymap();
            PlayerControls = InputKeymap.PlayerControls;
        }

        public void Enable()
        {
            InputKeymap.Enable();
        }

        public void Disable()
        {
            InputKeymap.Disable();
        }
    }
}

