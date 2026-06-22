using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("PowerUp Data File")]
    [SerializeField] private PowerUpData data;

    [Header("Audio")]
    [SerializeField] private AudioClip spawnSound;
    [Range(0f, 1f)][SerializeField] private float spawnVolume = 0.5f;
    [SerializeField] private AudioClip collectSound;
    [Range(0f, 1f)][SerializeField] private float collectVolume = 0.6f;

    // Initializing the PowerUp with data from the ScriptableObject
    public void Initialize(PowerUpData newData)
    {
        data = newData;

        
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && data.UiIcon != null)
        {
            spriteRenderer.sprite = data.UiIcon;
        }

        if (AudioManager.Instance != null && spawnSound != null)
        {
            AudioManager.Instance.PlaySFX(spawnSound, spawnVolume);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();           

            if (stats != null && data != null)
            {
                data.ApplyEffect(stats);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.score += data.PointsValue;
                    UIManager.Instance.UpdateScore(GameManager.Instance.score);
                }
                
                if (data.IsUnique && GameManager.Instance != null)
                {
                    GameManager.Instance.RemovePowerUpFromPool(data);
                }

                if (AudioManager.Instance != null && collectSound != null)
                {
                    AudioManager.Instance.PlaySFX(collectSound, collectVolume);
                }

                Destroy(gameObject);
            }
        }
    }
}