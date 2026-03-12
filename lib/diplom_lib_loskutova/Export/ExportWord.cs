using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace diplom_lib_loskutova.Export
{
    public class ExportWord
    {
        ConnectionDataBase connectionDB = new ConnectionDataBase();
        public void export(string query, string message)
        {
            var datatable = new DataTable();

            queryReturnData(query, datatable);
            ExportToWord(datatable, message);
        }
        public void exportRevers(string query, string message)
        {
            var datatable = new DataTable();

            queryReturnData(query, datatable);
            ExportToWordRevers(datatable, message);
        }

        public void exportStroka(string query, string message)
        {
            var datatable = new DataTable();

            queryReturnData(query, datatable);
            ExportToWordStroka(datatable, message);
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
        public void ExportToWord(DataTable dataTable, string message = "")
        {
            if (dataTable.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();
                word.Application.Documents.Add(Type.Missing);

                // Вставляем абзац с текстом перед таблицей
                word.Selection.TypeText(message);
                word.Selection.TypeParagraph(); // Добавляем пустой абзац

                // Создаем таблицу
                Microsoft.Office.Interop.Word.Table table = word.Application.ActiveDocument.Tables.Add(word.Selection.Range, dataTable.Rows.Count + 1, dataTable.Columns.Count, Type.Missing, Type.Missing);
                table.Borders.OutsideLineStyle = Microsoft.Office.Interop.Word.WdLineStyle.wdLineStyleSingle;
                table.Borders.InsideLineStyle = Microsoft.Office.Interop.Word.WdLineStyle.wdLineStyleSingle;

                // Заполняем заголовки таблицы
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    table.Cell(1, i + 1).Range.Text = dataTable.Columns[i].ColumnName;
                }

                // Заполняем данные таблицы
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        table.Cell(i + 2, j + 1).Range.Text = dataTable.Rows[i][j].ToString();
                    }
                }

                word.Visible = true;
            }
            else
            {
                MessageBox.Show("No data to export!");
            }
        }

        public void ExportToWordStroka(DataTable dataTable, string message = "")
        {
            if (dataTable.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();
                word.Application.Documents.Add(Type.Missing);

                // Проверяем, есть ли сообщение
                if (!string.IsNullOrEmpty(message))
                {
                    // Вставляем абзац с текстом перед данными
                    word.Selection.TypeText(message);
                    word.Selection.TypeParagraph(); // Добавляем пустой абзац
                }

                // Создаем строку для хранения данных
                StringBuilder dataBuilder = new StringBuilder();
                dataBuilder.Append("Перенесённые инфекционные заболевания: ");

                // Добавляем данные из DataTable в строку
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        dataBuilder.Append(dataTable.Rows[i][j].ToString());
                        // Проверяем, является ли текущий элемент последним в строке данных
                        if (i != dataTable.Rows.Count - 1 || j != dataTable.Columns.Count - 1)
                        {
                            // Если элемент не последний, добавляем запятую
                            dataBuilder.Append(", ");
                        }
                    }
                }

                // Вставляем собранную строку с данными в документ Word
                word.Selection.TypeText(dataBuilder.ToString());

                word.Visible = true;
            }
            else
            {
                MessageBox.Show("No data to export!");
            }
        }

        public void ExportToWordRevers(DataTable dataTable, string message = "")
        {
            if (dataTable.Rows.Count > 0)
            {
                Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();
                word.Application.Documents.Add(Type.Missing);

                // Вставляем абзац с текстом перед таблицей
                word.Selection.TypeText(message);
                word.Selection.TypeParagraph(); // Добавляем пустой абзац

                // Создаем таблицу
                Microsoft.Office.Interop.Word.Table table = word.Application.ActiveDocument.Tables.Add(word.Selection.Range, dataTable.Rows.Count + 1, dataTable.Columns.Count + 1, Type.Missing, Type.Missing);
                table.Borders.OutsideLineStyle = Microsoft.Office.Interop.Word.WdLineStyle.wdLineStyleSingle;
                table.Borders.InsideLineStyle = Microsoft.Office.Interop.Word.WdLineStyle.wdLineStyleSingle;

                // Заполняем заголовки таблицы (первая колонка)
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    // Записываем название обследования в первую колонку
                    table.Cell(i + 2, 1).Range.Text = dataTable.Columns[i].ColumnName;
                }

                // Заполняем данные таблицы (остальные колонки)
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        // Записываем данные в соответствующую ячейку таблицы
                        table.Cell(i + 2, j + 2).Range.Text = dataTable.Rows[i][j].ToString();
                    }
                }

                word.Visible = true;
            }
            else
            {
                MessageBox.Show("No data to export!");
            }
        }
    }
}
