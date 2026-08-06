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

            u.HorizontalVelocity = u.MoveDirection.normalized * u.MoveSpeed;
            if (u.MoveDirection.sqrMagnitude < 0.01f)
                u.HorizontalVelocity = Vector3.zero;

            u.DesiredVelocity = u.HorizontalVelocity + u.VerticalVelocity;            
        }
    }
}