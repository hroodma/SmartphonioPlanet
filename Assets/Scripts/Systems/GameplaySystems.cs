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

            u.HorizontalVelocity = u.Forward * u.MoveInput * u.MoveSpeed;

            if (Mathf.Abs(u.MoveInput) < 0.01f)
                u.HorizontalVelocity = Vector3.zero;

            u.HorizontalVelocity = Vector3.ProjectOnPlane(u.HorizontalVelocity, u.UpDirection);
            u.DesiredVelocity = u.HorizontalVelocity + u.VerticalVelocity;
        }
    }
}