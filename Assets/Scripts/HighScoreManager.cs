using System.Collections.Generic;
using UnityEngine;

// 1. Klasa reprezentująca pojedynczy wpis na liście
[System.Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;

    public ScoreEntry(string name, int score)
    {
        this.playerName = name;
        this.score = score;
    }
}

// 2. Klasa opakowująca listę (wymagane, by Unity mogło poprawnie wygenerować JSON)
[System.Serializable]
public class LeaderboardData
{
    public List<ScoreEntry> entries = new List<ScoreEntry>();
}

// 3. Główny zarządca wyników (klasa statyczna)
public static class HighScoreManager
{
    private const string LEADERBOARD_KEY = "Bombermania_Leaderboard";
    private const int MAX_ENTRIES = 10;

    public static LeaderboardData GetLeaderboard()
    {
        LeaderboardData data;

        if (PlayerPrefs.HasKey(LEADERBOARD_KEY))
        {
            string json = PlayerPrefs.GetString(LEADERBOARD_KEY);
            data = JsonUtility.FromJson<LeaderboardData>(json);
        }
        else
        {
            data = new LeaderboardData();
        }

        return FillEmptyEntries(data);
    }

    public static bool IsHighScore(int newScore)
    {
        if (newScore <= 0) return false; // Nie zapisujemy wyników zerowych

        LeaderboardData data = GetLeaderboard();

        // Sprawdzamy, czy nasz wynik jest wyższy od najsłabszego wpisu na liście
        // (lub od pustego wpisu "--- : 0")
        foreach (ScoreEntry entry in data.entries)
        {
            if (newScore > entry.score)
            {
                return true;
            }
        }
        return false;
    }

    public static void AddScore(string playerName, int score)
    {
        LeaderboardData data = GetLeaderboard();

        // Zabezpieczenie przed pustym nickiem
        string finalName = string.IsNullOrEmpty(playerName) ? "AAA" : playerName;

        // Dodajemy nowy wynik do listy
        data.entries.Add(new ScoreEntry(finalName, score));

        // Sortujemy listę malejąco (od najwyższego do najniższego wyniku)
        data.entries.Sort((x, y) => y.score.CompareTo(x.score));

        // Ucinamy listę, jeśli przekracza 10 pozycji
        if (data.entries.Count > MAX_ENTRIES)
        {
            data.entries.RemoveRange(MAX_ENTRIES, data.entries.Count - MAX_ENTRIES);
        }

        // Pakujemy listę z powrotem do JSONa i zapisujemy na dysk
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LEADERBOARD_KEY, json);
        PlayerPrefs.Save();

        Debug.Log($"<color=yellow>[HighScoreManager]</color> Zapisano nowy wynik: {finalName} - {score}");
    }

    // Metoda pomocnicza upewniająca się, że zawsze mamy równe 10 miejsc do wyświetlenia
    private static LeaderboardData FillEmptyEntries(LeaderboardData data)
    {
        if (data.entries == null) data.entries = new List<ScoreEntry>();

        while (data.entries.Count < MAX_ENTRIES)
        {
            data.entries.Add(new ScoreEntry("---", 0));
        }
        return data;
    }
}