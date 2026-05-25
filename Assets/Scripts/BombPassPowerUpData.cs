using UnityEngine;

[CreateAssetMenu(fileName = "NewBombPassPowerUp", menuName = "Bombermania/PowerUps/BombPass")]
public class BombPassPowerUpData : PowerUpData
{
    public override void ApplyEffect(PlayerStats stats)
    {
        stats.EnableBombPass();
        
        UIManager.Instance.AddToInventory(UiIcon);
        UIManager.Instance.ActivateLevelPowerUp();
    }
}
