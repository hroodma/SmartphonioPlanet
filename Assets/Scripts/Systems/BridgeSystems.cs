using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
        world.Player.IsInteract = false;

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
                if (animal.IsCaughted) return;

                world.Player.CaughtAnimals++;
                world.Player.SumBonusTime += animal.UnitBonusTime;
                world.Player.Interactable = animal;
                animal.IsCaughted = true;
                break;

            case IBooster booster:
                if(booster.IsTaken) return;

                world.Player.Interactable = booster;
                booster.IsTaken = true;

                world.Player.PlayPickupSound = true;
                break;
        }

        world.Player.IsInteract = true;
        Debug.Log($"{world.Player.IsInteract}");
    }
}

public sealed class PlayerUISyncSystem : ISystem
{
    private readonly World world;
    private readonly Bindings bindings;
    private readonly IScoreRepository scoreRepository;

    public PlayerUISyncSystem(World world, Bindings bindings, IScoreRepository scoreRepository)
    {
        this.world = world;
        this.bindings = bindings;
        this.scoreRepository = scoreRepository;
    }

    public void Run(float dt)
    {
        if (world.Player == null) return;

        bindings.PlayerUI.UpdateUI(world.Player, world.Match.Timer, scoreRepository);
    }
}

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
        foreach (var kv in bindings.Views)
        {
            if (!world.Entities.TryGetValue(kv.Key, out IEntity entity))
                continue;

            float normalizedSpeed = 0f;
            if (entity is IMoveable moveable)
            {
                Vector3 horizontal = Vector3.ProjectOnPlane(
                    moveable.Movement.DesiredVelocity,
                    moveable.Movement.UpDirection
                );

                if (moveable.Movement.MaxSpeed > 0f)
                    normalizedSpeed = horizontal.magnitude / moveable.Movement.MaxSpeed;

                if (normalizedSpeed < 0.01f)
                    normalizedSpeed = 0f;
            }

            bool shouldPlayInteract = false;
            if (entity is IPlayer player && player.IsInteract)
            {
                shouldPlayInteract = true;
                player.IsInteract = false;
            }

            kv.Value.Render(normalizedSpeed, shouldPlayInteract);
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
        // 1. ✅ Обработка звука подбора (срабатывает только для игрока и только 1 раз)
        if (world.Player != null && world.Player.PlayPickupSound)
        {
            if (bindings.Sounds.TryGetValue(world.Player.Data.Id, out IUnitSound playerSound))
            {
                playerSound.PlayInteractSound(world.Player.Interactable.Data.Kind);
            }

            // Сразу сбрасываем флаг, чтобы звук не зациклился
            world.Player.PlayPickupSound = false;
        }

        // 2. ✅ Обновление громкости шагов для ВСЕХ движущихся сущностей
        foreach (KeyValuePair<int, IUnitSound> kv in bindings.Sounds)
        {
            if (!world.Entities.TryGetValue(kv.Key, out IEntity e)) continue;
            if (!(e is IMoveable moveable)) continue;

            Vector3 horizontal = Vector3.ProjectOnPlane(
                moveable.Movement.DesiredVelocity,
                moveable.Movement.UpDirection
            );

            float speed = moveable.Movement.CurrentSpeed > 0f
                ? horizontal.magnitude / moveable.Movement.CurrentSpeed
                : 0f;

            if (speed < 0.01f) speed = 0f;

            kv.Value.UpdateFootstepVolume(speed);
        }
    }
}

public sealed class BoosterSystem : ISystem
{
    private readonly World world;
    private readonly IUnitSink sink;
    private float speedBoostTimer;

    public BoosterSystem(World world, IUnitSink sink)
    {
        this.world = world;
        this.sink = sink;
    }

    public void Run(float dt)
    {
        foreach (IEntity e in world.Entities.Values)
        {
            if (!(e is IBooster booster)) continue;

            if (!booster.IsTaken) continue;

            switch (booster)
            {
                case ISpeedBooster speedBooster:
                    world.Player.Movement.CurrentSpeed = Mathf.Min(speedBooster.Value, world.Player.Movement.MaxSpeed);
                    speedBoostTimer += speedBooster.Duration;
                    break;
            }

            booster.IsTaken = false;

            sink.Respawn(booster.Data.Id);
        }

        speedBoostTimer = Mathf.Max(0, speedBoostTimer - dt);

        if (speedBoostTimer <= 0) world.Player.Movement.CurrentSpeed = world.Player.Movement.DefaultSpeed;
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
    private readonly IScoreRepository scoreRepository;
    private bool shown;

    public MatchEndView(World world, Bindings bindings, GameResultUI ui, IScoreRepository scoreRepository)
    {
        this.world = world;
        this.bindings = bindings;
        this.ui = ui;
        this.scoreRepository = scoreRepository;
    }

    public void Run(float dt)
    {
        if (shown || !world.Match.Over || ui == null)
            return;

        shown = true;

        if (world.Player == null) return;

        int currentScore = world.Player.CaughtAnimals;

        scoreRepository.SaveHighScore(currentScore);

        int highScore = scoreRepository.GetHighScore();

        ui.ShowResult(currentScore, highScore);
    }
}