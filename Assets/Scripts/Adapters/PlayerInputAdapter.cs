using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputAdapter : MonoBehaviour, IPlayerInput
{
    private PlayerInputActions actions;

    public PlayerCommand Read()
    {
        PlayerCommand cmd = new PlayerCommand();

        Vector2 input = actions.Player.MoveCommand.ReadValue<Vector2>();
        cmd.MoveAxis = input;

        return cmd;
    }

    private void Awake()
    {
        EnsureActions();
    }

    private void OnEnable()
    {
        EnsureActions();
        actions.Enable();
    }

    private void OnDisable()
    {
        if (actions != null)
            actions.Disable();
    }

    private void EnsureActions()
    {
        if (actions == null)
            actions = new PlayerInputActions();
    }
}