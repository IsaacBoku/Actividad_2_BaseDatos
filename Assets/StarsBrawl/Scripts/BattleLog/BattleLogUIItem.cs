using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleLogUIItem : MonoBehaviour
{
    [SerializeField] private TMP_Text modeNameText;
    [SerializeField] private TMP_Text modalityText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text dateText;

    public int MatchId { get; private set; }

    public void Setup(string modeName, string modality, string result, int daysAgo, int matchId)
    {
        MatchId = matchId;
        modeNameText.text = modeName;
        modalityText.text = modality;
        resultText.text = result;

        if (daysAgo == 0) dateText.text = "Hoy";
        else if (daysAgo == 1) dateText.text = "Ayer";
        else dateText.text = $"Hace {daysAgo} días";

        if (result.Trim() == "Victoria") resultText.color = Color.green;
        else if (result.Trim() == "Derrota") resultText.color = Color.red;
    }

    public void OnClick()
    {
        UserSession.SelectedMatchId = MatchId;
        SceneManager.LoadScene("MatchDetails");
    }
}