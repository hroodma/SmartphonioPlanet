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
            float turnAngle = world.Player.Data.TurnSpeed * world.Player.TurnInput * dt;
            Quaternion turnRotation = Quaternion.AngleAxis(turnAngle, world.Player.Data.UpDirection);
            world.Player.Data.Forward = turnRotation * world.Player.Data.Forward;
        }

        world.Player.Data.Forward = Vector3.ProjectOnPlane(world.Player.Data.Forward, world.Player.Data.UpDirection);
        if (world.Player.Data.Forward.sqrMagnitude < 0.001f)
        {
            world.Player.Data.Forward = Vector3.ProjectOnPlane(world.Player.Data.Right, world.Player.Data.UpDirection);
        }
        world.Player.Data.Forward.Normalize();

        world.Player.Data.Right = Vector3.Cross(world.Player.Data.UpDirection, world.Player.Data.Forward).normalized;

        Vector3 targetVelocity;
        if (Mathf.Abs(world.Player.MoveInput) < 0.01f)
        {
            targetVelocity = Vector3.zero;
        }
        else
        {
            targetVelocity = world.Player.Data.Forward * world.Player.MoveInput * world.Player.Data.MoveSpeed;
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, world.Player.Data.UpDirection);
        }

        float accel = Mathf.Abs(world.Player.MoveInput) < 0.01f
            ? world.Player.Data.Acceleration * 2f
            : world.Player.Data.Acceleration;

        world.Player.Data.HorizontalVelocity = Vector3.MoveTowards(
            world.Player.Data.HorizontalVelocity,
            targetVelocity,
            accel * dt
        );

        world.Player.Data.DesiredVelocity = world.Player.Data.HorizontalVelocity + world.Player.Data.VerticalVelocity;
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

        playerPosition = world.Player.Data.Position;

        foreach (IAnimal animal in world.Animals.Values)
        {
            if (!animal.Data.Alive) continue;

            Vector3 toPlayer = playerPosition - animal.Data.Position;

            if (Vector3.Distance(animal.Data.Position, playerPosition) < animal.DetectionDistance)
                RunningLogic(toPlayer, animal, dt);
            else
                WanderingLogic(animal, dt);

            animal.Data.DesiredVelocity = animal.Data.HorizontalVelocity + animal.Data.VerticalVelocity;
        }        
    }

    public void WanderingLogic(IAnimal animal, float dt)
    {
        if (!animal.IsTurning)
        {
            float distanceMoved = animal.Data.HorizontalVelocity.magnitude * dt;
            animal.CurrentWalkDistance += distanceMoved;

            if (animal.CurrentWalkDistance >= animal.TargetWalkDistance)
            {
                animal.IsTurning = true;
                animal.CurrentWalkDistance = 0f;
                animal.Data.HorizontalVelocity = Vector3.zero;

                float randomAngle = Random.Range(-130f, 130f);

                Quaternion turnRot = Quaternion.AngleAxis(randomAngle, animal.Data.UpDirection);
                animal.TargetForward = Vector3.ProjectOnPlane(turnRot * animal.Data.Forward, animal.Data.UpDirection).normalized;
            }
        }

        if (animal.IsTurning)
        {
            Quaternion currentRot = Quaternion.LookRotation(animal.Data.Forward, animal.Data.UpDirection);
            Quaternion targetRot = Quaternion.LookRotation(animal.TargetForward, animal.Data.UpDirection);

            Quaternion newRot = Quaternion.RotateTowards(currentRot, targetRot, animal.Data.TurnSpeed * dt);
            animal.Data.Forward = newRot * Vector3.forward;

            if (Vector3.Dot(animal.Data.Forward, animal.TargetForward) > 0.999f)
            {
                animal.Data.Forward = animal.TargetForward;
                animal.IsTurning = false;

                animal.TargetWalkDistance = Random.Range(animal.MinDirectionDistance, animal.MaxDirectionDistance);
            }
        }
        else
        {
            Vector3 targetVelocity = animal.Data.Forward * animal.Data.MoveSpeed;
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, animal.Data.UpDirection);

            animal.Data.HorizontalVelocity = Vector3.MoveTowards(
                animal.Data.HorizontalVelocity,
                targetVelocity,
                animal.Data.Acceleration * dt
            );
        }
    }
    
    public void RunningLogic(Vector3 toPlayer, IAnimal animal, float dt)
    {
        Vector3 fleeDirection = -toPlayer.normalized;

        fleeDirection = Vector3.ProjectOnPlane(fleeDirection, animal.Data.UpDirection).normalized;

        if (fleeDirection.sqrMagnitude < 0.001f)
        {
            fleeDirection = Vector3.Cross(animal.Data.UpDirection, Vector3.right).normalized;
        }

        Quaternion currentRot = Quaternion.LookRotation(animal.Data.Forward, animal.Data.UpDirection);
        Quaternion fleeRot = Quaternion.LookRotation(fleeDirection, animal.Data.UpDirection);

        Quaternion newRot = Quaternion.RotateTowards(currentRot, fleeRot, animal.Data.TurnSpeed * 3f * dt);
        animal.Data.Forward = newRot * Vector3.forward;

        Vector3 targetVelocity = animal.Data.Forward * animal.Data.MaxSpeed;
        targetVelocity = Vector3.ProjectOnPlane(targetVelocity, animal.Data.UpDirection);

        animal.Data.HorizontalVelocity = Vector3.MoveTowards(
            animal.Data.HorizontalVelocity,
            targetVelocity,
            animal.Data.Acceleration * dt
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
            e.Data.DesiredVelocity = Vector3.zero;
    }
}