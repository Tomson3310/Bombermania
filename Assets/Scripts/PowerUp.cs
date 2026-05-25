using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("PowerUp Data File")]
    [SerializeField] private PowerUpData data;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats stats = other.GetComponent<PlayerStats>();

            if (stats != null && data != null)
            {                
                data.ApplyEffect(stats);

                Destroy(gameObject);
            }
        }
    }
}