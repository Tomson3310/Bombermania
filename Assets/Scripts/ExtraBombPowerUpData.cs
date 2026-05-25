using UnityEngine;

[CreateAssetMenu(fileName = "NewExtraBombPowerUp", menuName = "Bombermania/PowerUps/ExtraBomb")]
public class ExtraBombPowerUpData: PowerUpData

{
    [Header("Extra Bomb Power-Up Settings")]
    [SerializeField] private int maxBombsIncrease = 1;
    public override void ApplyEffect(PlayerStats stats)
    {
        stats.IncreaseMaxBombs(maxBombsIncrease);
        // Aktualizujemy UI, aby pokazać nowe statystyki
        UIManager.Instance.UpdateStats(stats.MaxBombs, stats.FireRange);
        UIManager.Instance.ActivateLevelPowerUp();
    }
}
