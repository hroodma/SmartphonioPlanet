using System.Collections.Generic;
using UnityEngine;

public sealed class MovementSystem : ISystem
{
    private readonly World world;

    public MovementSystem(World world)
    {
        this.world = world;
    }

    public void Run(float dt)
    {
        if (world.Player == null || !world.Player.Data.Alive) return;

        if (Mathf.Abs(world.Player.TurnInput) > 0.01f)
        {
            float turnAngle = world.Player.Movement.TurnSpeed * world.Player.TurnInput * dt;
            Quaternion turnRotation = Quaternion.AngleAxis(turnAngle, world.Player.Movement.UpDirection);
            world.Player.Movement.Forward = turnRotation * world.Player.Movement.Forward;
        }

        world.Player.Movement.Forward = Vector3.ProjectOnPlane(world.Player.Movement.Forward, world.Player.Movement.UpDirection);
        if (world.Player.Movement.Forward.sqrMagnitude < 0.001f)
        {
            world.Player.Movement.Forward = Vector3.ProjectOnPlane(world.Player.Movement.Right, world.Player.Movement.UpDirection);
        }
        world.Player.Movement.Forward.Normalize();

        world.Player.Movement.Right = Vector3.Cross(world.Player.Movement.UpDirection, world.Player.Movement.Forward).normalized;

        Vector3 targetVelocity;
        if (Mathf.Abs(world.Player.MoveInput) < 0.01f)
        {
            targetVelocity = Vector3.zero;
        }
        else
        {
            targetVelocity = world.Player.Movement.Forward * world.Player.MoveInput * world.Player.Movement.MoveSpeed;
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, world.Player.Movement.UpDirection);
        }

        float accel = Mathf.Abs(world.Player.MoveInput) < 0.01f
            ? world.Player.Movement.Acceleration * 2f
            : world.Player.Movement.Acceleration;

        world.Player.Movement.HorizontalVelocity = Vector3.MoveTowards(
            world.Player.Movement.HorizontalVelocity,
            targetVelocity,
            accel * dt
        );

        world.Player.Movement.DesiredVelocity = world.Player.Movement.HorizontalVelocity + world.Player.Movement.VerticalVelocity;
    }
}

public sealed class AnimalMovementSystem : ISystem
{
    private readonly World world;

    public AnimalMovementSystem(World world)
    {
        this.world = world;
    }

    public void Run(float dt)
    {
        if (world.Player == null) return;

        Vector3 playerPosition = new();

        playerPosition = world.Player.Movement.Position;

        foreach (IAnimal animal in world.Animals.Values)
        {
            if (!animal.Data.Alive || animal.IsCaughted) continue;

            Vector3 toPlayer = playerPosition - animal.Movement.Position;

            if (Vector3.Distance(animal.Movement.Position, playerPosition) < animal.DetectionDistance)
                RunningLogic(toPlayer, animal, dt);
            else
                WanderingLogic(animal, dt);

            animal.Movement.DesiredVelocity = animal.Movement.HorizontalVelocity + animal.Movement.VerticalVelocity;
        }        
    }

    public void WanderingLogic(IAnimal animal, float dt)
    {
        if (!animal.IsTurning)
        {
            float distanceMoved = animal.Movement.HorizontalVelocity.magnitude * dt;
            animal.CurrentWalkDistance += distanceMoved;

            if (animal.CurrentWalkDistance >= animal.TargetWalkDistance)
            {
                animal.IsTurning = true;
                animal.CurrentWalkDistance = 0f;
                animal.Movement.HorizontalVelocity = Vector3.zero;

                float randomAngle = Random.Range(-130f, 130f);

                Quaternion turnRot = Quaternion.AngleAxis(randomAngle, animal.Movement.UpDirection);
                animal.TargetForward = Vector3.ProjectOnPlane(turnRot * animal.Movement.Forward, animal.Movement.UpDirection).normalized;
            }
        }

        if (animal.IsTurning)
        {
            Quaternion currentRot = Quaternion.LookRotation(animal.Movement.Forward, animal.Movement.UpDirection);
            Quaternion targetRot = Quaternion.LookRotation(animal.TargetForward, animal.Movement.UpDirection);

            Quaternion newRot = Quaternion.RotateTowards(currentRot, targetRot, animal.Movement.TurnSpeed * dt);
            animal.Movement.Forward = newRot * Vector3.forward;

            if (Vector3.Dot(animal.Movement.Forward, animal.TargetForward) > 0.999f)
            {
                animal.Movement.Forward = animal.TargetForward;
                animal.IsTurning = false;

                animal.TargetWalkDistance = Random.Range(animal.MinDirectionDistance, animal.MaxDirectionDistance);
            }
        }
        else
        {
            Vector3 targetVelocity = animal.Movement.Forward * animal.Movement.MoveSpeed;
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, animal.Movement.UpDirection);

            animal.Movement.HorizontalVelocity = Vector3.MoveTowards(
                animal.Movement.HorizontalVelocity,
                targetVelocity,
                animal.Movement.Acceleration * dt
            );
        }
    }
    
    public void RunningLogic(Vector3 toPlayer, IAnimal animal, float dt)
    {
        Vector3 fleeDirection = -toPlayer.normalized;

        fleeDirection = Vector3.ProjectOnPlane(fleeDirection, animal.Movement.UpDirection).normalized;

        if (fleeDirection.sqrMagnitude < 0.001f)
        {
            fleeDirection = Vector3.Cross(animal.Movement.UpDirection, Vector3.right).normalized;
        }

        Quaternion currentRot = Quaternion.LookRotation(animal.Movement.Forward, animal.Movement.UpDirection);
        Quaternion fleeRot = Quaternion.LookRotation(fleeDirection, animal.Movement.UpDirection);

        Quaternion newRot = Quaternion.RotateTowards(currentRot, fleeRot, animal.Movement.TurnSpeed * 3f * dt);
        animal.Movement.Forward = newRot * Vector3.forward;

        Vector3 targetVelocity = animal.Movement.Forward * animal.Movement.MaxSpeed;
        targetVelocity = Vector3.ProjectOnPlane(targetVelocity, animal.Movement.UpDirection);

        animal.Movement.HorizontalVelocity = Vector3.MoveTowards(
            animal.Movement.HorizontalVelocity,
            targetVelocity,
            animal.Movement.Acceleration * dt
        );
    }
}

public sealed class CaughtSystem : ISystem
{
    private readonly World world;
    private readonly IUnitSink sink;
    private readonly List<int> caughtIds = new List<int>();

    public CaughtSystem(World world, IUnitSink sink)
    {
        this.world = world;
        this.sink = sink;
    }

    public void Run(float dt)
    {
        caughtIds.Clear();

        foreach (IEntity e in world.Entities.Values)
        {
            if (e is ICaughtable caughtable && caughtable.IsCaughted)
            {
                caughtIds.Add(e.Data.Id);
            }
        }

        foreach (int id in caughtIds)
        {
            sink.Uncaught(id);
        }
    }
}

public sealed class EndGameTimerSystem : ISystem
{
    private readonly World world;

    public EndGameTimerSystem(World world)
    {
        this.world = world;
    }

    public void Run(float dt)
    {
        if (world.Match.Timer <= 0)
        {
            world.Match.Over = true;
            return;
        }

        if (world.Player == null) return;

        world.Match.Timer += world.Player.SumBonusTime;
        world.Player.SumBonusTime = 0;

        world.Match.Timer = Mathf.Max(0, world.Match.Timer - dt);
    }
}

public sealed class FreezeSystem : ISystem
{
    private readonly World world;

    public FreezeSystem(World world)
    {
        this.world = world;
    }

    public void Run(float dt)
    {
        if (!world.Match.Over) return;

        foreach (IEntity e in world.Entities.Values)
        {
            if (!(e is IMoveable moveable)) continue;

            moveable.Movement.DesiredVelocity = Vector3.zero;
        }
    }
}