using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour, IPlayerUI
{
    [SerializeField] TMP_Text _timerUI;
    [SerializeField] TMP_Text _caughtAnimalsUI;
    [SerializeField] TMP_Text _highScoreUI;

    public void UpdateUI(PlayerData playerData, float timer, IScoreRepository scoreRepository)
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        _timerUI.text = $"{minutes}:{seconds:00}";

        _caughtAnimalsUI.text = $"поймано: {playerData.CaughtAnimals}";
        _highScoreUI.text = $"рекорд: {scoreRepository.GetHighScore()}";
    }
}
