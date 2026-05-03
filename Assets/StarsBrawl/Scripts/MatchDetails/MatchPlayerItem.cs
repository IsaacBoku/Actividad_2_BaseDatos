using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchPlayerItem : MonoBehaviour
{
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text trophiesText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image portraitImage;

    public void Setup(string nickname, int trophies, int level, Sprite portrait)
    {
        nicknameText.text = nickname;
        trophiesText.text = trophies.ToString();
        levelText.text = level.ToString();

        if (portrait != null)
            portraitImage.sprite = portrait;
    }
}