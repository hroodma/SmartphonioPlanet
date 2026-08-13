using UnityEngine;

[CreateAssetMenu(fileName = "Booster Stats", menuName = "UnitStats/Boosters")]
public class BoosterStats : MovementStats
{
    [Header("Values")]
    public float value = 10f;
}