using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject leaderboardPanel;
        
    [SerializeField] private TMP_Text[] nameTexts;
        
    [SerializeField] private TMP_Text[] scoreTexts;

    private void OnEnable()
    {        
        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        // Load the latest leaderboard data
        LeaderboardData data = HighScoreManager.GetLeaderboard();

        for (int i = 0; i < 10; i++)
        {
            if (i < nameTexts.Length && i < scoreTexts.Length)
            {
                if (nameTexts[i] != null && scoreTexts[i] != null)
                {
                    nameTexts[i].text = data.entries[i].playerName;

                    // "D6" adds leading zeros (e.g., 150 -> 000150)
                    scoreTexts[i].text = data.entries[i].score.ToString("D6");
                }
            }
        }
    }

    public void OpenLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
    }

    public void CloseLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }
}