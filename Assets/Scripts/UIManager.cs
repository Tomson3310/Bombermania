using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Top Panel")]
    public TMP_Text levelText;
    public TMP_Text scoreText;
    public TMP_Text livesText;
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

    public void AddToInventory(Sprite powerUpSprite)
    {
        if (currentInventoryIndex < inventoryIcons.Length)
        {
            inventoryIcons[currentInventoryIndex].gameObject.SetActive(true);
            inventoryIcons[currentInventoryIndex].sprite = powerUpSprite;

            inventoryCircles[currentInventoryIndex].gameObject.SetActive(true);

            currentInventoryIndex++;
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