using System.Data;
using System.Data.SqlClient;
using System.Configuration;

public static class DatabaseHelper
{
    // Метод с параметрами подключения и запроса
    public static DataTable LoadData(string connectionString, string query)
    {
        using (var connection = new SqlConnection(connectionString))
        {
            var adapter = new SqlDataAdapter(query, connection);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }
    }

    // Метод с использованием строки подключения по умолчанию
    public static DataTable LoadData(string query)
    {
        string defaultConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        return LoadData(defaultConnectionString, query);
    }
}
