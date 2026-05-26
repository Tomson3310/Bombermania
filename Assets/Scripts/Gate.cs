using UnityEngine;

public class Gate : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite openedGateSprite;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (GameManager.Instance != null && GameManager.Instance.hasKey)
        {
            OpenGate();
        }
    }

    public void OpenGate()
    {
        if (spriteRenderer != null && openedGateSprite != null)
        {
            spriteRenderer.sprite = openedGateSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null && GameManager.Instance.hasKey)
            {
                Debug.Log("LEVEL COMPLETED! Player passed through the open gate!");
                GameManager.Instance.LoadNextLevel();
            }
            else
            {
                Debug.Log("Brama jest zamknięta. Musisz najpierw znaleźć klucz!");
            }
        }
    }
}