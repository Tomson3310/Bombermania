using UnityEngine;

public abstract class PowerUpData : ScriptableObject
{
    [Header("UI Data")]
    [SerializeField] private string powerUpName;
    [SerializeField] private Sprite uiIcon;
    [SerializeField] private bool isUnique = false;

    [Header("Scoring")]
    [SerializeField] private int pointsValue;

    // Getters (for read-only access)
    public string PowerUpName => powerUpName;
    public Sprite UiIcon => uiIcon;
    public bool IsUnique => isUnique;
    public int PointsValue => pointsValue;

    public abstract void ApplyEffect(PlayerStats stats);
}