using UnityEngine;

public sealed class JoystickInputAdapter : MonoBehaviour, IPlayerInput
{
    [SerializeField] private Joystick joystick;

    public PlayerCommand Read()
    {
        PlayerCommand cmd = new PlayerCommand();

        Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);

        if (input.sqrMagnitude < 0.01f)
            input = Vector2.zero;

        cmd.MoveAxis = input;
        return cmd;
    }

    public void ShowJoystick(bool toggle) => joystick.gameObject.SetActive(toggle);
}