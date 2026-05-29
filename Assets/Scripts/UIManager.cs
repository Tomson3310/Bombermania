using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Top Panel")]
    public TMP_Text levelText;
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public TMP_Text timeText;
    public Image keyIcon;
    public Image powerUpLevelIcon;
    public Sprite keyColorSprite;
    public Sprite powerUpColorSprite;

    [Header("Bottom Panel - Stats")]
    public TMP_Text fireRadiusText;
    public TMP_Text maxBombsText;

    [Header("Bottom Panel - Inventory")]
    public Image[] inventoryIcons;
    public GameObject[] inventoryCircles;
    private int currentInventoryIndex = 0;

    [Header("Intermission Screen")]
    public GameObject intermissionPanel;
    public TMP_Text intermissionLevelText;
    public TMP_Text intermissionLivesText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // --- TOP PANEL ---
    public void UpdateLevel(int level) { levelText.text = "LEVEL " + level.ToString(); }
    public void UpdateScore(int score) { scoreText.text = "SCORE: " + score.ToString("D6"); }
    public void UpdateLives(int lives) { livesText.text = "x " + lives.ToString(); }

    public void ActivateKey() { keyIcon.sprite = keyColorSprite; }
    public void ActivateLevelPowerUp() { powerUpLevelIcon.sprite = powerUpColorSprite; }

    // --- BOTTOM PANEL ---
    public void UpdateStats(int bombs, int fire)
    {
        maxBombsText.text = bombs.ToString();
        fireRadiusText.text = fire.ToString();
    }

    public void UpdateTimerDisplay(int timeInSeconds)
    {
        int minutes = timeInSeconds / 60;
        int seconds = timeInSeconds % 60;
        timeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
    }
        
    public void ShowIntermission(int level, int lives)
    {
        intermissionPanel.SetActive(true);
        intermissionLevelText.text = "LEVEL " + level.ToString();
        intermissionLivesText.text = "x " + lives.ToString();
    }
        
    public void HideIntermission()
    {
        intermissionPanel.SetActive(false);
    }

    public void AddToInventory(Sprite powerUpSprite)
    {
        if (powerUpSprite == null) return;
        if (currentInventoryIndex < inventoryIcons.Length)
        {
            inventoryIcons[currentInventoryIndex].gameObject.SetActive(true);
            inventoryIcons[currentInventoryIndex].sprite = powerUpSprite;

            inventoryCircles[currentInventoryIndex].gameObject.SetActive(true);

            currentInventoryIndex++;
        }
    }
        
    public List<Sprite> GetCollectedIcons()
    {
        List<Sprite> collected = new List<Sprite>();
        for (int i = 0; i < currentInventoryIndex; i++)
        {
            collected.Add(inventoryIcons[i].sprite);
        }
        return collected;
    }
        
    public void RestoreInventory(List<Sprite> savedIcons)
    {
        // Index reset - double check to ensure we start filling from the first slot
        currentInventoryIndex = 0;
        
        foreach (Sprite icon in savedIcons)
        {
            AddToInventory(icon);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

}