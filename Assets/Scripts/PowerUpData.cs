using UnityEngine;

// Słowo "abstract" oznacza, że to tylko szablon. Nie stworzymy pliku z samym "PowerUpData", 
// musi to być zawsze jakiś konkretny PowerUp (np. Speed, Fire).
public abstract class PowerUpData : ScriptableObject
{
    [Header("UI Data")]
    public string powerUpName; // Przydatne do debugowania
    public Sprite uiIcon;      // Każdy PowerUp sam wie, jak wygląda w ekwipunku!

    // To jest najważniejsza część. Deklarujemy, że każdy power-up MUSI mieć funkcję ApplyEffect.
    // Ale nie piszemy tu kodu. Każdy specyficzny power-up napisze ten kod sam dla siebie.
    public abstract void ApplyEffect(PlayerStats stats);
}