using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration; // для ConfigurationManager

public class DbHelper
{
    private readonly string connectionString;

    // Конструктор без параметров - берет строку подключения из App.config по имени "MyConnectionString"
    public DbHelper()
    {
        connectionString = ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"].ConnectionString;
    }

    // Конструктор с передачей строки подключения вручную
    public DbHelper(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public string getConnectString()
    {
        return ConfigurationManager.ConnectionStrings["diplom_loskutova.Properties.Settings.DP_2025_LoskutovaConnectionString"].ConnectionString;
    }

    public (int count, string name, string role) CheckUser(string login, string password)
    {
        int count = 0;
        string name = null;
        string role = null;

        string query = @"
        SELECT 
            COUNT(*) OVER() AS UserCount,
            Фамилия + ' ' + Имя + ' ' + Отчество AS UserName,
            CAST(ID_Роли AS NVARCHAR) AS Role
        FROM ПОЛЬЗОВАТЕЛЬ 
        WHERE Логин = @login AND Пароль = @password";

        using (SqlConnection con = new SqlConnection(connectionString))
        using (SqlCommand cmd = new SqlCommand(query, con))
        {
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@password", password);
            con.Open();

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    count = reader.GetInt32(0);
                    if (!reader.IsDBNull(1))
                        name = reader.GetString(1);
                    if (!reader.IsDBNull(2))
                        role = reader.GetString(2);
                }
            }
        }
        return (count, name, role);
    }

    // Выполнить запрос и вернуть DataTable
    public DataTable ExecuteQuery(string query)
    {
        DataTable dt = new DataTable();

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();
            using (SqlCommand cmd = new SqlCommand(query, con))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(dt);
            }
        }
        return dt;
    }

    // Заполнить ListView из DataTable
    public void FillListView(ListView listView, DataTable dataTable)
    {
        listView.Items.Clear();
        listView.Columns.Clear();

        // Добавляем колонки
        foreach (DataColumn column in dataTable.Columns)
        {
            listView.Columns.Add(column.ColumnName);
        }
        listView.View = View.Details;

        // Добавляем строки
        foreach (DataRow row in dataTable.Rows)
        {
            ListViewItem item = new ListViewItem(row[0].ToString());
            for (int i = 1; i < dataTable.Columns.Count; i++)
            {
                item.SubItems.Add(row[i].ToString());
            }
            listView.Items.Add(item);
        }

        // Автоширина колонок
        foreach (ColumnHeader column in listView.Columns)
        {
            column.Width = -2;
        }
    }
}
