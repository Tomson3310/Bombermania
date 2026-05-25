using UnityEngine;

// Ten atrybut sprawia, że będziesz mógł kliknąć prawym przyciskiem myszy 
// w Unity i z menu wybrać opcję stworzenia tego pliku!
[CreateAssetMenu(fileName = "NewSpeedPowerUp", menuName = "Bombermania/PowerUps/Speed")]
public class SpeedPowerUpData : PowerUpData // Zauważ, że dziedziczy po PowerUpData!
{
    [Header("Speed Settings")]
    [SerializeField] private float speedMultiplier = 1.25f; // Każdy SO może mieć własne zmienne!

    // Nadpisujemy funkcję z szablonu. To tutaj dzieje się magia.
    public override void ApplyEffect(PlayerStats stats)
    {
        // 1. Zwiększamy statystyki
        stats.IncreasePlayerSpeed(speedMultiplier);

        // 2. Mówimy UI, żeby dodało NASZĄ grafikę do ekwipunku
        UIManager.Instance.AddToInventory(uiIcon);
        UIManager.Instance.ActivateLevelPowerUp();
    }
}