using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Leaderboard System")]
    public LeaderboardUI leaderboardUI; // Przeciągnij tu swój LeaderboardPanel w Inspektorze

    private void Start()
    {
        // Sprawdzamy, czy gracz właśnie wpisał nick po zgonie
        if (PlayerPrefs.GetInt("ShowLeaderboard", 0) == 1)
        {
            // Resetujemy flagę od razu, żeby przy normalnym odpaleniu gry się to nie psuło
            PlayerPrefs.SetInt("ShowLeaderboard", 0);
            PlayerPrefs.Save();

            // Otwieramy tablicę z hukiem!
            if (leaderboardUI != null)
            {
                leaderboardUI.OpenLeaderboard();
            }
        }
    }

    public void ShowLeaderboardManually()
    {
        // Metoda do podpięcia pod przycisk "LEADERBOARD" w menu głównym
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