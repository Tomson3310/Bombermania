using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsTests
{
    private GameObject playerStatsObject;
    private PlayerStats playerStats;
    private GameObject uiManagerObject;
    private UIManager uiManager;

    [SetUp]
    public void SetUp()
    {
        // 1. Clean up any leftover UIManagers from previous tests
        UIManager[] leftoverUIManagers = Object.FindObjectsByType<UIManager>(FindObjectsSortMode.None);
        foreach (UIManager ui in leftoverUIManagers)
        {
            Object.DestroyImmediate(ui.gameObject);
        }

        // 2. Create UIManager first (since PlayerStats depends on it in Start() and other methods)
        uiManagerObject = new GameObject("TestUIManager");
        uiManager = uiManagerObject.AddComponent<UIManager>();

        // 3. Initialize UI elements to prevent NullReferenceExceptions inside UIManager
        InitializeUIElements();

        // 4. Create PlayerStats
        playerStatsObject = new GameObject("TestPlayerStats");
        playerStats = playerStatsObject.AddComponent<PlayerStats>();
    }

    [TearDown]
    public void TearDown()
    {
        // Cleanup using DestroyImmediate for testing environments
        if (playerStatsObject != null)
            Object.DestroyImmediate(playerStatsObject);

        if (uiManagerObject != null)
            Object.DestroyImmediate(uiManagerObject);
    }

    private void InitializeUIElements()
    {
        // Create dummy UI elements for UIManager
        GameObject textObject = new GameObject("DummyText");
        TMP_Text textComponent = textObject.AddComponent<TextMeshProUGUI>();

        uiManager.levelText = textComponent;
        uiManager.scoreText = textComponent;
        uiManager.livesText = textComponent;
        uiManager.fireRadiusText = textComponent;
        uiManager.maxBombsText = textComponent;

        GameObject imageObject = new GameObject("DummyImage");
        Image imageComponent = imageObject.AddComponent<Image>();

        uiManager.keyIcon = imageComponent;
        uiManager.powerUpLevelIcon = imageComponent;

        // Create dummy inventory arrays
        uiManager.inventoryIcons = new Image[5];
        uiManager.inventoryCircles = new GameObject[5];

        // FILL THE ARRAYS WITH ACTUAL DUMMY OBJECTS
        for (int i = 0; i < 5; i++)
        {
            // Create a GameObject for the circle
            uiManager.inventoryCircles[i] = new GameObject($"DummyCircle_{i}");

            // Create a GameObject for the icon and add an Image component to it
            GameObject iconObj = new GameObject($"DummyIcon_{i}");
            uiManager.inventoryIcons[i] = iconObj.AddComponent<Image>();
        }
    }

    // Helper method to get private field values
    private T GetPrivateField<T>(string fieldName)
    {
        FieldInfo field = typeof(PlayerStats).GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field.GetValue(playerStats);
    }

    // --- INITIAL STATE TESTS ---

    [Test]
    public void PlayerStats_HasCorrectInitialValues()
    {
        // Assert: Check initial property values
        int initialLives = GetPrivateField<int>("lives");
        int initialFireRange = GetPrivateField<int>("fireRange");
        int initialMaxBombs = GetPrivateField<int>("maxBombs");
        bool initialHasDetonator = GetPrivateField<bool>("hasDetonator");

        Assert.AreEqual(3, initialLives, "Initial lives should be 3");
        Assert.AreEqual(1, initialFireRange, "Initial fireRange should be 1");
        Assert.AreEqual(1, initialMaxBombs, "Initial maxBombs should be 1");
        Assert.AreEqual(false, initialHasDetonator, "Initial hasDetonator should be false");
    }

    [Test]
    public void FireRange_ReturnsCorrectValue()
    {
        // Assert
        Assert.AreEqual(1, playerStats.FireRange, "FireRange property should return initial value of 1");
    }

    [Test]
    public void MaxBombs_ReturnsCorrectValue()
    {
        // Assert
        Assert.AreEqual(1, playerStats.MaxBombs, "MaxBombs property should return initial value of 1");
    }

    [Test]
    public void HasDetonator_ReturnsFalseInitially()
    {
        // Assert
        Assert.AreEqual(false, playerStats.HasDetonator, "HasDetonator should be false initially");
    }

    // --- LOSE LIFE TESTS ---

    [Test]
    public void LoseLife_DecreasesLiveCount()
    {
        // Act - Przekazujemy wymagany argument DeathType
        playerStats.LoseLife(DeathType.Normal);

        // Assert
        int currentLives = GetPrivateField<int>("lives");
        Assert.AreEqual(2, currentLives, "Lives should decrease from 3 to 2 after LoseLife()");
    }

    [Test]
    public void LoseLife_CanBeLostMultipleTimes()
    {
        // Act - Przekazujemy wymagany argument DeathType
        playerStats.LoseLife(DeathType.Normal);
        playerStats.LoseLife(DeathType.Normal);
        playerStats.LoseLife(DeathType.Normal);

        // Assert
        int currentLives = GetPrivateField<int>("lives");
        Assert.AreEqual(0, currentLives, "Lives should be 0 after losing 3 times");
    }

    [Test]
    public void LoseLife_CanGoNegative()
    {
        // Act: Lose more lives than available - Przekazujemy wymagany argument DeathType
        playerStats.LoseLife(DeathType.Normal);
        playerStats.LoseLife(DeathType.Normal);
        playerStats.LoseLife(DeathType.Normal);
        playerStats.LoseLife(DeathType.Normal);

        // Assert
        int currentLives = GetPrivateField<int>("lives");
        Assert.AreEqual(-1, currentLives, "Lives can go negative");
    }

    // --- INCREASE FIRE RADIUS TESTS ---

    [Test]
    public void IncreaseFireRadius_IncreasesFireRadiusByOne()
    {
        // Act
        playerStats.IncreaseFireRadius(1);

        // Assert
        Assert.AreEqual(2, playerStats.FireRange, "FireRadius should increase from 1 to 2");
    }

    [Test]
    public void IncreaseFireRadius_CanBeCalledMultipleTimes()
    {
        // Act
        playerStats.IncreaseFireRadius(1);
        playerStats.IncreaseFireRadius(1);
        playerStats.IncreaseFireRadius(1);

        // Assert
        Assert.AreEqual(4, playerStats.FireRange, "FireRadius should increase to 4 after three increases");
    }

    // --- INCREASE MAX BOMBS TESTS ---

    [Test]
    public void IncreaseMaxBombs_IncreasesMaxBombsByOne()
    {
        // Act
        playerStats.IncreaseMaxBombs(1);

        // Assert
        Assert.AreEqual(2, playerStats.MaxBombs, "MaxBombs should increase from 1 to 2");
    }

    [Test]
    public void IncreaseMaxBombs_CanBeCalledMultipleTimes()
    {
        // Act
        playerStats.IncreaseMaxBombs(1);
        playerStats.IncreaseMaxBombs(1);
        playerStats.IncreaseMaxBombs(1);

        // Assert
        Assert.AreEqual(4, playerStats.MaxBombs, "MaxBombs should increase to 4 after three increases");
    }

    // --- ENABLE DETONATOR TESTS ---

    [Test]
    public void EnableDetonator_SetsHasDetonatorToTrue()
    {
        // Act
        playerStats.EnableDetonator();

        // Assert
        Assert.AreEqual(true, playerStats.HasDetonator, "HasDetonator should be true after EnableDetonator()");
    }

    [Test]
    public void EnableDetonator_CanOnlyBeEnabledOnce()
    {
        // Act: Call it twice
        playerStats.EnableDetonator();
        playerStats.EnableDetonator();

        // Assert
        Assert.AreEqual(true, playerStats.HasDetonator, "HasDetonator should still be true");
    }

    // --- COMBINED ACTION TESTS ---

    [Test]
    public void AllModifiers_CanBeAppliedTogether()
    {
        // Act - Przekazujemy wymagany argument DeathType
        playerStats.IncreaseFireRadius(1);
        playerStats.IncreaseMaxBombs(1);
        playerStats.EnableDetonator();
        playerStats.LoseLife(DeathType.Normal);

        // Assert
        int currentLives = GetPrivateField<int>("lives");

        Assert.AreEqual(2, currentLives, "Lives should be 2");
        Assert.AreEqual(2, playerStats.FireRange, "FireRadius should be 2");
        Assert.AreEqual(2, playerStats.MaxBombs, "MaxBombs should be 2");
        Assert.AreEqual(true, playerStats.HasDetonator, "HasDetonator should be true");
    }
}