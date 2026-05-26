using UnityEngine;

[CreateAssetMenu(fileName = "NewSpeedPowerUp", menuName = "Bombermania/PowerUps/Speed")]
public class SpeedPowerUpData : PowerUpData
{
    [Header("Speed Settings")]
    [SerializeField] private float speedMultiplier = 1.5f;

    
    public override void ApplyEffect(PlayerStats stats)
    {
        
        stats.IncreasePlayerSpeed(speedMultiplier);
        
        UIManager.Instance.AddToInventory(UiIcon);
        UIManager.Instance.ActivateLevelPowerUp();
    }
}