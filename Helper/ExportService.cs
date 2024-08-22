using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace TelemarketingControlSystem.Helper
{
    public  class ExportService
    {
        public  byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();

            // Add a new worksheet
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            // Add the headers
            var properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = properties[i].Name;
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            // Add the data
            int row = 2;
            foreach (var item in data)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[row, i + 1].Value = properties[i].GetValue(item);
                }
                row++;
            }

            // Auto fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Return the Excel file as a byte array
            return package.GetAsByteArray();
        }
    }
}
