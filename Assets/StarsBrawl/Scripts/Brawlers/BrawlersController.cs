using System.Data;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BrawlersController : MonoBehaviour
{
    [Header("UI - Cabecera (i)")]
    [SerializeField] private TMP_Text brawlersCountText;

    [Header("UI - Lista (ii)")]
    [SerializeField] private Transform brawlersContainer;
    [SerializeField] private BrawlerUIItem brawlerPrefab; 

    [Header("Referencias")]
    [SerializeField] private PortraitProvider portraitProvider;

    private async void Start()
    {
        string userId = UserSession.LoggedUserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("No hay usuario logueado.");
            return;
        }

        await LoadBrawlersData(userId);
    }

    private async Task LoadBrawlersData(string userId)
    {
        foreach (Transform child in brawlersContainer)
        {
            Destroy(child.gameObject);
        }

        string countQuery = $@"
            SELECT 
                (SELECT COUNT(*) FROM user_brawlers WHERE user_id = '{userId}') as unlocked_count,
                (SELECT COUNT(*) FROM brawlers) as total_count";

        DataTable countDt = await DatabaseManager.Instance.ExecuteQueryAsync(countQuery);
        if (countDt != null && countDt.Rows.Count > 0)
        {
            int unlocked = System.Convert.ToInt32(countDt.Rows[0]["unlocked_count"]);
            int total = System.Convert.ToInt32(countDt.Rows[0]["total_count"]);
            brawlersCountText.text = $"{unlocked}/{total}";
        }

        string listQuery = $@"
            SELECT b.brawler_id, b.name, ub.level, ub.trophies
            FROM user_brawlers ub
            JOIN brawlers b ON ub.brawler_id = b.brawler_id
            WHERE ub.user_id = '{userId}'
            ORDER BY ub.trophies DESC";

        DataTable listDt = await DatabaseManager.Instance.ExecuteQueryAsync(listQuery);

        if (listDt != null)
        {
            foreach (DataRow row in listDt.Rows)
            {
                int brawlerId = System.Convert.ToInt32(row["brawler_id"]);
                string bName = row["name"].ToString();
                int bLevel = System.Convert.ToInt32(row["level"]);
                int bTrophies = System.Convert.ToInt32(row["trophies"]);

                Sprite bPortrait = null;
                if (portraitProvider != null)
                {
                    portraitProvider.TryGetPortraitByID(brawlerId, out bPortrait);
                }

                BrawlerUIItem newItem = Instantiate(brawlerPrefab, brawlersContainer);

                newItem.Setup(brawlerId,bName, bLevel, bTrophies, bPortrait);
            }
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}