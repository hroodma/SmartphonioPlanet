using UnityEngine;

[CreateAssetMenu(fileName = "Unit Stats", menuName = "UnitStats")]
public class UnitStats : ScriptableObject
{
    [Header ("Speed")]
    public float maxSpeed;
    public float moveSpeed;
    public float acceleration;
    public float turnSpeed;
}

[CreateAssetMenu(fileName = "Animal Stats", menuName = "UnitStats/Animals")]
public class AnimalStats : UnitStats
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
public class PlayerStats : UnitStats
{
    [Header("Interact")]
    public float interactionRadius = 0.7f;
}