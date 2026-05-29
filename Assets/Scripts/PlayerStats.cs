using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int lives = 3;
    [SerializeField] private int fireRange = 1;
    [SerializeField] private int maxBombs = 1;
    [SerializeField] private bool hasDetonator = false;
    [SerializeField] private bool hasCratePass = false;
    [SerializeField] private bool hasBombPass = false;
    [SerializeField] private float playerMoveSpeed = 5f;

    // Public getters for stats
    public int Lives => lives;
    public int FireRange => fireRange;
    public int MaxBombs => maxBombs;
    public bool HasDetonator => hasDetonator;
    public float PlayerMoveSpeed => playerMoveSpeed;
    public bool HasCratePass => hasCratePass;
    public bool HasBombPass => hasBombPass;

    private void Start()
    {

        Debug.Log("<color=green>[PlayerStats]</color> Start gracza. Inicjalizacja statystyk...");
        // Logic for loading saved stats from GameManager, if available
        if (GameManager.Instance != null && GameManager.Instance.hasSavedSession)
        {
            lives = GameManager.Instance.savedLives;
            fireRange = GameManager.Instance.savedFireRange;
            maxBombs = GameManager.Instance.savedMaxBombs;
            hasDetonator = GameManager.Instance.savedHasDetonator;
            playerMoveSpeed = GameManager.Instance.savedPlayerMoveSpeed;

            hasCratePass = GameManager.Instance.savedHasCratePass;
            hasBombPass = GameManager.Instance.savedHasBombPass;
            Debug.Log($"<color=green>[PlayerStats]</color> Załadowano sejf: Życia={lives}, Prędkość={playerMoveSpeed}, BombPass={hasBombPass}");
        }
        else
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.basePlayerSpeed = this.playerMoveSpeed;
            }
            Debug.Log("<color=green>[PlayerStats]</color> Nowa gra. Używam domyślnych wartości z prefabu.");
        }

        // Very important: Set layer collision rules based on loaded stats
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Crate"), hasCratePass);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Bomb"), hasBombPass);

        // UI Update
        UIManager.Instance.UpdateLives(lives);
        UIManager.Instance.UpdateStats(maxBombs, fireRange);
    }

    public void LoseLife()
    {
        lives--;
        UIManager.Instance.UpdateLives(lives);
        Debug.Log($"<color=green>[PlayerStats]</color> UTRATA ŻYCIA! Pozostało żyć: {lives}");

        if (lives > 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetCurrentLevel();
            }
        }
        else
        {
            Debug.Log("<color=red>[PlayerStats]</color> GAME OVER - Brak żyć!");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    public void IncreaseFireRadius(int amount)
    {
        fireRange += amount;
    }

    public void IncreaseMaxBombs(int amount)
    {
        maxBombs += amount;
    }

    public void EnableDetonator()
    {
        hasDetonator = true;
    }

    public void IncreasePlayerSpeed(float multiplier)
    {
        playerMoveSpeed *= multiplier;
    }

    public void EnableCratePass()
    {        
        hasCratePass = true;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Crate"), true);
    }

    public void EnableBombPass()
    {
        hasBombPass = true;        
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Bomb"), true);
    }

    public void IncreaseLives(int amount)
    {
        lives += amount;
        UIManager.Instance.UpdateLives(lives);
    }
}