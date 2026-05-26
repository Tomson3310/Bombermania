using UnityEngine;

public class CameraController : MonoBehaviour
{
    private LevelData currentLevel;

    [Header("Camera Settings")]    
    [SerializeField] private float cameraZoom = 7f;    
    [SerializeField] private float yOffset = 0f;

    private Transform playerTransform;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;        
        cam.orthographicSize = cameraZoom;

        if (GameManager.Instance != null)
        {
            currentLevel = GameManager.Instance.GetCurrentLevelData();
        }
    }

    private void LateUpdate()
    {
        // Find the player
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else return;
        }

        if (currentLevel == null) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // Y axis - always centered on the level, with optional offset
        float cameraY = (currentLevel.Height / 2f) + yOffset;


        // X axis - depends on level width and player position
        float cameraX;

        
        if (currentLevel.Width <= halfWidth * 2)
        {
            // Lock camera to center of level if it's narrower than the camera's view
            cameraX = currentLevel.Width / 2f;
        }
        else
        {
            // If the level is wider than the camera's view, follow the player but clamp to level bounds
            cameraX = Mathf.Clamp(playerTransform.position.x, halfWidth, currentLevel.Width - halfWidth);
        }

        
        transform.position = new Vector3(cameraX, cameraY, transform.position.z);
    }
}