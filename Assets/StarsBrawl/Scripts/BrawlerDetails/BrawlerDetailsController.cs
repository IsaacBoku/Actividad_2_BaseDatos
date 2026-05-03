using System.Data;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BrawlerDetailsController : MonoBehaviour
{
    [Header("i. Identificación y Clasificación")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text classText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private Image brawlerPortraitImage;

    [Header("ii. Trofeos")]
    [SerializeField] private TMP_Text trophiesText;

    [Header("iii. Descripción")]
    [SerializeField] private TMP_Text brawlerDescriptionText;

    [Header("iv. Atributo Especial")]
    [SerializeField] private GameObject attributePanel;
    [SerializeField] private TMP_Text attributeDescriptionText;

    [Header("v. Estadísticas de Combate")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text superNameText;

    [Header("Referencias")]
    [SerializeField] private PortraitProvider portraitProvider;

    private async void Start()
    {
        string userId = UserSession.LoggedUserId;
        int brawlerId = UserSession.SelectedBrawlerId;

        if (string.IsNullOrEmpty(userId) || brawlerId == 0)
        {
            Debug.LogError("Faltan datos de sesión.");
            return;
        }

        UpdateBrawlerPortrait(brawlerId);

        await LoadBrawlerDetails(userId, brawlerId);
    }

    private void UpdateBrawlerPortrait(int brawlerId)
    {
        if (portraitProvider != null && brawlerPortraitImage != null)
        {
            if (portraitProvider.TryGetPortraitByID(brawlerId, out Sprite s))
            {
                brawlerPortraitImage.sprite = s;
            }
        }
    }

    private async Task LoadBrawlerDetails(string userId, int brawlerId)
    {
        string query = $@"
        SELECT 
            b.name AS brawler_name,
            c.name AS class_name,
            r.name AS rarity_name,
            ub.trophies,
            b.description AS brawler_desc,
            t.description AS attribute_desc,
            ub.level,
            b.health AS base_health,
            atk.damage AS base_damage,
            atk.projectiles_per_attack,
            s.name AS super_name
        FROM user_brawlers ub
        JOIN brawlers b ON ub.brawler_id = b.brawler_id
        JOIN classes c ON b.class_id = c.class_id
        JOIN rarities r ON b.rarity_id = r.rarity_id
        LEFT JOIN traits t ON b.trait_id = t.trait_id
        JOIN attacks atk ON b.attack_id = atk.attack_id
        JOIN supers s ON b.super_id = s.super_id
        WHERE ub.user_id = '{userId}' 
        AND ub.brawler_id = {brawlerId}";

        DataTable dt = await DatabaseManager.Instance.ExecuteQueryAsync(query);

        if (dt != null && dt.Rows.Count > 0)
        {
            DataRow row = dt.Rows[0];

            nameText.text = row["brawler_name"].ToString();
            classText.text = "Clase: " + row["class_name"].ToString();
            rarityText.text = "Rareza: " + row["rarity_name"].ToString();
            trophiesText.text = row["trophies"].ToString();
            brawlerDescriptionText.text = row["brawler_desc"].ToString();

            // Lógica de Trait
            if (row["attribute_desc"] != System.DBNull.Value && !string.IsNullOrEmpty(row["attribute_desc"].ToString()))
            {
                attributePanel.SetActive(true);
                attributeDescriptionText.text = row["attribute_desc"].ToString();
            }
            else
            {
                attributePanel.SetActive(false);
            }

            // Estadísticas escaladas
            int level = System.Convert.ToInt32(row["level"]);
            int baseHealth = System.Convert.ToInt32(row["base_health"]);
            int baseDamage = System.Convert.ToInt32(row["base_damage"]);
            int projectiles = System.Convert.ToInt32(row["projectiles_per_attack"]);

            float multiplier = 1f + (0.10f * (level - 1));
            int scaledHealth = Mathf.RoundToInt(baseHealth * multiplier);
            int scaledDamage = Mathf.RoundToInt(baseDamage * multiplier);

            levelText.text = level.ToString();
            healthText.text = scaledHealth.ToString();
            superNameText.text = row["super_name"].ToString();

            if (projectiles > 1)
                damageText.text = $"{scaledDamage} x {projectiles}";
            else
                damageText.text = scaledDamage.ToString();
        }
    }

    public void GoBackToBrawlers() => SceneManager.LoadScene("Brawlers");
}