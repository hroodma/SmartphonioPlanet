using UnityEngine;

[CreateAssetMenu(fileName = "Animal Stats", menuName = "UnitStats/Animals")]
public class AnimalStats : MovementStats
{
    [Header("Bonus")]
    public float unitBunusTime = 3f;

    [Header("Distances")]
    public float detectionDistance = 5f;
    public float minDirectionDistance = 15f;
    public float maxDirectionDistance = 30f;

    [Header("Tag")]
    public AnimalTag tag;
}