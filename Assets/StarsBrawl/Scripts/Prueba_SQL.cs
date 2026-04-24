using UnityEngine;
using MySqlConnector;
public class Prueba_SQL : MonoBehaviour
{
    string servidor = "127.0.0.1";
    string bd = "brawl_stars";
    string usuario = "root";
    string pass = "root";

    void Start()
    {
        string cadenaConexion = $"Server={servidor};Database={bd};User ID={usuario};Password={pass};SsLMode=None;";

        using (MySqlConnection conexion = new MySqlConnection(cadenaConexion))
        {

            conexion.Open();

            string sql = "SELECT * FROM brawlers";

            MySqlCommand cmd = new MySqlCommand(sql, conexion);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string nombre = reader.GetString(1).ToString();
                    //int poder = Convert.ToInt32(reader["poder"]);
                    Debug.Log($"Brawler: {nombre}");
                }
            }

            conexion.Close();
            //try
            //{
            //    conexion.Open();
            //    Debug.Log("Conexión exitosa a la base de datos.");
            //    string consulta = "SELECT * FROM jugadores";
            //    MySqlCommand comando = new MySqlCommand(consulta, conexion);
            //    MySqlDataReader lector = comando.ExecuteReader();
            //    while (lector.Read())
            //    {
            //        string nombreJugador = lector["nombre"].ToString();
            //        int nivelJugador = Convert.ToInt32(lector["nivel"]);
            //        Debug.Log($"Jugador: {nombreJugador}, Nivel: {nivelJugador}");
            //    }
            //}
            //catch (MySqlException ex)
            //{
            //    Debug.LogError($"Error al conectar a la base de datos: {ex.Message}");
            //}
        }
    }

}
