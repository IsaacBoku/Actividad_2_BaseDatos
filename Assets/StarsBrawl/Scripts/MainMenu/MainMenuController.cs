using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Data;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Datos del Usuario")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text total_trophiesText;
    [SerializeField] private TMP_Text blingText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text gemsText;

    [Header("Último Brawler Usado")]
    [SerializeField] private Image brawlerPortrait;
    [SerializeField] private TMP_Text brawlerTrophiesText;
    [SerializeField] private TMP_Text brawlerLevelText;

    [Header("Referencias")]
    [SerializeField] private PortraitProvider portraitProvider;

    private async void Start()
    {
        string userId = UserSession.LoggedUserId;
        if (string.IsNullOrEmpty(userId)) return;

        await LoadMainMenuData(userId);
    }

    private async System.Threading.Tasks.Task LoadMainMenuData(string userId)
    {
        string userQuery = $@"
        SELECT u.nickname, u.bling, u.coins, u.gems, 
        (SELECT COALESCE(SUM(trophies), 0) FROM user_brawlers WHERE user_id = u.user_id) AS total_trophies
        FROM users u WHERE u.user_id = '{userId}'";

        DataTable userData = await DatabaseManager.Instance.ExecuteQueryAsync(userQuery);

        if (userData != null && userData.Rows.Count > 0)
        {
            DataRow row = userData.Rows[0];
            nicknameText.text = row["nickname"].ToString();
            total_trophiesText.text = row["total_trophies"].ToString();
            blingText.text = row["bling"].ToString();
            coinsText.text = row["coins"].ToString();
            gemsText.text = row["gems"].ToString();
        }

        string brawlerQuery = $@"
        SELECT ub.brawler_id, ub.trophies, ub.level 
        FROM user_brawlers ub
        JOIN match_user mu ON ub.brawler_id = mu.brawler_id AND ub.user_id = mu.user_id
        WHERE ub.user_id = '{userId}' 
        ORDER BY mu.match_id DESC 
        LIMIT 1";

        DataTable brawlerData = await DatabaseManager.Instance.ExecuteQueryAsync(brawlerQuery);

        if (brawlerData != null && brawlerData.Rows.Count > 0)
        {
            DataRow bRow = brawlerData.Rows[0];
            brawlerTrophiesText.text = bRow["trophies"].ToString();
            brawlerLevelText.text = bRow["level"].ToString();

            // Convertir ID de forma segura
            if (int.TryParse(bRow["brawler_id"].ToString(), out int bId))
            {
                if (portraitProvider != null && portraitProvider.TryGetPortraitByID(bId, out Sprite sprite))
                {
                    brawlerPortrait.sprite = sprite;
                }
            }
        }
        else
        {
            Debug.LogWarning("El usuario no tiene brawlers registrados.");
        }
    }

    public void GoToUserDetails() => SceneManager.LoadScene("UserDetails");
    public void GoToBrawlers() => SceneManager.LoadScene("Brawlers");
    public void GoToBattleLog() => SceneManager.LoadScene("BattleLog");
}