using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Progress")]
    public int currentLevel = 1;
    public int score = 0;
    public int enemyCount = 0;

    [Header("Player State")]
    public bool hasKey = false;
    public GameObject keyPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        UIManager.Instance.UpdateLevel(currentLevel);
        UIManager.Instance.UpdateScore(score);
    }

    public void RegisterEnemy() { enemyCount++; }

    public void EnemyDefeated(Vector3 deathPosition)
    {
        enemyCount--;

        score += 100;
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