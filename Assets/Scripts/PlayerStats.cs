using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int lives = 3;
    [SerializeField] private int fireRadius = 1;
    [SerializeField] private int maxBombs = 1;
    [SerializeField] private bool hasDetonator = false;
    [SerializeField] private bool hasCratePass = false;
    [SerializeField] private bool hasBombPass = false;
    [SerializeField] private float playerMoveSpeed = 5f;
    [SerializeField] private float speedBoost = 1.25f;

    [Header("PowerUp UI Graphics")]
    public Sprite detonatorSprite;
    public Sprite speedBoostSprite;
    public Sprite cratePassSprite;
    public Sprite bombPassSprite;

    public int FireRadius => fireRadius;
    public int MaxBombs => maxBombs;
    public bool HasDetonator => hasDetonator;
    public float PlayerMoveSpeed => playerMoveSpeed;
    public bool HasCratePass => hasCratePass;
    public bool HasBombPass => hasBombPass;

    private void Start()
    {
        // Reset collision settings in case player had crate/bomb pass from previous level
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Crate"), false);
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Bomb"), false);

        UIManager.Instance.UpdateLives(lives);
        UIManager.Instance.UpdateStats(maxBombs, fireRadius);
    }

    public void LoseLife()
    {
        lives--;
        UIManager.Instance.UpdateLives(lives);
        if (lives <= 0) { Debug.Log("Game Over!"); }
    }

    public void IncreaseFireRadius()
    {
        fireRadius++;
        UIManager.Instance.UpdateStats(maxBombs, fireRadius);

        UIManager.Instance.ActivateLevelPowerUp();
    }

    public void IncreaseMaxBombs()
    {
        maxBombs++;
        UIManager.Instance.UpdateStats(maxBombs, fireRadius);

        UIManager.Instance.ActivateLevelPowerUp();
    }

    public void EnableDetonator()
    {
        hasDetonator = true;
        UIManager.Instance.AddToInventory(detonatorSprite);
        UIManager.Instance.ActivateLevelPowerUp();
    }

    public void IncreasePlayerSpeed()
    {
        playerMoveSpeed *= speedBoost;
        UIManager.Instance.AddToInventory(speedBoostSprite);
        UIManager.Instance.ActivateLevelPowerUp();
    }

    public void EnableCratePass()
    {
        // This method would set a flag to allow the player to pass through crates.
        hasCratePass = true;

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Crate"), true);

        UIManager.Instance.AddToInventory(cratePassSprite);
        UIManager.Instance.ActivateLevelPowerUp();
    }

    public void EnableBombPass()
    {
        // This method would set a flag to allow the player to pass through bombs.
        hasBombPass = true;
        
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Bomb"), true);
        
        UIManager.Instance.AddToInventory(bombPassSprite);
        UIManager.Instance.ActivateLevelPowerUp();
    }

    public void IncreaseLives()
    {
        lives++;
        UIManager.Instance.UpdateLives(lives);
    }
}