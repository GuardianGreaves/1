using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Controls;

public class StatusRep
{
    private readonly string _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

    public const string GetStatus = @"
        SELECT * FROM [dbo].[СТАТУС];
    ";

    public StatusRep()
    {
    }

    public void Add(string name, string description)
    {
        string sql = "INSERT INTO [dbo].[СТАТУС] (Название, Описание) VALUES (@Name, @Description)";
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public void Update(int id, string name, string description)
    {
        string sql = "UPDATE [dbo].[СТАТУС] SET Название = @Name, Описание = @Description WHERE ID_Статуса = @Id";
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteRow(int id)
    {
        string sql = "DELETE FROM [dbo].[СТАТУС] WHERE ID_Статуса = @Id";
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public static void DeleteSelectedRows(DataGrid dataGrid)
    {
        foreach (DataRowView row in dataGrid.SelectedItems)
        {
            var repo = new StatusRep();
            int id = Convert.ToInt32(row["ID_Статуса"]);
            repo.DeleteRow(id);
        }
    }



    public static int SelectedRow(DataGrid dataGrid)
    {
        int id = 0;
        foreach (DataRowView row in dataGrid.SelectedItems)
        {
            var repo = new StatusRep();
            id = Convert.ToInt32(row["ID_Статуса"]);
        }
        return id;
    }

}
