using System.Data;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchDetailsController : MonoBehaviour
{
    [Header("Ganadores")]
    [SerializeField] private Transform winnersContainer;
    [SerializeField] private MatchPlayerItem playerPrefab;

    [Header("Perdedores")]
    [SerializeField] private Transform losersContainer;

    [Header("Referencias")]
    [SerializeField] private PortraitProvider portraitProvider;

    private async void Start()
    {
        int matchId = UserSession.SelectedMatchId;
        if (matchId == 0) return;

        await LoadMatchDetails(matchId);
    }

    private async Task LoadMatchDetails(int matchId)
    {
        foreach (Transform t in winnersContainer) Destroy(t.gameObject);
        foreach (Transform t in losersContainer) Destroy(t.gameObject);

        string query = $@"
            SELECT 
                mu.result,
                u.nickname,
                ub.trophies,
                ub.level,
                b.brawler_id
            FROM match_user mu
            JOIN users u ON mu.user_id = u.user_id
            JOIN brawlers b ON mu.brawler_id = b.brawler_id
            JOIN user_brawlers ub 
                ON ub.user_id = mu.user_id 
                AND ub.brawler_id = mu.brawler_id
            WHERE mu.match_id = {matchId}";

        DataTable dt = await DatabaseManager.Instance.ExecuteQueryAsync(query);

        if (dt == null || dt.Rows.Count == 0)
        {
            Debug.LogWarning("No hay datos de la partida.");
            return;
        }

        foreach (DataRow row in dt.Rows)
        {
            string result = row["result"].ToString();
            string nickname = row["nickname"].ToString();
            int trophies = System.Convert.ToInt32(row["trophies"]);
            int level = System.Convert.ToInt32(row["level"]);
            int brawlerId = System.Convert.ToInt32(row["brawler_id"]);

            Sprite portrait = null;
            if (portraitProvider != null)
                portraitProvider.TryGetPortraitByID(brawlerId, out portrait);

            MatchPlayerItem item = Instantiate(playerPrefab);

            item.Setup(nickname, trophies, level, portrait);

            if (result == "Victoria")
                item.transform.SetParent(winnersContainer, false);
            else
                item.transform.SetParent(losersContainer, false);
        }
    }

    public void GoBack() => SceneManager.LoadScene("BattleLog");
}