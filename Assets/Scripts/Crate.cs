using UnityEngine;

public class Crate : MonoBehaviour
{
    [Header("Hidden Item (Assigned by Generator)")]
    public GameObject hiddenItemPrefab;
    public PowerUpData powerUpData;

    private bool isDestroyed = false;

    public void DestroyCrate()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (hiddenItemPrefab != null)
        {
            GameObject spawnedItem = Instantiate(hiddenItemPrefab, transform.position, Quaternion.identity);

            // Szukamy Twojego skryptu PowerUp
            PowerUp powerUpScript = spawnedItem.GetComponent<PowerUp>();

            // Jeśli to Power-Up, wstrzykujemy mu tożsamość
            if (powerUpScript != null && powerUpData != null)
            {
                powerUpScript.Initialize(powerUpData);
            }
        }

        Destroy(gameObject);
    }
}