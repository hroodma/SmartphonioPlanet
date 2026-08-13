using UnityEngine;

[CreateAssetMenu(fileName = "Movement Stats", menuName = "UnitStats/Movement")]
public class MovementStats : UnitStats
{
    [Header("Speed")]
    public float maxSpeed;
    public float defaultSpeed;
    public float acceleration;
    public float turnSpeed;
}