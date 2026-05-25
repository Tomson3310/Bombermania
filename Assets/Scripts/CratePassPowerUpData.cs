using UnityEngine;

[CreateAssetMenu(fileName = "NewCratePassPowerUp", menuName = "Bombermania/PowerUps/CratePass")]
public class CratePassPowerUpData : PowerUpData
{
    public override void ApplyEffect(PlayerStats stats)
    {
        stats.EnableCratePass();

        UIManager.Instance.AddToInventory(uiIcon);
        UIManager.Instance.ActivateLevelPowerUp();
    }
}
