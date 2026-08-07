using System;
using UnityEditor;
using UnityEngine;

public sealed class Bootstrap : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private GameLoop loop;
    [SerializeField] private PlayerInputAdapter playerInput;

    [Header("Player")]
    [SerializeField] private UnitBody playerBody;

    private void Awake()
    {
        World world = new World();
        Bindings bindings = new Bindings();

        bindings.PlayerInput = playerInput;

        // TECT TECT TECT TECT TECT TECT TECT TECT TECT TECT
        world.Planet.Center = Vector3.zero;
        world.Planet.Radius = 5f;
        world.Planet.GravityStrength = 200f;

        UnitData testUnitData = TestUnitData();
        bindings.Bodies[testUnitData.Id] = playerBody;
        bindings.PlayerId = testUnitData.Id;
        world.Units[testUnitData.Id] = testUnitData;

        PhysicsWriteSystem write = new PhysicsWriteSystem(world, bindings);

        ISystem[] fixedSystems =
        {
            new PhysicsReadSystem(world, bindings),
            new PlanetGravitySystem(world),
            new MovementSystem(world),
            write
        };

        ISystem[] frameSystems =
        {
            new PlayerCommandSystem(world, bindings),
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
        unitData.Acceleration = 20f;
        unitData.TurnSpeed = 60f;
        unitData.UpDirection = Vector3.up;

        return unitData;
    }
}