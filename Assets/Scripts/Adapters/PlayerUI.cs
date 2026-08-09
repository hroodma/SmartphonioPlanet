using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour, IPlayerUI
{
    [SerializeField] TMP_Text _timerUI;
    [SerializeField] TMP_Text _caughtAnimalsUI;

    public void UpdateUI(UnitData playerData, float timer)
    {
        _timerUI.text = timer.ToString();

        _caughtAnimalsUI.text = playerData.CaughtAnimals.ToString();
    }
}
