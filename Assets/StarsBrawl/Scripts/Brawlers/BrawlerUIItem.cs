using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrawlerUIItem : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text trophiesText;

    private int myBrawlerId;

    public void Setup(int brawlerId,string brawlerName, int level, int trophies, Sprite portrait)
    {
        myBrawlerId = brawlerId;
        nameText.text = brawlerName;
        levelText.text = $"{level}";
        trophiesText.text = trophies.ToString();

        if (portrait != null)
        {
            portraitImage.sprite = portrait;
        }
    }
    public void OnBrawlerClicked()
    {
        UserSession.SelectedBrawlerId = myBrawlerId;
        UnityEngine.SceneManagement.SceneManager.LoadScene("BrawlerDetails");
    }
}