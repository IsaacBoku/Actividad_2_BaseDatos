using UnityEngine;
using MySqlConnector;
using System.Threading.Tasks;
using System.Data;
public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    [Header("Configuración de la Base de datos")]
    [SerializeField] private string servidor = "localhost";
    [SerializeField] private string bd = "brawl_stars";
    [SerializeField] private string usuario = "root";
    [SerializeField] private string pass = "root";

    private string cadenaConexion;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        cadenaConexion = $"Server={servidor};Database={bd};User ID={usuario};Password={pass};SslMode=None;";
    }

    public async Task<DataTable> ExecuteQueryAsync(string query)
    {
        DataTable result = new DataTable();
        try
        {
            using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
            {
                await conexion.OpenAsync();
                using (MySqlCommand cmd = new MySqlCommand(query, conexion))
                {
                    using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        result.Load(reader);
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"Error al ejecutar la consulta: {ex.Message}");
        }
        return result;
    }

}
