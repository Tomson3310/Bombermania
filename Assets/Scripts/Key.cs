using UnityEngine;

public class Key : MonoBehaviour
{
    [Header("Key Settings")]
    public AudioClip keySpawn;
    public AudioClip pickUp;    
    [Range(0f, 1f)][SerializeField] private float spawnVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float pickUpVolume = 1f;

    private void Start()
    {
        if (AudioManager.Instance != null && keySpawn != null)
        {
            AudioManager.Instance.PlaySFX(keySpawn, spawnVolume);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PickUpKey();
            }            
            if (AudioManager.Instance != null && pickUp != null)
            {
                AudioManager.Instance.PlaySFX(pickUp, pickUpVolume);
            }

            Destroy(gameObject);
        }
    }
}