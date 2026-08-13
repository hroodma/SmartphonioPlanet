using UnityEngine;

[CreateAssetMenu(fileName = "Player Stats", menuName = "UnitStats/Player")]
public class PlayerStats : MovementStats
{
    [Header("Interact")]
    public float interactionRadius = 0.7f;
}