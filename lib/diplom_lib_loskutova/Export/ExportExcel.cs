using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace diplom_lib_loskutova.Export
{
    public class ExportExcel
    {
        ConnectionDataBase connectionDB = new ConnectionDataBase();

        public void ExportDataTableToExcel(DataTable dataTable, string filePath, string sheetName = "Лист1")
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                System.Windows.MessageBox.Show("Нет данных для экспорта!");
                return;
            }

            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                // 1. Рабочая книга
                WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // 2. Стили (ОБЯЗАТЕЛЬНО!)
                AddStyles(workbookPart);

                // 3. Лист
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                // 4. Регистрируем лист
                Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet()
                {
                    Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = sheetName
                };
                sheets.Append(sheet);

                // 5. Данные листа
                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // 6. ЗАГОЛОВКИ (жирный шрифт)
                Row headerRow = new Row() { RowIndex = 1 };
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    Cell cell = new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(dataTable.Columns[i].ColumnName),
                        StyleIndex = 1  // Жирный стиль
                    };
                    headerRow.Append(cell);
                }
                sheetData.Append(headerRow);

                // 7. ДАННЫЕ
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    Row row = new Row() { RowIndex = (uint)(i + 2) };
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        Cell cell = new Cell()
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(dataTable.Rows[i][j]?.ToString() ?? ""),
                            StyleIndex = 0  // Обычный стиль
                        };
                        row.Append(cell);
                    }
                    sheetData.Append(row);
                }

                // 8. СОХРАНЕНИЕ
                workbookPart.Workbook.Save();
                // using автоматически закроет документ
            }
        }

        private void AddStyles(WorkbookPart workbookPart)
        {
            WorkbookStylesPart stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = new Stylesheet();

            // Шрифты
            Fonts fonts = new Fonts(
                new Font(), // Обычный
                new Font(new Bold()) // Жирный
            )
            { Count = 2u };

            // Заливки
            Fills fills = new Fills(
                new Fill(), // По умолчанию
                new Fill(new PatternFill() { PatternType = PatternValues.None })
            )
            { Count = 2u };

            // Границы
            Borders borders = new Borders(
                new Border(), // По умолчанию
                new Border()  // Простая граница
            )
            { Count = 2u };

            // Форматы ячеек
            CellFormats cellFormats = new CellFormats(
                new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 }, // Обычная
                new CellFormat() { FontId = 1, FillId = 0, BorderId = 0, ApplyFont = true } // Жирная
            )
            { Count = 2u };

            stylesPart.Stylesheet.Append(fonts);
            stylesPart.Stylesheet.Append(fills);
            stylesPart.Stylesheet.Append(borders);
            stylesPart.Stylesheet.Append(cellFormats);
            stylesPart.Stylesheet.Save();
        }
    }
}
