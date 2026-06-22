using System.Collections.Generic;
using UnityEngine;

// class representing a single score entry
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

// class representing the leaderboard data
[System.Serializable]
public class LeaderboardData
{
    public List<ScoreEntry> entries = new List<ScoreEntry>();
}

// class representing the high score manager
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
        if (newScore <= 0) return false;

        LeaderboardData data = GetLeaderboard();

        // check if the new score is higher than any of the existing scores
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

        // input validation: if the player name is empty, we use "AAA" as a default
        string finalName = string.IsNullOrEmpty(playerName) ? "AAA" : playerName;

        // add the new score entry to the list
        data.entries.Add(new ScoreEntry(finalName, score));

        // sorting the list in descending order based on score
        data.entries.Sort((x, y) => y.score.CompareTo(x.score));

        // cutting the list to the maximum number of entries if necessary
        if (data.entries.Count > MAX_ENTRIES)
        {
            data.entries.RemoveRange(MAX_ENTRIES, data.entries.Count - MAX_ENTRIES);
        }

        // packing the data back into JSON and saving it to PlayerPrefs
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LEADERBOARD_KEY, json);
        PlayerPrefs.Save();

        Debug.Log($"<color=yellow>[HighScoreManager]</color> Zapisano nowy wynik: {finalName} - {score}");
    }

    // Method helper ensuring we always have exactly 10 places to display
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