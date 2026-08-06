using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Leaderboard System")]
    public LeaderboardUI leaderboardUI;

    private void Start()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.menuMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic, true);
        }

        if (PlayerPrefs.GetInt("ShowLeaderboard", 0) == 1)
        {            
            PlayerPrefs.SetInt("ShowLeaderboard", 0);
            PlayerPrefs.Save();
                        
            if (leaderboardUI != null)
            {
                leaderboardUI.OpenLeaderboard();
            }
        }
    }

    public void ShowLeaderboardManually()
    {        
        if (leaderboardUI != null)
        {
            leaderboardUI.OpenLeaderboard();
        }
    }

    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearSavedSession();
            GameManager.Instance.uniquePowerUpsInInventory.Clear();
            GameManager.Instance.currentLevel = 1;
            GameManager.Instance.score = 0;
        }

        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Debug.Log("Wychodzenie z gry...");
        Application.Quit();
    }
}