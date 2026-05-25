using UnityEngine;

public abstract class PowerUpData : ScriptableObject
{
    [Header("UI Data")]
    [SerializeField] private string powerUpName;
    [SerializeField] private Sprite uiIcon;

    // Getters (for read-only access)
    public string PowerUpName => powerUpName;
    public Sprite UiIcon => uiIcon;

    public abstract void ApplyEffect(PlayerStats stats);
}