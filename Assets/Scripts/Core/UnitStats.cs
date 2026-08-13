using UnityEngine;

[CreateAssetMenu(fileName = "Unit Stats", menuName = "UnitStats")]
public class UnitStats : ScriptableObject
{
    
}

[CreateAssetMenu(fileName = "Booster Stats", menuName = "UnitStats/Boosters")]
public class SpeedBoostStats : UnitStats
{
    public float speedBoostValue;
    public float duration;
}

[CreateAssetMenu(fileName = "Movement Stats", menuName = "UnitStats")]
public class MovementStats : UnitStats
{
    [Header("Speed")]
    public float maxSpeed;
    public float moveSpeed;
    public float acceleration;
    public float turnSpeed;
}

[CreateAssetMenu(fileName = "Animal Stats", menuName = "UnitStats/Animals")]
public class AnimalStats : MovementStats
{
    [Header ("Bonus")]
    public float unitBunusTime = 3f;

    [Header ("Distances")]
    public float detectionDistance = 5f;
    public float minDirectionDistance = 15f;
    public float maxDirectionDistance = 30f;

    [Header ("Tag")]
    public AnimalTag tag;
}

[CreateAssetMenu(fileName = "Player Stats", menuName = "UnitStats/Player")]
public class PlayerStats : MovementStats
{
    [Header("Interact")]
    public float interactionRadius = 0.7f;
}