using UnityEngine;

public class PlayerPrefsScoreRepository : IScoreRepository
{
    private const string HIGH_SCORE_KEY = "HighScore";

    public int GetHighScore()
    {
        return PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    public void SaveHighScore(int score)
    {
        if (score > GetHighScore())
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, score);
            PlayerPrefs.Save();
        }
    }
}