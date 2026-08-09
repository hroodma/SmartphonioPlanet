using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public enum UnitKind { Player, Animal }

public sealed class UnitData
{
    public int Id;
    public UnitKind Kind;

    public bool Alive = true;

    public Vector3 Position;
    public Vector3 HorizontalVelocity;
    public Vector3 VerticalVelocity;
    public Vector3 DesiredVelocity;

    public Vector3 Forward;
    public Vector3 Right;

    public float MoveSpeed;
    public float Acceleration;
    public Vector3 MoveDirection;
    public Vector3 UpDirection;

    public float MoveInput;
    public float TurnInput;
    public float TurnSpeed = 60f;
    public float InteractionRadius;

    public int CollectedAnimals;
}

public sealed class PlanetData
{
    public Vector3 Center;
    public float GravityStrength;
    public float Radius;
}

public sealed class MatchState
{
    public bool Over;
    public float timer;
}

public sealed class World
{
    public readonly Dictionary<int, UnitData> Units = new Dictionary<int, UnitData>();
    public readonly PlanetData Planet = new PlanetData();
    public readonly MatchState Match = new MatchState();

    private int nextId = 1;

    public UnitData Add(UnitData unit)
    {
        unit.Id = nextId++;
        Units[unit.Id] = unit;
        return unit;
    }

    public void Remove(int id)
    {
        Units.Remove(id);
    }
}

public sealed class Bindings
{
    public readonly Dictionary<int, IBody> Bodies = new Dictionary<int, IBody>();
    public readonly Dictionary<int, IUnitView> Views = new Dictionary<int, IUnitView>();
    public readonly Dictionary<int, IUnitSound> Sounds = new Dictionary<int, IUnitSound>();
    public IPlayerInput PlayerInput;
    public int PlayerId;
}

public struct PlayerCommand
{
    public bool Move;
    public Vector2 MoveAxis;
} 