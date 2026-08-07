using UnityEngine;

[CreateAssetMenu(fileName = "Unit Stats", menuName = "UnitStats")]
public class UnitStats : ScriptableObject
{
    public float moveSpeed = 5f;
    public float acceleration = 20f;
    public float turnSpeed = 60f;
}
