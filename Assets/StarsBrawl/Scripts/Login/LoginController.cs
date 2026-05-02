using UnityEngine;
using TMPro; // Para el InputField
using UnityEngine.SceneManagement;
using System.Data;

public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField idInputField; // Arrastra el InputField aquí
    [SerializeField] private GameObject errorMessage;      // Un texto de error (opcional)

    // Se ejecuta al pulsar el botón CONFIRMAR
    public async void OnConfirmClick()
    {
        string inputId = idInputField.text;

        if (string.IsNullOrEmpty(inputId)) return;

        // Consulta para verificar si el usuario existe
        string query = $"SELECT user_id FROM users WHERE user_id = '{inputId}'";

        DataTable result = await DatabaseManager.Instance.ExecuteQueryAsync(query);

        if (result.Rows.Count > 0)
        {
            // Si existe, guardamos el ID y cargamos MainMenu
            UserSession.LoggedUserId = inputId;
            await SceneManager.LoadSceneAsync("MainMenu");
        }
        else
        {
            if (errorMessage) errorMessage.SetActive(true);
            Debug.LogWarning("El ID de usuario no existe en la base de datos.");
        }
    }
}