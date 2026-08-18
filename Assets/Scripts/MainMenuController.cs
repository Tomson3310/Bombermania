using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Leaderboard System")]
    public LeaderboardUI leaderboardUI;

    [Header("Opcje - UI")]
    public GameObject optionsPanel;
    public Slider musicSlider;
    public Slider sfxSlider;

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

        // Options
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        if (AudioManager.Instance != null)
        {
            // Set the sliders to the current volume settings and add listeners to update the AudioManager when they change
            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.Instance.GetMusicVolume();
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.GetSFXVolume();
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            }
        }
    }

    // --- Options Panel ---

    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        PlayerPrefs.Save();
    }

    private void SetMusicVolume(float val)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(val);
    }

    private void SetSFXVolume(float val)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(val);
    }

    // --- Game Controls ---

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