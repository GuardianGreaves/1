using System;
using System.Data.SqlClient;

public class StatusRep
{
    private readonly string _connectionString;

    public const string GetStatus = @"
        SELECT * FROM [dbo].[СТАТУС];
    ";

    public StatusRep(string connectionString)
    {
        _connectionString = connectionString;
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

    public void Delete(int id)
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
}
