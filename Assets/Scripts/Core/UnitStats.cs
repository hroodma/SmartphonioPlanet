using UnityEngine;

[CreateAssetMenu(fileName = "Unit Stats", menuName = "UnitStats")]
public class UnitStats : ScriptableObject
{
    public float moveSpeed = 5f;
    public float acceleration = 20f;
    public float turnSpeed = 60f;
    public float interactionRadius = 0.7f;
}

[CreateAssetMenu(fileName = "Animal Stats", menuName = "UnitStats/Animals")]
public class AnimalStats : UnitStats
{
    public AnimalTag tag;

    public float fleeSpeed = 5f;

    public float unitBunusTime = 3f;

    public float detectionDistance = 5f;
    public float minDirectionDistance = 15f;
    public float maxDirectionDistance = 30f;
}