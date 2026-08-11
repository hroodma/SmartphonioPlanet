using UnityEngine;

[CreateAssetMenu(fileName = "Unit Stats", menuName = "UnitStats")]
public class UnitStats : ScriptableObject
{
    public float moveSpeed = 5f;
    public float acceleration = 20f;
    public float turnSpeed = 60f;
    public float interactionRadius = 0.7f;
    public float unitBunusTime = 3f;

    public float minDirectionDistance = 2f;
    public float maxDirectionDistance = 10f;
}
