using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace diplom_lib_loskutova.Export
{
    public class ExportWord
    {
        ConnectionDataBase connectionDB = new ConnectionDataBase();
        public void ExportDataTableToWord(DataTable dataTable, string filePath, string title = "")
        {
            // Проверяем данные
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                System.Windows.MessageBox.Show("Нет данных для экспорта!");
                return;
            }

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                Body body = mainPart.Document.Body;

                // Заголовок
                if (!string.IsNullOrEmpty(title))
                {
                    Paragraph titlePara = new Paragraph(new Run(new Text(title)));
                    RunProperties titleProps = new RunProperties(new Bold());
                    titlePara.Descendants<Run>().First().PrependChild(titleProps);
                    body.Append(titlePara);
                    body.Append(new Paragraph()); // Пустой абзац
                }

                // Таблица
                Table table = CreateTableFromDataTable(dataTable);
                body.Append(table);

                mainPart.Document.Save();
            }
        }

        private Table CreateTableFromDataTable(DataTable dataTable)
        {
            Table table = new Table();

            // ЗАГОЛОВКИ
            TableRow headerRow = new TableRow();
            foreach (DataColumn col in dataTable.Columns)
            {
                Paragraph para = new Paragraph(new Run(new Text(col.ColumnName)));
                RunProperties props = new RunProperties(new Bold());
                para.Descendants<Run>().First().PrependChild(props);

                TableCell cell = new TableCell(para);
                cell.PrependChild(new TableCellProperties(new TableCellWidth()
                {
                    Type = TableWidthUnitValues.Dxa,
                    Width = "2500"
                }));
                headerRow.Append(cell);
            }
            table.Append(headerRow);

            // ДАННЫЕ
            foreach (DataRow row in dataTable.Rows)
            {
                TableRow dataRow = new TableRow();
                foreach (DataColumn col in dataTable.Columns)
                {
                    string cellValue = row[col]?.ToString() ?? "";
                    TableCell cell = new TableCell(new Paragraph(new Run(new Text(cellValue))));
                    cell.PrependChild(new TableCellProperties(new TableCellWidth()
                    {
                        Type = TableWidthUnitValues.Dxa,
                        Width = "2500"
                    }));
                    dataRow.Append(cell);
                }
                table.Append(dataRow);
            }

            // ГРАНИЦЫ ТАБЛИЦЫ
            table.PrependChild(new TableProperties(
                new TableBorders(
                    new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                    new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                )
            ));

            return table;
        }
    }
}