using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameResultUI : MonoBehaviour
{
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;

    public void ShowResult(int caughtResult)
    {
        resultPanel.SetActive(true);
        resultText.text = $"Пойманных зверей: {caughtResult}";
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void QuitGame() => Application.Quit();
}