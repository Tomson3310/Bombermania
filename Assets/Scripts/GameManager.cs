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
                PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
                if (player != null)
                {
                    player.Die();
                }
            }
        }
    }

    public LevelData GetCurrentLevelData()
    {
        int listIndex = currentLevel - 1; // Level 1 is at index 0
        if (listIndex >= 0 && listIndex < allLevels.Count)
        {
            return allLevels[listIndex];
        }
        return null;
    }

    public void LoadNextLevel()
    {
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
        }

        if (UIManager.Instance != null)
        {
            savedInventoryIcons = UIManager.Instance.GetCollectedIcons();
        }

        currentLevel++;
        isLevelActive = false;

        if (currentLevel - 1 < allLevels.Count)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
        else
        {
            Debug.Log("Gratulacje! Przeszedłeś wszystkie poziomy!");
        }
    }

    public void StartLevel(int startTime)
    {
        levelTimer = startTime;
        isLevelActive = true;
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
    }

    public void RegisterEnemy() { enemyCount++; }

    public void EnemyDefeated(Vector3 deathPosition, int points)
    {
        enemyCount--;

        score += points;
        UIManager.Instance.UpdateScore(score);

        Debug.Log("Enemy defeated! Remaining: " + enemyCount);

        if (enemyCount <= 0) { SpawnKey(deathPosition); }
    }

    private void SpawnKey(Vector3 spawnPosition)
    {
        Debug.Log("Last enemy defeated! Key spawned!");
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