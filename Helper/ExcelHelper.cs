using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TelemarketingControlSystem.Services.Projects;

namespace TelemarketingControlSystem.Helper
{
	public class ExcelHelper
	{
		//Reference: https://juldhais.net/upload-and-save-excel-file-data-into-the-database-in-asp-net-core-web-api-44957cc8ddeb

		public static List<GSMExcel> Import<T>(string filePath) where T : new()
		{
			List<GSMExcel> listResult = [];
			XSSFWorkbook workbook;
			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				workbook = new XSSFWorkbook(stream);

			var sheet = workbook.GetSheetAt(0);
			var rowHeader = sheet.GetRow(0);
			var colIndexList = new Dictionary<string, int>();

			foreach (var cell in rowHeader.Cells)
			{
				var colName = cell.StringCellValue;
				colIndexList.Add(colName, cell.ColumnIndex);
			}

			var currentRow = 1;
			while (currentRow <= sheet.LastRowNum)
			{
				var row = sheet.GetRow(currentRow);
				if (row == null) break;
				GSMExcel obj = new();
				foreach (var property in typeof(T).GetProperties())
				{
					if (!colIndexList.ContainsKey(property.Name))
						throw new Exception($"The {property.Name} column  is not found");

					var colIndex = colIndexList[property.Name];
					var cell = row.GetCell(colIndex);

					if (cell != null && property.PropertyType == typeof(string))
					{
						cell.SetCellType(CellType.String);
						property.SetValue(obj, cell.StringCellValue);
					}
				}

				listResult.Add(obj);
				currentRow++;
			}
			return listResult;
		}
	}
}