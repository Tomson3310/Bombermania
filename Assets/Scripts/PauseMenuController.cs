using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Główne okna UI")]
    public GameObject pauseMenuBackground;
    public GameObject mainPausePanel;
    public GameObject optionsPanel;
    public GameObject helpPanel;
    public GameObject confirmQuitPanel;

    [Header("Suwaki Opcji")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private bool isPaused = false;
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
                
        controls.Player.Pause.performed += ctx => TogglePause();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        pauseMenuBackground.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // --- PAUSE LOGIC ---

    public void TogglePause()
    {
        // If the game is already paused, resume it; otherwise, pause it
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;         // Stop the game time
        AudioListener.pause = true;  // Stop the music and sound effects

        pauseMenuBackground.SetActive(true);
        ShowMainPanel();

        if (AudioManager.Instance != null)
        {
            if (musicSlider != null) musicSlider.value = AudioManager.Instance.GetMusicVolume();
            if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.GetSFXVolume();
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;          // Resume time
        AudioListener.pause = false;  // Resume music and sound effects

        pauseMenuBackground.SetActive(false);
    }

    // --- PANEL MANAGEMENT ---

    public void ShowMainPanel()
    {
        mainPausePanel.SetActive(true);
        optionsPanel.SetActive(false);
        helpPanel.SetActive(false);
        confirmQuitPanel.SetActive(false);
    }

    public void ShowOptionsPanel()
    {
        mainPausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void ShowHelpPanel()
    {
        mainPausePanel.SetActive(false);
        helpPanel.SetActive(true);
    }

    public void ShowConfirmQuitPanel()
    {
        mainPausePanel.SetActive(false);
        confirmQuitPanel.SetActive(true);
    }

    // --- VOLUME CONTROLS ---

    private void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
    }

    private void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
    }

    // --- QUIT TO MENU ---

    public void ConfirmQuitToMenu()
    {
        // Before quitting to the main menu, ensure the game is resumed and all necessary cleanup is done
        Time.timeScale = 1f;
        AudioListener.pause = false;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        // Cleanup GameManager state to avoid carrying over any game state to the main menu
        if (GameManager.Instance != null)
        {
            // Kill all coroutines to prevent any ongoing game logic from running after quitting
            GameManager.Instance.StopAllCoroutines();

            GameManager.Instance.ClearSavedSession();
            GameManager.Instance.uniquePowerUpsInInventory.Clear();
            GameManager.Instance.isLevelActive = false;
        }
            
        SceneManager.LoadScene(0);
    }
}