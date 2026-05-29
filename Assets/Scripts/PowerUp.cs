using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("PowerUp Data File")]
    [SerializeField] private PowerUpData data;

    // Initializing the PowerUp with data from the ScriptableObject
    public void Initialize(PowerUpData newData)
    {
        data = newData;

        
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && data.UiIcon != null)
        {
            spriteRenderer.sprite = data.UiIcon;
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
                
                if (data.IsUnique && GameManager.Instance != null)
                {
                    GameManager.Instance.RemovePowerUpFromPool(data);
                }
                Destroy(gameObject);
            }
        }
    }
}