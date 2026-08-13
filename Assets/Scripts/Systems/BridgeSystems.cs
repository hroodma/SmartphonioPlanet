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
            if (!world.Entities.TryGetValue(kv.Key, out IEntity e))
                continue;

            if (!(e is IMoveable moveable)) continue;

            moveable.Movement.Position = kv.Value.Position;
            moveable.Movement.Forward = kv.Value.Forward;
            moveable.Movement.Right = kv.Value.Right;
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
        if (world.Match.Over || bindings.PlayerInput == null)
            return;
        if (world.Player == null || !world.Player.Data.Alive)
            return;

        PlayerCommand cmd = bindings.PlayerInput.Read();

        if (cmd.MoveAxis.sqrMagnitude >= 0.01f)
        {
            // MoveInput: только W (1) или S (-1) или 0
            if (Mathf.Abs(cmd.MoveAxis.y) > 0.1f)
                world.Player.MoveInput = Mathf.Sign(cmd.MoveAxis.y);
            else
                world.Player.MoveInput = 0f;

            // TurnInput: только D (1) или A (-1) или 0
            if (Mathf.Abs(cmd.MoveAxis.x) > 0.1f)
                world.Player.TurnInput = Mathf.Sign(cmd.MoveAxis.x);
            else
                world.Player.TurnInput = 0f;

            if (world.Player.MoveInput < 0)
                world.Player.TurnInput *= -1;
        }
        else
        {
            world.Player.MoveInput = 0f;
            world.Player.TurnInput = 0f;
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
        foreach (IEntity e in world.Entities.Values)
        {
            if (!e.Data.Alive) continue;

            if (!(e is IMoveable moveable)) continue;

            Vector3 toCenter = (world.Planet.Center - moveable.Movement.Position).normalized;

            moveable.Movement.UpDirection = -toCenter;

            moveable.Movement.MoveDirection = Vector3.ProjectOnPlane(moveable.Movement.MoveDirection, moveable.Movement.UpDirection);

            moveable.Movement.VerticalVelocity = -moveable.Movement.UpDirection * world.Planet.GravityStrength * dt;
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
        if (world.Player == null || !world.Player.Data.Alive) return;

        int count = Physics.OverlapSphereNonAlloc(
                world.Player.Movement.Position,
                world.Player.InteractionRadius,
                _buffer,
                LayerMask.GetMask("Interactable")
            );

        for (int i = 0; i < count; i++)
        {
            if (_buffer[i].TryGetComponent<UnitRef>(out var otherRef))
            {
                if (otherRef.Id == world.Player.Data.Id) continue;

                if (world.Entities.TryGetValue(otherRef.Id, out IEntity other) && other is IInteractable interactable)
                {
                    HandleInteraction(interactable);
                }
            }
        }
    }

    private void HandleInteraction(IInteractable interactable)
    {
        if (!interactable.Data.Alive) return;

        switch (interactable)
        {
            case IAnimal animal:
                world.Player.CaughtAnimals++;
                world.Player.SumBonusTime += animal.UnitBonusTime;
                animal.IsCaughted = true;
                break;
        }
    }
}

public sealed class PlayerUISyncSystem : ISystem
{
    private readonly World world;
    private readonly Bindings bindings;

    public PlayerUISyncSystem(World world, Bindings bindings)
    {
        this.world = world;
        this.bindings = bindings;
    }

    public void Run(float dt)
    {
        if (world.Player == null) return;

        bindings.PlayerUI.UpdateUI(world.Player, world.Match.Timer);
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
            if (world.Entities.TryGetValue(kv.Key, out IEntity e))
            {
                kv.Value.Render(e);
            }
        }
    }
}

public sealed class SoundSyncSystem : ISystem
{
    private readonly World world;
    private readonly Bindings bindings;

    public SoundSyncSystem(World world, Bindings bindings)
    {
        this.world = world;
        this.bindings = bindings;
    }

    public void Run(float dt)
    {
        foreach (KeyValuePair<int, IUnitSound> kv in bindings.Sounds)
        {
            if (!world.Entities.TryGetValue(kv.Key, out IEntity e)) continue;

            if (!(e is IMoveable moveable)) continue;

            Vector3 horizontal = Vector3.ProjectOnPlane(moveable.Movement.DesiredVelocity, moveable.Movement.UpDirection);
            float speed = horizontal.magnitude / moveable.Movement.MoveSpeed;

            if (speed < 0.01f) speed = 0f;

            kv.Value.UpdateFootstepVolume(speed);
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
            if (!world.Entities.TryGetValue(kv.Key, out IEntity e)) continue;

            if (!(e is IMoveable moveable)) continue;

            kv.Value.Apply(moveable.Movement.DesiredVelocity, moveable.Movement.UpDirection, moveable.Movement.Forward);
        }
    }
}

public sealed class MatchEndView : ISystem
{
    private readonly World world;
    private readonly Bindings bindings;
    private readonly GameResultUI ui;
    private bool shown;

    public MatchEndView(World world, Bindings bindings, GameResultUI ui)
    {
        this.world = world;
        this.bindings = bindings;
        this.ui = ui;
    }

    public void Run(float dt)
    {
        if (shown || !world.Match.Over || ui == null)
            return;

        shown = true;

        if (world.Player == null) return;

        ui.ShowResult(world.Player.CaughtAnimals);
    }
}