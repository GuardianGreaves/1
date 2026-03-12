using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace diplom_lib_loskutova.Export
{
    public class ExportExcel
    {
        ConnectionDataBase connectionDB = new ConnectionDataBase();

        public void export(string query)
        {
            var datatable = new DataTable();

            queryReturnData(query, datatable);

            ExportToExcel(datatable);
        }
        public DataTable queryReturnData(string query, DataTable dataTable)
        {
            SqlConnection myCon = new SqlConnection(connectionDB.GetSqlConnection().ConnectionString);
            myCon.Open();

            SqlDataAdapter SDA = new SqlDataAdapter(query, myCon);
            SDA.SelectCommand.ExecuteNonQuery();

            SDA.Fill(dataTable);
            return dataTable;
        }
        public void ExportToExcel(DataTable dataTable)
        {
            if (dataTable.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                excel.Application.Workbooks.Add(Type.Missing);

                for (int i = 1; i < dataTable.Columns.Count + 1; i++)
                {
                    excel.Cells[1, i] = dataTable.Columns[i - 1].ColumnName;
                }

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        excel.Cells[i + 2, j + 1] = dataTable.Rows[i][j].ToString();
                    }
                }

                excel.Columns.AutoFit();
                excel.Visible = true;
            }
            else
            {
                MessageBox.Show("No data to export!");
            }
        }
    }
}
