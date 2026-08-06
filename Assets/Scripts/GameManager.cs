using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Progress")]
    public List<LevelData> allLevels;
    public int currentLevel = 1;
    public int score = 0;
    public int enemyCount = 0;
    public float levelTimer = 0f;
    public bool isLevelActive = false;

    [Header("Player State")]
    public bool hasKey = false;
    public GameObject keyPrefab;

    [Header("Saved Player Stats Vault")]
    public bool hasSavedSession = false;
    public int savedLives;
    public int savedFireRange;
    public int savedMaxBombs;
    public bool savedHasDetonator;
    public bool savedHasCratePass;
    public bool savedHasBombPass;
    public float savedPlayerMoveSpeed;
    public List<Sprite> savedInventoryIcons = new List<Sprite>();

    private List<PowerUpData> activePowerUpPool = new List<PowerUpData>();
    public List<PowerUpData> uniquePowerUpsInInventory = new List<PowerUpData>();

    [HideInInspector] public float basePlayerSpeed;

    [Header("Game Over Screen Settings")]
    public float gameOverDisplayDuration = 5f;
    public AudioClip gameoverSound;
    public float gameoverVolume = 0.5f;

    [Header("Game Completed Screen Settings")]
    public float gameCompletedDisplayDuration = 4f;

    [Header("Bonus Scoring Settings")]
    public int pointsPerSecondRemaining = 10;
    public float countdownTickDelay = 0.02f;
    public float postCountdownDelay = 0.5f;
    public AudioClip timeTickSound;
    public float timeTickMaxPitch = 2.5f;
    public float timeTickVolume = 0.5f;

    [Header("Life Bonus Settings (End Game)")]
    public int pointsPerLifeRemaining = 5000;
    public float lifeBonusTickDelay = 0.5f;
    public float postLifeBonusDelay = 1.0f;
    public AudioClip lifeTickSound;
    public float lifeTickPitchStep = 0.3f;
    public float lifeTickVolume = 0.5f;

    public void ClearSavedSession()
    {
        hasSavedSession = false;
        savedInventoryIcons.Clear();
    }

    private void Awake()
    {        
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);       
    }

    private void Start()
    {        
        UIManager.Instance.UpdateLevel(currentLevel);
        UIManager.Instance.UpdateScore(score);        
    }

    private void Update()
    {
        if (isLevelActive)
        {
            levelTimer -= Time.deltaTime;
           
            UIManager.Instance.UpdateTimerDisplay(Mathf.CeilToInt(levelTimer));

            if (levelTimer <= 0f)
            {
                levelTimer = 0f;
                isLevelActive = false;
                UIManager.Instance.UpdateTimerDisplay(0);
                PlayerStats player = FindAnyObjectByType<PlayerStats>();
                if (player != null)
                {
                    player.LoseLife(DeathType.Normal);
                }
            }
        }
    }

    public LevelData GetCurrentLevelData()
    {
        int listIndex = currentLevel - 1;
        if (listIndex >= 0 && listIndex < allLevels.Count)
        {
            activePowerUpPool = new List<PowerUpData>(allLevels[listIndex].PowerUpsToSpawn);
            if (uniquePowerUpsInInventory.Count > 0)
            {
                // remove unique power-ups that the player already has from the pool to prevent duplicates
                activePowerUpPool.RemoveAll(item => uniquePowerUpsInInventory.Contains(item));
            }
            return allLevels[listIndex];
        }
        return null;
    }

    private System.Collections.IEnumerator LifeBonusSequence()
    {
        Debug.Log($"<color=cyan>[GameManager]</color> Rozpoczynam odliczanie bonusu za życia. Pozostało żyć: {savedLives}");
        float currentPitch = 1f;

        while (savedLives > 0)
        {
            savedLives--;
            score += pointsPerLifeRemaining;
                        
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateLives(savedLives);
                UIManager.Instance.UpdateScore(score);
            }

            if (AudioManager.Instance != null && lifeTickSound != null)
            {
                AudioManager.Instance.PlaySFX(lifeTickSound, lifeTickVolume, currentPitch);
                currentPitch += lifeTickPitchStep;
            }

            yield return new WaitForSeconds(lifeBonusTickDelay);
        }
                
        yield return new WaitForSeconds(postLifeBonusDelay);
                
        StartCoroutine(GameCompletedSequence());
    }

    public void LoadNextLevel()
    {
        Debug.Log($"<color=cyan>[GameManager]</color> Zaczynam sekwencję przejścia poziomu {currentLevel}.");
                
        isLevelActive = false;
                
        StartCoroutine(LevelCompletedSequence());
    }

    private System.Collections.IEnumerator LevelCompletedSequence()
    {
        float musicDuration = 0f;
               
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();

            if (AudioManager.Instance.victoryMusic != null)
            {
                AudioManager.Instance.PlayMusic(AudioManager.Instance.victoryMusic, false);
                
                musicDuration = AudioManager.Instance.victoryMusic.length;
            }
        }
                
        if (musicDuration > 0f)
        {
            yield return new WaitForSeconds(musicDuration);
        }
               
        Debug.Log($"<color=cyan>[GameManager]</color> Muzyka zwycięstwa zakończona. Odliczam bonus za czas!");
        StartCoroutine(TimeBonusSequence());
    }


    private System.Collections.IEnumerator TimeBonusSequence()
    {        
        int remainingSeconds = Mathf.CeilToInt(levelTimer);
        int initialSeconds = remainingSeconds;
        
        float currentPitch = 1f;
        float pitchStep = 0f;
        
        if (initialSeconds > 0)
        {
            pitchStep = (timeTickMaxPitch - 1f) / initialSeconds;
        }

        while (remainingSeconds > 0)
        {
            remainingSeconds--;
            levelTimer = remainingSeconds;
                        
            score += pointsPerSecondRemaining;
                        
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateTimerDisplay(remainingSeconds);
                UIManager.Instance.UpdateScore(score);
            }

            if (AudioManager.Instance != null && timeTickSound != null)
            {
                AudioManager.Instance.PlaySFX(timeTickSound, timeTickVolume, currentPitch);
                currentPitch += pitchStep;
            }


            yield return new WaitForSeconds(countdownTickDelay);
        }

        
        yield return new WaitForSeconds(postCountdownDelay);
                
        PlayerStats currentStats = FindAnyObjectByType<PlayerStats>();
        if (currentStats != null)
        {
            savedLives = currentStats.Lives;
            savedFireRange = currentStats.FireRange;
            savedMaxBombs = currentStats.MaxBombs;
            savedHasDetonator = currentStats.HasDetonator;
            savedHasCratePass = currentStats.HasCratePass;
            savedHasBombPass = currentStats.HasBombPass;
            savedPlayerMoveSpeed = currentStats.PlayerMoveSpeed;

            hasSavedSession = true;
            Debug.Log($"<color=cyan>[GameManager]</color> Statystyki gracza pomyślnie zapisane do sejfu.");
        }

        if (UIManager.Instance != null)
        {
            savedInventoryIcons = UIManager.Instance.GetCollectedIcons();
        }

        currentLevel++;
                
        if (currentLevel - 1 < allLevels.Count)
        {
            Debug.Log($"<color=cyan>[GameManager]</color> Ładuję scenę dla Poziomu {currentLevel}...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
        else
        {
            Debug.Log("<color=yellow>[GameManager]</color> Gratulacje! Przeszedłeś wszystkie poziomy! Przechodzę do bonusu za życia.");
            
            StartCoroutine(LifeBonusSequence());
        }
    }

    public void StartLevel(int startTime)
    {
        Debug.Log($"<color=cyan>[GameManager]</color> StartLevel wywołany dla poziomu {currentLevel}. Zegar: {startTime}s.");
        levelTimer = startTime;        
        hasKey = false;
        enemyCount = 0;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLevel(currentLevel);
            UIManager.Instance.UpdateScore(score);

            if (hasSavedSession)
            {
                UIManager.Instance.RestoreInventory(savedInventoryIcons);
            }
        }
        StartCoroutine(LevelStartSequence());
    }

    private System.Collections.IEnumerator LevelStartSequence()
    {
        Debug.Log("<color=cyan>[GameManager]</color> LevelStartSequence START. Zamrażam poziom (isLevelActive = false).");
        isLevelActive = false;
                
        if (AudioManager.Instance != null && AudioManager.Instance.intermissionMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.intermissionMusic, false);
        }

        int livesToShow = 3;

        if (hasSavedSession)
        {
            livesToShow = savedLives;
        }
        else
        {
            PlayerStats player = FindAnyObjectByType<PlayerStats>();
            if (player != null)
            {
                livesToShow = player.Lives;
            }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowIntermission(currentLevel, livesToShow);
        }

        Debug.Log("<color=cyan>[GameManager]</color> Ekran kurtyny wyświetlony. Rozpoczynam odliczanie 3 sekund...");
        yield return new WaitForSeconds(4f);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideIntermission();
        }

        isLevelActive = true;
        Debug.Log("<color=cyan>[GameManager]</color> LevelStartSequence KONIEC. Gra ożywa!");
                
        if (AudioManager.Instance != null && AudioManager.Instance.gameplayMusic != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameplayMusic, true);
        }
    }

    public void ResetCurrentLevel()
    {
        Debug.Log($"<color=cyan>[GameManager]</color> ResetCurrentLevel wywołany! Scena będzie przeładowana za ułamek sekundy.");
        isLevelActive = false;
        hasKey = false;
        enemyCount = 0;
        
        savedHasBombPass = false;
        savedHasDetonator = false;
        savedHasCratePass = false;
        savedPlayerMoveSpeed = basePlayerSpeed;
        
        PlayerStats currentStats = FindAnyObjectByType<PlayerStats>();
        if (currentStats != null)
        {
            savedLives = currentStats.Lives;
            savedFireRange = currentStats.FireRange;
            savedMaxBombs = currentStats.MaxBombs;
            Debug.Log($"<color=cyan>[GameManager]</color> Zapisuję do sejfu przed restartem: Życia={savedLives}, Zasięg={savedFireRange}, Bomby={savedMaxBombs}");
        }
        
        hasSavedSession = true;
        
        if (uniquePowerUpsInInventory.Count > 0)
        {
            foreach (PowerUpData uniquePowerUp in uniquePowerUpsInInventory)
            {
                if (savedInventoryIcons.Contains(uniquePowerUp.UiIcon))
                {
                    savedInventoryIcons.Remove(uniquePowerUp.UiIcon);
                }
            }
            uniquePowerUpsInInventory.Clear();
        }
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void GameOver()
    {
        Debug.Log("<color=red>[GameManager]</color> Rozpoczynam sekwencję Game Over.");
        isLevelActive = false;

        // Reset the game state
        ClearSavedSession();
        uniquePowerUpsInInventory.Clear();
        currentLevel = 1;        
        
        StartCoroutine(GameOverSequence());
    }

    private System.Collections.IEnumerator GameOverSequence()
    {        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }
        if (AudioManager.Instance != null && gameoverSound != null)
        {
            AudioManager.Instance.PlaySFX(gameoverSound, gameoverVolume);
        }
                
        yield return new WaitForSeconds(gameOverDisplayDuration);
                
        if (HighScoreManager.IsHighScore(score))
        {
            Debug.Log("<color=yellow>[GameManager]</color> Nowy rekord! Zatrzymuję powrót do menu i proszę o NICK.");
                        
            if (UIManager.Instance != null)
            {                
                UIManager.Instance.ShowHighScoreInput();
            }                        
        }
        else
        {            
            Debug.Log("<color=cyan>[GameManager]</color> Brak rekordu. Wracam do Menu Głównego.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    private System.Collections.IEnumerator GameCompletedSequence()
    {        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameCompleted();
        }
                
        yield return new WaitForSeconds(gameCompletedDisplayDuration);
                
        if (HighScoreManager.IsHighScore(score))
        {
            Debug.Log("<color=yellow>[GameManager]</color> Zwycięstwo i nowy rekord! Proszę o NICK.");

            if (UIManager.Instance != null)
            {                
                UIManager.Instance.ShowHighScoreInput();
            }
        }
        else
        {            
            Debug.Log("<color=cyan>[GameManager]</color> Zwycięstwo, ale brak rekordu. Wracam do Menu Głównego.");
            ClearSavedSession();
            uniquePowerUpsInInventory.Clear();
            currentLevel = 1;
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
        
    public void SaveHighScoreAndExit(string playerName)
    {        
        HighScoreManager.AddScore(playerName, score);
        
        PlayerPrefs.SetInt("ShowLeaderboard", 1);
        PlayerPrefs.Save();
                
        Debug.Log("<color=cyan>[GameManager]</color> Zapisano rekord. Przeładowuję do tablicy wyników w Menu.");
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }



    public void RegisterEnemy() { enemyCount++; }

    public void EnemyDefeated(Vector3 deathPosition, int points)
    {
        enemyCount--;

        score += points;
        UIManager.Instance.UpdateScore(score);        

        if (enemyCount <= 0) { SpawnKey(deathPosition); }
    }

    public PowerUpData GetRandomPowerUpFromPool()
    {
        if (activePowerUpPool.Count == 0) return null;        
        
        int randomIndex = Random.Range(0, activePowerUpPool.Count);
        PowerUpData selectedPowerUp = activePowerUpPool[randomIndex];       
        return selectedPowerUp;
    }

    public void RemovePowerUpFromPool(PowerUpData powerUpToRemove)
    {
        if (activePowerUpPool.Contains(powerUpToRemove))
        {
            activePowerUpPool.Remove(powerUpToRemove);
        }
        if (powerUpToRemove.IsUnique && !uniquePowerUpsInInventory.Contains(powerUpToRemove))
        {
            uniquePowerUpsInInventory.Add(powerUpToRemove);
        }
    }

    private void SpawnKey(Vector3 spawnPosition)
    {        
        float snapX = Mathf.Floor(spawnPosition.x) + 0.5f;
        float snapY = Mathf.Floor(spawnPosition.y) + 0.5f;
        Vector3 centeredPosition = new Vector3(snapX, snapY, 0f);

        if (keyPrefab != null) { Instantiate(keyPrefab, centeredPosition, Quaternion.identity); }
    }

    public void PickUpKey()
    {
        hasKey = true;
        UIManager.Instance.ActivateKey();

        Gate levelGate = FindAnyObjectByType<Gate>();
        if (levelGate != null) { levelGate.OpenGate(); }
    }    

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}