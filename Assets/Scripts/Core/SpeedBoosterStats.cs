using UnityEngine;

[CreateAssetMenu(fileName = "Speed Booster Stats", menuName = "UnitStats/Boosters/Speed")]
public class SpeedBoosterStats : BoosterStats
{
    [Header("Duration")]
    public float duration = 10f;
}