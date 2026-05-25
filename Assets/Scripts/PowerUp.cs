using UnityEngine;

public class PowerUp : MonoBehaviour
{
    
    public enum PowerUpType
    {
        FireRange,
        ExtraBomb,
        Detonator,
        SpeedMove,
        CratePass,
        BombPass,
        ExtraLife
    }

    [Header("PowerUp Settings")]
    [SerializeField] private PowerUpType type;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();

            if (stats != null)
            {
                switch (type)
                {
                    case PowerUpType.FireRange:
                        stats.IncreaseFireRadius();
                        break;

                    case PowerUpType.ExtraBomb:
                        stats.IncreaseMaxBombs();
                        break;

                    case PowerUpType.Detonator:
                        stats.EnableDetonator();
                        break;
                    case PowerUpType.SpeedMove:
                        stats.IncreasePlayerSpeed();
                        break;
                    case PowerUpType.CratePass:
                        stats.EnableCratePass();
                        break;
                    case PowerUpType.BombPass:
                        stats.EnableBombPass();
                        break;
                    case PowerUpType.ExtraLife:
                        stats.IncreaseLives();
                        break;
                }

                Destroy(gameObject);
            }
        }
    }
}