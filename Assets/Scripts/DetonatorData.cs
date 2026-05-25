using UnityEngine;

[CreateAssetMenu(fileName = "NewDetonator", menuName = "Bombermania/PowerUps/Detonator")]
public class DetonatorData: PowerUpData
{
    public override void ApplyEffect(PlayerStats stats)
    {
        stats.EnableDetonator();

        UIManager.Instance.AddToInventory(UiIcon);
        UIManager.Instance.ActivateLevelPowerUp();
    }
}
