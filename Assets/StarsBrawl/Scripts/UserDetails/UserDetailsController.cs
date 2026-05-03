using System.Data;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserDetailsController : MonoBehaviour
{
    [Header("Identificación")]
    [SerializeField] private TMP_Text userIdText;
    [SerializeField] private TMP_Text nicknameText;

    [Header("Rango y Trofeos")]
    [SerializeField] private Image rankIcon;
    [SerializeField] private TMP_Text rankNameText;
    [SerializeField] private TMP_Text totalTrophiesText;

    [Header("Estadísticas de Victorias")]
    [SerializeField] private TMP_Text survivalWinsText;
    [SerializeField] private TMP_Text tripleWinsText;
    [SerializeField] private TMP_Text winStreakText;

    [Header("Récords")]
    [SerializeField] private TMP_Text mostPlayedBrawlerText;
    [SerializeField] private TMP_Text bestWinRateBrawlerText;
    [SerializeField] private TMP_Text mostPlayedModeText;

    [Header("Referencias")]
    [SerializeField] private RankIconProvider rankIconProvider;

    private async void Start()
    {
        string userId = UserSession.LoggedUserId;
        if (string.IsNullOrEmpty(userId)) return;

        await LoadFullUserProfile(userId);
    }

    private async Task LoadFullUserProfile(string userId)
    {
        string trophiesQuery = $"SELECT SUM(trophies) as total FROM user_brawlers WHERE user_id = '{userId}'";
        DataTable dtTrophies = await DatabaseManager.Instance.ExecuteQueryAsync(trophiesQuery);

        int totalTrophies = 0;
        if (dtTrophies?.Rows.Count > 0 && dtTrophies.Rows[0]["total"] != System.DBNull.Value)
            totalTrophies = System.Convert.ToInt32(dtTrophies.Rows[0]["total"]);

        string mainQuery = $@"
            SELECT 
                u.user_id, u.nickname,
                (SELECT r.name FROM ranks r WHERE {totalTrophies} BETWEEN r.min_trophies AND r.max_trophies LIMIT 1) as rank_name,
                (SELECT r.rank_id FROM ranks r WHERE {totalTrophies} BETWEEN r.min_trophies AND r.max_trophies LIMIT 1) as rank_id,

                (SELECT COUNT(*) 
                 FROM match_user mu
                 JOIN matches m ON mu.match_id = m.match_id
                 JOIN match_types mt ON m.match_type_id = mt.match_type_id
                 WHERE mu.user_id = u.user_id 
                 AND mu.result = 'Victoria' 
                 AND mt.modality = 'Arena de supervivencia') as survival_wins,

                (SELECT COUNT(*) 
                 FROM match_user mu
                 JOIN matches m ON mu.match_id = m.match_id
                 JOIN match_types mt ON m.match_type_id = mt.match_type_id
                 WHERE mu.user_id = u.user_id 
                 AND mu.result = 'Victoria' 
                 AND mt.modality = '3 contra 3') as triple_wins

            FROM users u 
            WHERE u.user_id = '{userId}'";

        DataTable dt = await DatabaseManager.Instance.ExecuteQueryAsync(mainQuery);

        if (dt != null && dt.Rows.Count > 0)
        {
            DataRow r = dt.Rows[0];

            userIdText.text = r["user_id"].ToString();
            nicknameText.text = r["nickname"].ToString();
            totalTrophiesText.text = totalTrophies.ToString();

            rankNameText.text = r["rank_name"] != System.DBNull.Value ? r["rank_name"].ToString() : "Bronce I";
            survivalWinsText.text = r["survival_wins"].ToString();
            tripleWinsText.text = r["triple_wins"].ToString();

            if (r["rank_id"] != System.DBNull.Value && rankIconProvider != null)
            {
                int rId = System.Convert.ToInt32(r["rank_id"]);
                if (rankIconProvider.TryGetRankIconByID(rId, out Sprite s))
                    rankIcon.sprite = s;
            }

            await CalculateWinStreak(userId);
        }

        await LoadRecords(userId);
    }

    private async Task CalculateWinStreak(string userId)
    {
        string streakQuery = $"SELECT TRIM(result) as result FROM match_user WHERE user_id = '{userId}' ORDER BY match_id DESC";
        DataTable dt = await DatabaseManager.Instance.ExecuteQueryAsync(streakQuery);

        int streak = 0;

        if (dt != null)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (row["result"].ToString() == "Victoria") streak++;
                else break;
            }
        }

        winStreakText.text = streak.ToString();
    }

    private async Task LoadRecords(string userId)
    {
        string bMostPlayed = $@"
            SELECT b.name 
            FROM match_user mu 
            JOIN brawlers b ON mu.brawler_id = b.brawler_id 
            WHERE mu.user_id = '{userId}' 
            GROUP BY mu.brawler_id 
            ORDER BY COUNT(*) DESC 
            LIMIT 1";

        string modeMostPlayed = $@"
            SELECT mt.name 
            FROM match_user mu 
            JOIN matches m ON mu.match_id = m.match_id
            JOIN match_types mt ON m.match_type_id = mt.match_type_id
            WHERE mu.user_id = '{userId}' 
            GROUP BY mt.match_type_id 
            ORDER BY COUNT(*) DESC 
            LIMIT 1";

        DataTable dt1 = await DatabaseManager.Instance.ExecuteQueryAsync(bMostPlayed);
        DataTable dt3 = await DatabaseManager.Instance.ExecuteQueryAsync(modeMostPlayed);

        mostPlayedBrawlerText.text = (dt1?.Rows.Count > 0) ? dt1.Rows[0]["name"].ToString() : "---";
        mostPlayedModeText.text = (dt3?.Rows.Count > 0) ? dt3.Rows[0]["name"].ToString() : "---";

        bestWinRateBrawlerText.text = mostPlayedBrawlerText.text;
    }

    public void GoToMainMenu() => SceneManager.LoadScene("MainMenu");
}