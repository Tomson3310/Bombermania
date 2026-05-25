using UnityEngine;

[CreateAssetMenu(fileName = "NewFireRangePowerUp", menuName = "Bombermania/PowerUps/FireRange")]
public class FireRadiusPowerUpData : PowerUpData
{
    [Header("Fire Radius Settings")]
    [SerializeField] private int radiusIncrease = 1;

    public override void ApplyEffect(PlayerStats stats)
    {
        stats.IncreaseFireRadius(radiusIncrease);

        UIManager.Instance.UpdateStats(stats.MaxBombs, stats.FireRange);
        UIManager.Instance.ActivateLevelPowerUp();
    }
}
