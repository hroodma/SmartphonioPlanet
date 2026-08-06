using System.Collections.Generic;
using UnityEngine;

public sealed class PhysicsReadSystem : ISystem
{
    private readonly World world;
    private readonly Bindings bindings;

    public PhysicsReadSystem(World world, Bindings bindings)
    {
        this.world = world;
        this.bindings = bindings;
    }

    public void Run(float dt)
    {
        foreach (KeyValuePair<int, IBody> kv in bindings.Bodies)
        {
            if (!world.Units.TryGetValue(kv.Key, out UnitData u))
                continue;

            u.Position = kv.Value.Position;
            u.Forward = kv.Value.Forward;
            u.Right = kv.Value.Right;
        }
    }
}

public sealed class PlayerCommandSystem : ISystem
{
    private readonly World world;
    private readonly Bindings bindings;

    public PlayerCommandSystem(World world, Bindings bindings)
    {
        this.world = world;
        this.bindings = bindings;
    }

    public void Run(float dt)
    {
        //if (world.Match.Over || bindings.PlayerInput == null)
        //    return;
        if (!world.Units.TryGetValue(bindings.PlayerId, out UnitData player) || !player.Alive)
            return;

        PlayerCommand cmd = bindings.PlayerInput.Read();

        if (cmd.MoveAxis.sqrMagnitude >= 0.01f)
        {
            // MoveInput: только W (1) или S (-1) или 0
            if (Mathf.Abs(cmd.MoveAxis.y) > 0.1f)
                player.MoveInput = Mathf.Sign(cmd.MoveAxis.y);
            else
                player.MoveInput = 0f;

            // TurnInput: только D (1) или A (-1) или 0
            if (Mathf.Abs(cmd.MoveAxis.x) > 0.1f)
                player.TurnInput = Mathf.Sign(cmd.MoveAxis.x);
            else
                player.TurnInput = 0f;
        }
        else
        {
            player.MoveInput = 0f;
            player.TurnInput = 0f;
        }
    }
}

public sealed class PlanetGravitySystem : ISystem
{
    private readonly World world;

    public PlanetGravitySystem(World world)
    {
        this.world = world;
    }

    public void Run(float dt)
    {
        foreach (UnitData u in world.Units.Values)
        {
            if (!u.Alive) continue;

            Vector3 toCenter = (world.Planet.Center - u.Position).normalized;

            u.UpDirection = -toCenter;

            u.MoveDirection = Vector3.ProjectOnPlane(u.MoveDirection, u.UpDirection);

            u.VerticalVelocity = -u.UpDirection * world.Planet.GravityStrength * dt;

            float distanceFromCenter = Vector3.Distance(u.Position, world.Planet.Center);
            if (distanceFromCenter <= world.Planet.Radius + 0.1f)
            {
                float fallSpeed = Vector3.Dot(u.VerticalVelocity, -u.UpDirection);
                if (fallSpeed > 0)
                    u.VerticalVelocity += u.UpDirection * fallSpeed;
            }
        }
    }
}

// ВЫХОД: отдать желаемую скорость и поворот телам.
public sealed class PhysicsWriteSystem : ISystem
{
    private readonly World world;
    private readonly Bindings bindings;

    public PhysicsWriteSystem(World world, Bindings bindings)
    {
        this.world = world;
        this.bindings = bindings;
    }

    public void Run(float dt)
    {
        foreach (KeyValuePair<int, IBody> kv in bindings.Bodies)
        {
            if (!world.Units.TryGetValue(kv.Key, out UnitData u))
                continue;

            kv.Value.Apply(u.DesiredVelocity, u.UpDirection, u.Forward);
        }
    }
}