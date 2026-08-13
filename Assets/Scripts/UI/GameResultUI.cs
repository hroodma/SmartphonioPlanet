using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameResultUI : MonoBehaviour
{
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text highScoreText;

    public void ShowResult(int caughtResult, int highScore)
    {
        resultPanel.SetActive(true);
        resultText.text = $"Поймано зверей: {caughtResult}";

        bool isNewRecord = caughtResult >= highScore && caughtResult > 0;

        if (isNewRecord)
        {
            highScoreText.text = $"🏆 Новый рекорд: {caughtResult}!";
            highScoreText.color = Color.yellow;
        }
        else
        {
            highScoreText.text = $"Рекорд: {highScore}";
            highScoreText.color = Color.white;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void QuitGame() => Application.Quit();
}