using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace diplom_lib_loskutova
{
    public class ConnectionDataBase : IDisposable
    {
        private readonly string connectionString;
        private SqlConnection connection;

        public ConnectionDataBase(string customConnectionString = null)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DP_2025_LoskutovaConnectionString"]?.ConnectionString
                ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=DP_2025_Loskutova;Integrated Security=True";
            connection = new SqlConnection(connectionString);
        }

        public DataTable ExecuteSql(string sql)
        {
            connection.Open(); // Открываем подключение перед выполнением запроса
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                connection.Close(); // Закрываем подключение после выполнения запроса
                return dt;
            }
        }

        public DataTable ExecuteSqlParameters(string sql, params SqlParameter[] parameters)
        {
            connection.Open(); // Открываем подключение перед выполнением запроса
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddRange(parameters);
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    connection.Close(); // Закрываем подключение после выполнения запроса
                    return dt;
                }
            }
        }

        public SqlConnection GetSqlConnection()
        {
            if (connection == null)
            {
                connection = new SqlConnection(connectionString);
            }
            return connection;
        }

        public void Dispose()
        {
        }
    }
}
