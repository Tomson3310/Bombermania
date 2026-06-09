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
    public float gameOverDisplayDuration = 4f;

    public void ClearSavedSession()
    {
        hasSavedSession = false;
        savedInventoryIcons.Clear();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // VERY IMPORTANT: Keeps the GameManager alive across scene loads
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

            // Update timer display with ceiling to avoid showing 0.9s as 0
            UIManager.Instance.UpdateTimerDisplay(Mathf.CeilToInt(levelTimer));

            if (levelTimer <= 0f)
            {
                levelTimer = 0f;
                isLevelActive = false;
                UIManager.Instance.UpdateTimerDisplay(0);                                
                PlayerStats player = FindAnyObjectByType<PlayerStats>();
                if (player != null)
                {
                    player.LoseLife();
                }
            }
        }
    }

    public LevelData GetCurrentLevelData()
    {
        int listIndex = currentLevel - 1; // Level 1 is at index 0
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

    public void LoadNextLevel()
    {
        Debug.Log($"<color=cyan>[GameManager]</color> LoadNextLevel wywołany! Ukończono poziom {currentLevel}.");

        // Save current player stats before loading the next level
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
            Debug.Log($"<color=cyan>[GameManager]</color> Statystyki gracza pomyślnie zapisane do sejfu przed zmianą poziomu.");
        }

        if (UIManager.Instance != null)
        {
            savedInventoryIcons = UIManager.Instance.GetCollectedIcons();
        }

        currentLevel++;
        isLevelActive = false;

        if (currentLevel - 1 < allLevels.Count)
        {
            Debug.Log($"<color=cyan>[GameManager]</color> Ładuję scenę dla Poziomu {currentLevel}...");            
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
        else
        {
            Debug.Log("<color=yellow>[GameManager]</color> Gratulacje! Przeszedłeś wszystkie poziomy!");
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
        
        int livesToShow = 3;

        if (hasSavedSession)
        {
            // after death or loading a saved game
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
        yield return new WaitForSeconds(3f);
                
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideIntermission();
        }

        // Starting the level after the intermission
        isLevelActive = true;
        Debug.Log("<color=cyan>[GameManager]</color> LevelStartSequence KONIEC. Kurtyna zeszła. Gra ożywa (isLevelActive = true)!");
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
        // 1. Pokazujemy czerwony ekran Game Over
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }

        // 2. Dajemy graczowi czas na przetrawienie porażki (używając Twojej nowej zmiennej!)
        yield return new WaitForSeconds(gameOverDisplayDuration);

        // 3. SPRAWDZAMY CZY TO JEST NOWY REKORD
        if (HighScoreManager.IsHighScore(score))
        {
            Debug.Log("<color=yellow>[GameManager]</color> Nowy rekord! Zatrzymuję powrót do menu i proszę o NICK.");

            // Opcjonalnie wyłączamy ekran Game Over, żeby panel wpisywania był wyraźniejszy
            if (UIManager.Instance != null)
            {
                // UIManager.Instance.gameOverPanel.SetActive(false); // Odkomentuj jeśli wolisz
                UIManager.Instance.ShowHighScoreInput();
            }

            // Coroutine się tutaj kończy. Czekamy, aż gracz wpisze NICK i kliknie przycisk.
        }
        else
        {
            // Zwykła śmierć - brak rekordu
            Debug.Log("<color=cyan>[GameManager]</color> Brak rekordu. Wracam do Menu Głównego.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    // Tę metodę wywołuje UIManager, gdy gracz kliknie przycisk ZAPISZ
    public void SaveHighScoreAndExit(string playerName)
    {
        // Zapisujemy wynik do naszego menedżera
        HighScoreManager.AddScore(playerName, score);

        // Ustawiamy specjalną flagę dla MainMenu: "Hej, tym razem pokaż od razu tablicę wyników!"
        PlayerPrefs.SetInt("ShowLeaderboard", 1);
        PlayerPrefs.Save();

        // Wracamy do Menu Głównego (indeks 0)
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