using System;
using System.Data;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleLogController : MonoBehaviour
{
    [Header("UI - Contenedor")]
    [SerializeField] private Transform logContainer;
    [SerializeField] private BattleLogUIItem logPrefab;

    private async void Start()
    {
        string userId = UserSession.LoggedUserId;
        if (string.IsNullOrEmpty(userId)) return;

        await LoadBattleLog(userId);
    }

    private async Task LoadBattleLog(string userId)
    {
        foreach (Transform child in logContainer)
            Destroy(child.gameObject);

        string query = $@"
        SELECT 
            m.match_id,
            mt.name AS mode_name, 
            mt.modality, 
            mu.result, 
            m.date
        FROM match_user mu
        JOIN matches m ON mu.match_id = m.match_id
        JOIN match_types mt ON m.match_type_id = mt.match_type_id
        WHERE mu.user_id = '{userId}'
        ORDER BY m.date DESC
        LIMIT 20";

        DataTable dt = await DatabaseManager.Instance.ExecuteQueryAsync(query);

        if (dt != null)
        {
            DateTime now = DateTime.Now;

            foreach (DataRow row in dt.Rows)
            {
                int matchId = Convert.ToInt32(row["match_id"]);
                string modeName = row["mode_name"].ToString();
                string modality = row["modality"].ToString();
                string result = row["result"].ToString();

                DateTime matchDate = Convert.ToDateTime(row["date"]);
                int daysAgo = (now.Date - matchDate.Date).Days;

                BattleLogUIItem newItem = Instantiate(logPrefab, logContainer);
                newItem.Setup(modeName, modality, result, daysAgo, matchId);
            }
        }
    }

    public void GoBack() => SceneManager.LoadScene("MainMenu");
}