using UnityEngine;

[CreateAssetMenu(fileName = "NewExtraLifePowerUp", menuName = "Bombermania/PowerUps/ExtraLife")]
public class ExtraLifePowerUpData : PowerUpData
{
    [Header("Extra Life Settings")]
    [SerializeField] private int livesIncrease = 1;

    public override void ApplyEffect(PlayerStats stats)
    {
        stats.IncreaseLives(livesIncrease);
        UIManager.Instance.ActivateLevelPowerUp();
    }
}
