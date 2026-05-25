using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float lifetime = 0.5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy != null) enemy.Die();
        }
        else if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null) player.Die();
        }
        else if (other.CompareTag("Bomb"))
        {
            Bomb bomb = other.GetComponent<Bomb>();
            if (bomb != null)
            {
                bomb.ForceExplode();
            }
        }
    }
}