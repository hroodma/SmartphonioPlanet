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
        }
    }
}

public sealed class InteractionSystem : ISystem
{
    private readonly World world;

    private readonly Collider[] _buffer = new Collider[32];

    public InteractionSystem(World world)
    {
        this.world = world;
    }

    public void Run(float dt)
    {
        foreach (UnitData u in world.Units.Values)
        {
            if (!u.Alive) continue;

            if (u.Kind == UnitKind.Animal) continue;

            int count = Physics.OverlapSphereNonAlloc(
                u.Position,
                u.InteractionRadius,
                _buffer,
                LayerMask.GetMask("Interactable")
            );

            for (int i = 0; i < count; i++)
            {
                if (_buffer[i].TryGetComponent<UnitRef>(out var otherRef))
                {
                    if (otherRef.Id == u.Id) continue;

                    if (world.Units.TryGetValue(otherRef.Id, out UnitData other))
                    {
                        HandleInteraction(u, other);
                    }
                }
            }
        }
    }

    private void HandleInteraction(UnitData self, UnitData other)
    {
        if (other.Kind == UnitKind.Animal && other.Alive)
        {
            self.CollectedAnimals++;
            other.Alive = false;
            Debug.Log($"пойманных зверей: {self.CollectedAnimals}");
        }
    }
}

// ВЫХОД: погнать данные в Animator каждого юнита.
public sealed class ViewSyncSystem : ISystem
{
    private readonly World world;
    private readonly Bindings bindings;

    public ViewSyncSystem(World world, Bindings bindings)
    {
        this.world = world;
        this.bindings = bindings;
    }

    public void Run(float dt)
    {
        foreach (KeyValuePair<int, IUnitView> kv in bindings.Views)
        {
            if (world.Units.TryGetValue(kv.Key, out UnitData u))
            {
                kv.Value.Render(u);
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