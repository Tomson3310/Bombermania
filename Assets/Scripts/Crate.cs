using UnityEngine;
using System.Collections; // Wymagane dla Coroutine

public class Crate : MonoBehaviour
{
    [Header("Hidden Item (Assigned by Generator)")]
    public GameObject hiddenItemPrefab;
    public PowerUpData powerUpData;

    [Header("Animation")]
    public Animator animator;

    private bool isDestroyed = false;

    private void Update()
    {        
        if (isDestroyed)
        {
            CheckAndKillEntitiesInside();
        }
    }

    private void CheckAndKillEntitiesInside()
    {
        // Use an OvelapBox with a smaller size
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, new Vector2(0.8f, 0.8f), 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerMovement player = hit.GetComponent<PlayerMovement>();
                if (player != null)
                {                    
                    player.Die(DeathType.Burn);
                }
            }
            else if (hit.CompareTag("Enemy"))
            {
                EnemyAI enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.Die();
                }
            }
        }
    }

    public void DestroyCrate()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Burning animation trigger
        if (animator != null)
        {            
            animator.Play("Crate_Burn", -1, 0f);
        }
        
        StartCoroutine(BurnCoroutine());
    }

    private IEnumerator BurnCoroutine()
    {
        // CRITICAL: Wait for the next frame to ensure the animation state is updated before we check its length
        yield return null;

        float waitTime = 0.5f; // Default value (just in case)

        if (animator != null)
        {
            // Get the current state info of the animator to determine the length of the animation
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);            
            waitTime = stateInfo.length;
        }
        
        yield return new WaitForSeconds(waitTime);

        // After the animation is complete
        if (hiddenItemPrefab != null)
        {
            GameObject spawnedItem = Instantiate(hiddenItemPrefab, transform.position, Quaternion.identity);

            PowerUp powerUpScript = spawnedItem.GetComponent<PowerUp>();

            if (powerUpScript != null && powerUpData != null)
            {
                powerUpScript.Initialize(powerUpData);
            }
        }

        Destroy(gameObject);
    }
}