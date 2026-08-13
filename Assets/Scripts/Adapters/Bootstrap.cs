using UnityEngine;
using UnityEngine.InputSystem;

public sealed class Bootstrap : MonoBehaviour
{
    public const float GRAVITY_STRENGTH = 200f;

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

    [Header("Planet")]
    [SerializeField] private PlanetValues planet;

    [Header("Stats (ScriptableObject)")]
    [SerializeField] private PlayerStats playerStats;

    private void Awake()
    {
        World world = new World();
        Bindings bindings = new Bindings();

        world.Match.Timer = startTimer;
        world.Match.Over = false;

        world.Planet.Center = planet.center;
        world.Planet.Radius = planet.radius;
        world.Planet.GravityStrength = GRAVITY_STRENGTH;

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

        Debug.Log($"{playerStats}");
        Debug.Log($"{playerStats.maxSpeed}");
        Debug.Log($"{playerBody}");

        // Усыновляем то, что уже расставлено в сцене.
        if (playerBody != null)
            bindings.PlayerId = factory.RegisterBody(playerBody, playerStats, UnitKind.Player);

        factory.SpawnWave();

        PhysicsWriteSystem write = new PhysicsWriteSystem(world, bindings);
        IScoreRepository scoreRepository = new PlayerPrefsScoreRepository();

        ISystem[] fixedSystems =
        {
            new PhysicsReadSystem(world, bindings),
            new PlanetGravitySystem(world),
            new InteractionSystem(world),
            new BoosterSystem(world, factory),
            new MovementSystem(world),
            new AnimalMovementSystem(world),
            new CaughtSystem(world, factory),
            write
        };

        ISystem[] frameSystems =
        {
            new PlayerCommandSystem(world, bindings),
            new EndGameTimerSystem(world),
            new PlayerUISyncSystem(world, bindings, scoreRepository),
            new ViewSyncSystem(world, bindings),
            new SoundSyncSystem(world, bindings),
            new MatchEndView(world, bindings, gameResultUI, scoreRepository)
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