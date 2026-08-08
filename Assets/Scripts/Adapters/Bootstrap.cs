using System;
using UnityEditor;
using UnityEngine;

public sealed class Bootstrap : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private GameLoop loop;
    [SerializeField] private UnitFactory factory;
    [SerializeField] private PlayerInputAdapter playerInput;

    [Header("Placed units")]
    [SerializeField] private UnitBody playerBody;

    [Header("Stats (ScriptableObject)")]
    [SerializeField] private UnitStats playerStats;

    private void Awake()
    {
        World world = new World();
        Bindings bindings = new Bindings();

        factory.Init(world, bindings);
        bindings.PlayerInput = playerInput;

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
            new CaughtSystem(world, factory),
            write
        };

        ISystem[] frameSystems =
        {
            new PlayerCommandSystem(world, bindings),
            new ViewSyncSystem(world, bindings)
        };

        ISystem[] endedSystems =
        {

        };

        GameManager game = new GameManager(world, fixedSystems, frameSystems, endedSystems);
        loop.Bind(game);
    }

    private UnitData TestUnitData()
    {
        UnitData unitData = new UnitData();
        unitData.Id = 1;
        unitData.MoveSpeed = 5f;
        unitData.TurnSpeed = 60f;
        unitData.UpDirection = Vector3.up;

        return unitData;
    }
}