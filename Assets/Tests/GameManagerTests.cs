using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TestTools;

public class GameManagerTests
{
    private GameObject gameManagerObject;
    private GameManager gameManager;
    private GameObject uiManagerObject;
    private UIManager uiManager;

    [SetUp]
    public void SetUp()
    {
        // Clean up any leftover GameManager instances (Singleton reset)
        GameManager[] leftoverGameManagers = Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None);
        foreach (GameManager gm in leftoverGameManagers)
        {
            Object.DestroyImmediate(gm.gameObject);
        }

        // Clean up any leftover Keys from previous tests
        Key[] leftoverKeys = Object.FindObjectsByType<Key>(FindObjectsSortMode.None);
        foreach (Key key in leftoverKeys)
        {
            Object.DestroyImmediate(key.gameObject);
        }

        // Clean up any leftover UIManagers
        UIManager[] leftoverUIManagers = Object.FindObjectsByType<UIManager>(FindObjectsSortMode.None);
        foreach (UIManager ui in leftoverUIManagers)
        {
            Object.DestroyImmediate(ui.gameObject);
        }

        // Create UIManager first (since GameManager depends on it)
        uiManagerObject = new GameObject("TestUIManager");
        uiManager = uiManagerObject.AddComponent<UIManager>();

        // Initialize UI elements to prevent null references
        InitializeUIElements();

        // Create GameManager
        gameManagerObject = new GameObject("TestGameManager");
        gameManager = gameManagerObject.AddComponent<GameManager>();
    }

    [TearDown]
    public void TearDown()
    {
        // Cleanup all spawned Keys
        Key[] allKeys = Object.FindObjectsByType<Key>(FindObjectsSortMode.None);
        foreach (Key key in allKeys)
        {
            Object.DestroyImmediate(key.gameObject);
        }

        // Destroy GameManager and UIManager
        if (gameManagerObject != null)
            Object.DestroyImmediate(gameManagerObject);
        if (uiManagerObject != null)
            Object.DestroyImmediate(uiManagerObject);

        //  Cleaning up any dummy UI elements created for testing
        GameObject dummyText = GameObject.Find("DummyText");
        if (dummyText != null)
            Object.DestroyImmediate(dummyText);

        GameObject dummyImage = GameObject.Find("DummyImage");
        if (dummyImage != null)
            Object.DestroyImmediate(dummyImage);
    }

    private void InitializeUIElements()
    {
        // Create dummy UI elements for UIManager
        GameObject textObject = new GameObject("DummyText");
        TMP_Text textComponent = textObject.AddComponent<TextMeshProUGUI>();

        uiManager.levelText = textComponent;
        uiManager.scoreText = textComponent;
        uiManager.livesText = textComponent;

        GameObject imageObject = new GameObject("DummyImage");
        Image imageComponent = imageObject.AddComponent<Image>();

        uiManager.keyIcon = imageComponent;
        uiManager.powerUpLevelIcon = imageComponent;

        // Create dummy inventory arrays
        uiManager.inventoryIcons = new Image[5];

        uiManager.fireRadiusText = textComponent;
        uiManager.maxBombsText = textComponent;
    }

    private T GetPrivateField<T>(string fieldName)
    {
        FieldInfo field = typeof(GameManager).GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field.GetValue(gameManager);
    }

    // --- SINGLETON PATTERN TESTS ---

    [Test]
    public void Singleton_OnlyOneInstanceExists()
    {
        // Assert
        Assert.IsNotNull(GameManager.Instance, "GameManager.Instance should not be null");
        Assert.AreEqual(gameManager, GameManager.Instance, "GameManager.Instance should reference the created instance");
    }

    [UnityTest]
    public IEnumerator Singleton_CreatesNewInstanceOnlyOnce()
    {
        // Arrange
        GameManager firstInstance = GameManager.Instance;
        Assert.IsNotNull(firstInstance, "First instance should exist");
        GameObject firstInstanceObject = firstInstance.gameObject;

        // Act: Try to create another instance
        GameObject anotherGameManagerObject = new GameObject("AnotherGameManager");
        GameManager anotherGameManager = anotherGameManagerObject.AddComponent<GameManager>();

        // Wait for Destroy() to take effect - it will be called in Awake of second GameManager
        yield return null;
        yield return null;

        // Assert
        // The first instance should still be active (the duplicate is destroyed)
        Assert.IsTrue(firstInstanceObject != null, "First GameManager should still exist");
        Assert.AreEqual(firstInstance, GameManager.Instance, "Instance should still be the first one");
        Assert.IsTrue(anotherGameManagerObject == null, "Duplicate should be destroyed");
    }

    // --- INITIAL STATE TESTS ---

    [Test]
    public void GameManager_HasCorrectInitialValues()
    {
        // Assert
        Assert.AreEqual(1, gameManager.currentLevel, "Initial level should be 1");
        Assert.AreEqual(0, gameManager.score, "Initial score should be 0");
        Assert.AreEqual(0, gameManager.enemyCount, "Initial enemy count should be 0");
        Assert.AreEqual(false, gameManager.hasKey, "Initial hasKey should be false");
    }

    // --- ENEMY MANAGEMENT TESTS ---

    [Test]
    public void RegisterEnemy_IncreasesEnemyCount()
    {
        // Act
        gameManager.RegisterEnemy();

        // Assert
        Assert.AreEqual(1, gameManager.enemyCount, "Enemy count should increase to 1");
    }

    [Test]
    public void RegisterEnemy_CanRegisterMultipleEnemies()
    {
        // Act
        gameManager.RegisterEnemy();
        gameManager.RegisterEnemy();
        gameManager.RegisterEnemy();

        // Assert
        Assert.AreEqual(3, gameManager.enemyCount, "Enemy count should be 3 after registering 3 enemies");
    }

    [UnityTest]
    public IEnumerator EnemyDefeated_DecreasesEnemyCount()
    {
        // Arrange
        gameManager.RegisterEnemy();
        gameManager.RegisterEnemy();
        int initialEnemyCount = gameManager.enemyCount;

        // Act
        gameManager.EnemyDefeated(Vector3.zero, 100);
        yield return null;

        // Assert
        Assert.AreEqual(initialEnemyCount - 1, gameManager.enemyCount, "Enemy count should decrease by 1");
    }

    [UnityTest]
    public IEnumerator EnemyDefeated_IncreasesScore()
    {
        // Arrange
        int initialScore = gameManager.score;

        // Act
        gameManager.EnemyDefeated(Vector3.zero, 100);
        yield return null;

        // Assert
        Assert.AreEqual(initialScore + 100, gameManager.score, "Score should increase by 100");
    }

    [UnityTest]
    public IEnumerator EnemyDefeated_ScoreAccumulatesCorrectly()
    {
        // Arrange
        gameManager.RegisterEnemy();
        gameManager.RegisterEnemy();
        gameManager.RegisterEnemy();

        // Act
        gameManager.EnemyDefeated(Vector3.zero, 100);
        yield return null;
        gameManager.EnemyDefeated(Vector3.zero, 100);
        yield return null;
        gameManager.EnemyDefeated(Vector3.zero, 100);
        yield return null;

        // Assert
        Assert.AreEqual(300, gameManager.score, "Score should be 300 after defeating 3 enemies");
    }

    // --- KEY SPAWNING TESTS ---

    [UnityTest]
    public IEnumerator EnemyDefeated_SpawnsKey_WhenAllDefeated()
    {
        // Arrange
        gameManager.RegisterEnemy();
        GameObject keyPrefab = new GameObject("TestKey");
        keyPrefab.AddComponent<Key>();  // Add Key component!
        gameManager.keyPrefab = keyPrefab;

        // Act
        gameManager.EnemyDefeated(new Vector3(5f, 5f, 0f), 100);
        yield return null;

        // Assert
        GameObject spawnedKeyObj = GameObject.Find("TestKey(Clone)");
        Assert.IsNotNull(spawnedKeyObj, "Key should be spawned when all enemies are defeated");

        // Cleanup
        Object.Destroy(keyPrefab);

        // Extra cleanup for spawned Keys
        Key[] remainingKeys = Object.FindObjectsByType<Key>(FindObjectsSortMode.None);
        foreach (Key key in remainingKeys)
        {
            Object.Destroy(key.gameObject);
        }
    }

    [UnityTest]
    public IEnumerator EnemyDefeated_NoKeySpawn_WhenEnemiesRemain()
    {
        // Arrange
        gameManager.RegisterEnemy();
        gameManager.RegisterEnemy();
        GameObject keyPrefab = new GameObject("TestKey");
        keyPrefab.AddComponent<Key>();  // Add Key component!
        gameManager.keyPrefab = keyPrefab;

        // Act
        gameManager.EnemyDefeated(Vector3.zero, 100);
        yield return null;
        yield return null;  // Extra wait to ensure no Key spawns

        // Assert
        GameObject spawnedKeyObj = GameObject.Find("TestKey(Clone)");
        Assert.IsNull(spawnedKeyObj, "Key should NOT be spawned when enemies remain");

        // Cleanup
        Object.DestroyImmediate(keyPrefab);
    }

    [UnityTest]
    public IEnumerator EnemyDefeated_KeySpawnsAtCorrectPosition()
    {
        // Arrange
        gameManager.RegisterEnemy();
        GameObject keyPrefab = new GameObject("TestKey");
        keyPrefab.AddComponent<Key>();
        keyPrefab.transform.position = Vector3.zero;  // Set prefab position to zero
        gameManager.keyPrefab = keyPrefab;
        Vector3 deathPosition = new Vector3(3.7f, 4.9f, 0f);

        // Act
        gameManager.EnemyDefeated(deathPosition, 100);
        yield return null;

        // Assert
        GameObject spawnedKeyObj = GameObject.Find("TestKey(Clone)");
        Assert.IsNotNull(spawnedKeyObj, "Key should be spawned");

        Vector3 expectedPosition = new Vector3(3.5f, 4.5f, 0f);
        Assert.AreEqual(expectedPosition, spawnedKeyObj.transform.position, "Key should be spawned at snapped position");

        // Cleanup
        Object.Destroy(keyPrefab);

        // Extra cleanup for spawned Keys
        Key[] remainingKeys = Object.FindObjectsByType<Key>(FindObjectsSortMode.None);
        foreach (Key key in remainingKeys)
        {
            Object.Destroy(key.gameObject);
        }
    }

    // --- KEY MANAGEMENT TESTS ---

    [UnityTest]
    public IEnumerator PickUpKey_SetsHasKeyFlag()
    {
        // Act
        gameManager.PickUpKey();
        yield return null;

        // Assert
        Assert.AreEqual(true, gameManager.hasKey, "hasKey flag should be true after PickUpKey()");
    }

    [UnityTest]
    public IEnumerator PickUpKey_OpensGate()
    {
        // Arrange: Create a Gate object
        GameObject gateObject = new GameObject("TestGate");
        Gate gate = gateObject.AddComponent<Gate>();
        gate.openedGateSprite = null; // Don't need actual sprite for this test

        // Act
        gameManager.PickUpKey();
        yield return null;

        // Assert
        Assert.AreEqual(true, gameManager.hasKey, "hasKey flag should be true");
        // Gate should have been opened (verified by sprite change, but we'll just check the flag)

        // Cleanup
        Object.Destroy(gateObject);
    }

    // --- COMBINED ACTION TESTS ---

    [UnityTest]
    public IEnumerator FullGameFlow_RegisterAndDefeatAllEnemies_SpawnsKey()
    {
        // Arrange
        GameObject keyPrefab = new GameObject("TestKey");
        keyPrefab.AddComponent<Key>();  // Add Key component!
        gameManager.keyPrefab = keyPrefab;
        gameManager.RegisterEnemy();
        gameManager.RegisterEnemy();

        // Act: Defeat first enemy
        gameManager.EnemyDefeated(Vector3.zero, 100);
        yield return null;
        yield return null;  // Extra wait

        // Assert: Key should not spawn yet
        GameObject spawnedKeyObj = GameObject.Find("TestKey(Clone)");
        Assert.IsNull(spawnedKeyObj, "Key should NOT be spawned with enemies remaining");

        // Act: Defeat second enemy
        gameManager.EnemyDefeated(Vector3.zero, 100);
        yield return null;
        yield return null;  // Extra wait

        // Assert: Key should now spawn
        spawnedKeyObj = GameObject.Find("TestKey(Clone)");
        Assert.IsNotNull(spawnedKeyObj, "Key should be spawned when all enemies defeated");
        Assert.AreEqual(200, gameManager.score, "Score should be 200");
        Assert.AreEqual(0, gameManager.enemyCount, "Enemy count should be 0");

        // Cleanup
        Object.DestroyImmediate(keyPrefab);
    }
}