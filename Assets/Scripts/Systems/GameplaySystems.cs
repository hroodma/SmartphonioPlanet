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
        foreach (UnitData u in world.Units.Values)
        {
            if (!u.Alive) continue;
            
            if (u.Kind == UnitKind.Player)
            {
                if (Mathf.Abs(u.TurnInput) > 0.01f)
                {
                    float turnAngle = u.TurnSpeed * u.TurnInput * dt;
                    Quaternion turnRotation = Quaternion.AngleAxis(turnAngle, u.UpDirection);
                    u.Forward = turnRotation * u.Forward;
                }

                u.Forward = Vector3.ProjectOnPlane(u.Forward, u.UpDirection);
                if (u.Forward.sqrMagnitude < 0.001f)
                {
                    u.Forward = Vector3.ProjectOnPlane(u.Right, u.UpDirection);
                }
                u.Forward.Normalize();

                u.Right = Vector3.Cross(u.UpDirection, u.Forward).normalized;

                Vector3 targetVelocity;
                if (Mathf.Abs(u.MoveInput) < 0.01f)
                {
                    targetVelocity = Vector3.zero;
                }
                else
                {
                    targetVelocity = u.Forward * u.MoveInput * u.MoveSpeed;
                    targetVelocity = Vector3.ProjectOnPlane(targetVelocity, u.UpDirection);
                }

                float accel = Mathf.Abs(u.MoveInput) < 0.01f
                    ? u.Acceleration * 2f
                    : u.Acceleration;

                u.HorizontalVelocity = Vector3.MoveTowards(
                    u.HorizontalVelocity,
                    targetVelocity,
                    accel * dt
                );
            }

            u.DesiredVelocity = u.HorizontalVelocity + u.VerticalVelocity;
        }
    }
}

public sealed class AnimalMovementSystem : ISystem
{
    private readonly World world;
    private readonly Bindings bindings;

    public AnimalMovementSystem(World world, Bindings bindings)
    {
        this.world = world;
        this.bindings = bindings;
    }

    public void Run(float dt)
    {
        Vector3 playerPosition = new Vector3();

        if (world.Units.TryGetValue(bindings.PlayerId, out UnitData playerData))
            playerPosition = playerData.Position;

        foreach (UnitData u in world.Units.Values)
        {
            if (!u.Alive) continue;

            if (u.Kind == UnitKind.Animal)
            {
                Vector3 toPlayer = playerPosition - u.Position;

                if (Vector3.Distance(u.Position, playerPosition) < u.DetecionDistance)
                    RunningLogic(toPlayer, u, dt);
                else
                    WanderingLogic(u, dt);
            }
        }        
    }

    public void WanderingLogic(UnitData u, float dt)
    {
        if (!u.IsTurning)
        {
            float distanceMoved = u.HorizontalVelocity.magnitude * dt;
            u.CurrentWalkDistance += distanceMoved;

            if (u.CurrentWalkDistance >= u.TargetWalkDistance)
            {
                u.IsTurning = true;
                u.CurrentWalkDistance = 0f;
                u.HorizontalVelocity = Vector3.zero;

                float randomAngle = Random.Range(-130f, 130f);

                Quaternion turnRot = Quaternion.AngleAxis(randomAngle, u.UpDirection);
                u.TargetForward = Vector3.ProjectOnPlane(turnRot * u.Forward, u.UpDirection).normalized;
            }
        }

        if (u.IsTurning)
        {
            Quaternion currentRot = Quaternion.LookRotation(u.Forward, u.UpDirection);
            Quaternion targetRot = Quaternion.LookRotation(u.TargetForward, u.UpDirection);

            Quaternion newRot = Quaternion.RotateTowards(currentRot, targetRot, u.TurnSpeed * dt);
            u.Forward = newRot * Vector3.forward;

            if (Vector3.Dot(u.Forward, u.TargetForward) > 0.999f)
            {
                u.Forward = u.TargetForward;
                u.IsTurning = false;

                u.TargetWalkDistance = Random.Range(u.MinDirectionDistance, u.MaxDirectionDistance);
            }
        }
        else
        {
            Vector3 targetVelocity = u.Forward * u.MoveSpeed;
            targetVelocity = Vector3.ProjectOnPlane(targetVelocity, u.UpDirection);

            u.HorizontalVelocity = Vector3.MoveTowards(
                u.HorizontalVelocity,
                targetVelocity,
                u.Acceleration * dt
            );
        }
    }
    
    public void RunningLogic(Vector3 toPlayer, UnitData u, float dt)
    {
        Vector3 fleeDirection = -toPlayer.normalized;

        fleeDirection = Vector3.ProjectOnPlane(fleeDirection, u.UpDirection).normalized;

        if (fleeDirection.sqrMagnitude < 0.001f)
        {
            fleeDirection = Vector3.Cross(u.UpDirection, Vector3.right).normalized;
        }

        Quaternion currentRot = Quaternion.LookRotation(u.Forward, u.UpDirection);
        Quaternion fleeRot = Quaternion.LookRotation(fleeDirection, u.UpDirection);

        Quaternion newRot = Quaternion.RotateTowards(currentRot, fleeRot, u.TurnSpeed * 3f * dt);
        u.Forward = newRot * Vector3.forward;

        Vector3 targetVelocity = u.Forward * u.FleeSpeed;
        targetVelocity = Vector3.ProjectOnPlane(targetVelocity, u.UpDirection);

        u.HorizontalVelocity = Vector3.MoveTowards(
            u.HorizontalVelocity,
            targetVelocity,
            u.Acceleration * dt
        );
    }
}

public sealed class CaughtSystem : ISystem
{
    private readonly World world;
    private readonly IUnitSink sink;
    private readonly List<int> caught = new List<int>();

    public CaughtSystem(World world, IUnitSink sink)
    {
        this.world = world;
        this.sink = sink;
    }

    public void Run(float dt)
    {
        caught.Clear();
        foreach (UnitData u in world.Units.Values)
            if (!u.Alive)
                caught.Add(u.Id);

        foreach (int id in caught)
        {
            if (!world.Units.TryGetValue(id, out UnitData u))
                continue;

            u.Alive = false;
            u.DesiredVelocity = Vector3.zero;

            switch (u.Kind)
            {
                case UnitKind.Animal:
                    sink.Respawn(id);
                    break;
            }
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

        foreach (UnitData u in world.Units.Values)
        {            
            if (u.Kind == UnitKind.Player)
            {
                world.Match.Timer += u.SumBonusTime;
                u.SumBonusTime = 0;
            }
        }

        world.Match.Timer = Mathf.Max(0, world.Match.Timer - dt);
        Debug.Log($"Осталось: {world.Match.Timer}");
    }
}