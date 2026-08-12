using UnityEngine;
using UnityEngine.InputSystem;

public sealed class Bootstrap : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private GameLoop loop;
    [SerializeField] private UnitFactory factory;
    [SerializeField] private PlayerInputAdapter keyboardInput;
    [SerializeField] private JoystickInputAdapter joystickInput;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private GameResultUI gameResultUI;
    [SerializeField] private float startTimer;

    [Header("Placed units")]
    [SerializeField] private UnitBody playerBody;

    [Header("Stats (ScriptableObject)")]
    [SerializeField] private UnitStats playerStats;

    private void Awake()
    {
        World world = new World();
        Bindings bindings = new Bindings();
        world.Match.Timer = startTimer;

        factory.Init(world, bindings);

        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            bindings.PlayerInput = joystickInput;
            if (joystickInput != null)
                joystickInput.ShowJoystick(true);
        }
        else
        {
            bindings.PlayerInput = keyboardInput;
            if (joystickInput != null)
                joystickInput.ShowJoystick(false);
        }

        bindings.PlayerUI = playerUI;

        // TECT TECT TECT TECT TECT TECT TECT TECT TECT TECT
        world.Planet.Center = Vector3.zero;
        world.Planet.GravityStrength = 200f;
        world.Planet.Radius = 25;

        // Усыновляем то, что уже расставлено в сцене.
        if (playerBody != null)
            bindings.PlayerId = factory.RegisterBody(playerBody, playerStats, UnitKind.Player);

        factory.SpawnWave();

        PhysicsWriteSystem write = new PhysicsWriteSystem(world, bindings);

        ISystem[] fixedSystems =
        {
            new PhysicsReadSystem(world, bindings),
            new PlanetGravitySystem(world),
            new InteractionSystem(world),
            new MovementSystem(world),
            new AnimalMovementSystem(world, bindings),
            new CaughtSystem(world, factory),
            write
        };

        ISystem[] frameSystems =
        {
            new PlayerCommandSystem(world, bindings),
            new EndGameTimerSystem(world),
            new PlayerUISyncSystem(world, bindings),
            new ViewSyncSystem(world, bindings),
            new SoundSyncSystem(world, bindings),
            new MatchEndView(world, bindings, gameResultUI)
        };

        ISystem[] endedSystems =
        {
            new FreezeSystem(world),
            write
        };

        GameManager game = new GameManager(world, fixedSystems, frameSystems, endedSystems);
        loop.Bind(game);
    }
}