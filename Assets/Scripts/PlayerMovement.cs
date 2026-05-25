using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private Vector2 primaryDirection;
    private Vector2 secondaryDirection;
    private Vector2 lastRawInput;

    private bool wasBlockedX;
    private bool wasBlockedY;

    private PlayerControls controls;
    private PlayerStats playerStats;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();
        playerStats = GetComponent<PlayerStats>();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        Vector2 rawInput = controls.Player.Move.ReadValue<Vector2>();

        // normalize input (with deadzone)
        Vector2 input = new Vector2(
            Mathf.Abs(rawInput.x) > 0.1f ? Mathf.Sign(rawInput.x) : 0,
            Mathf.Abs(rawInput.y) > 0.1f ? Mathf.Sign(rawInput.y) : 0
        );

        Vector2 dirX = new Vector2(input.x, 0);
        Vector2 dirY = new Vector2(0, input.y);

        bool isBlockedX = input.x != 0 && IsDirectionBlocked(dirX);
        bool isBlockedY = input.y != 0 && IsDirectionBlocked(dirY);

        // Prioritize gap that just opened (gap seeking)
        if (input.x != 0 && wasBlockedX && !isBlockedX)
        {
            primaryDirection = dirX;
            secondaryDirection = dirY;
        }

        else if (input.y != 0 && wasBlockedY && !isBlockedY)
        {
            primaryDirection = dirY;
            secondaryDirection = dirX;
        }
        // Prioritize most recently pressed direction
        else if (input != lastRawInput)
        {
            if (input.x != 0 && lastRawInput.x == 0)
            {
                primaryDirection = dirX;
                secondaryDirection = dirY;
            }
            else if (input.y != 0 && lastRawInput.y == 0)
            {
                primaryDirection = dirY;
                secondaryDirection = dirX;
            }
            else if (input == Vector2.zero)
            {
                primaryDirection = Vector2.zero;
                secondaryDirection = Vector2.zero;
            }
            else
            {
                if (input.x != 0) { primaryDirection = dirX; secondaryDirection = Vector2.zero; }
                else if (input.y != 0) { primaryDirection = dirY; secondaryDirection = Vector2.zero; }
            }
        }
        // setting for next frame
        lastRawInput = input;
        wasBlockedX = isBlockedX;
        wasBlockedY = isBlockedY;

        movementInput = primaryDirection;

        if (primaryDirection != Vector2.zero && secondaryDirection != Vector2.zero)
        {
            if (IsDirectionBlocked(primaryDirection))
            {
                if (!IsDirectionBlocked(secondaryDirection))
                {
                    movementInput = secondaryDirection;
                }
                else
                {
                    movementInput = Vector2.zero;
                }
            }
        }
    }

    private bool IsDirectionBlocked(Vector2 direction)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 0.15f, direction, 0.4f, obstacleLayer);

        foreach (RaycastHit2D hit in hits)
        {
            // Skip the player itself
            if (hit.collider.gameObject == gameObject) continue;

            // Skip bombs the player is standing on and if player has bomb pass power-up
            if (hit.collider.CompareTag("Bomb"))
            {
                if (playerStats.HasBombPass) continue;

                Collider2D playerCollider = GetComponent<Collider2D>();

                if (playerCollider != null && hit.collider.bounds.Intersects(playerCollider.bounds))
                {
                    continue;
                }
            }

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Crate"))
            {
                // Ignore crates if player has crate pass power-up
                if (playerStats.HasCratePass) continue;
            }

            return true;
        }

        return false;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movementInput * playerStats.PlayerMoveSpeed;
    }
    public void Die()
    {
        Debug.Log("Player died! GAME OVER");
        Destroy(gameObject);
    }
}