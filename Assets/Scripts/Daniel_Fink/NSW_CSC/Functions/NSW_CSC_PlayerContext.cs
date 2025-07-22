using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSW_CSC;
using NSW_Utilities;

public class NSW_CSC_PlayerContext : MonoBehaviour
{
    private NSW_CSC_PlayerState State;
    private NSW_CSC_MovementControler MovementControler;
    private NSW_CSC_CameraController CameraController;
    private NSW_CSC_InputHandler InputHandler;
    private NSW_CSC_JumpMovement JumpMovement;
    private NSW_CSC_PlayerAvatar PlayerAvatar;

    void Awake()
    {
        State = GetComponent<NSW_CSC_PlayerState>();
        MovementControler = GetComponent<NSW_CSC_MovementControler>();
        CameraController = GetComponent<NSW_CSC_CameraController>();
        InputHandler = GetComponent<NSW_CSC_InputHandler>();
        JumpMovement = GetComponent<NSW_CSC_JumpMovement>();
        PlayerAvatar = GetComponent<NSW_CSC_PlayerAvatar>();

        MovementControler.StatesInit(State);
        CameraController.StateInit(State);
        JumpMovement.StatesInit(State);
        PlayerAvatar.StatesInit(State);

        State.InputInit(InputHandler);
        MovementControler.InputInit(InputHandler);
        CameraController.InputInit(InputHandler);
        PlayerAvatar.InputInit(InputHandler);

        State.MovementInit(MovementControler);
        CameraController.MovementInit(MovementControler);
        PlayerAvatar.MovementInit(MovementControler);

        MovementControler.CameraInit(CameraController);
        PlayerAvatar.CameraInit(CameraController);
    }
}

