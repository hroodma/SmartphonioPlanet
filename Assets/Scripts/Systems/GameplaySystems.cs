using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

            u.DesiredVelocity = u.HorizontalVelocity + u.VerticalVelocity;
        }
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
            if (!u.Alive && !caught.Contains(u.Id))
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