using UnityEngine;

public abstract class PowerUpData : ScriptableObject
{
    [Header("UI Data")]
    [SerializeField] private string powerUpName;
    [SerializeField] private Sprite uiIcon;
    [SerializeField] private bool isUnique = false; // If true, player can only have one of this type at a time

    // Getters (for read-only access)
    public string PowerUpName => powerUpName;
    public Sprite UiIcon => uiIcon;
    public bool IsUnique => isUnique;

    public abstract void ApplyEffect(PlayerStats stats);
}