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

    public int FireRange => fireRange;
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
        UIManager.Instance.UpdateStats(maxBombs, fireRange);
    }

    public void LoseLife()
    {
        lives--;
        UIManager.Instance.UpdateLives(lives);
        if (lives <= 0) { Debug.Log("Game Over!"); }
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