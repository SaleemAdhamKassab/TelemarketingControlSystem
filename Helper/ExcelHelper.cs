using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TelemarketingControlSystem.Services.Projects;

namespace TelemarketingControlSystem.Helper
{
	public class ExcelHelper
	{
		//Reference: https://juldhais.net/upload-and-save-excel-file-data-into-the-database-in-asp-net-core-web-api-44957cc8ddeb

		private static void validateCell(string columnName, int rowNumber, object cellValue, List<string> validList)
		{
			rowNumber++;
			//if (string.IsNullOrEmpty(cellValue.ToString()))
			//	throw new Exception($"Empty {columnName} at line number: {rowNumber}");

			if (!string.IsNullOrEmpty(cellValue.ToString()) && !validList.Contains(cellValue.ToString()))
				throw new Exception($"Invalid {columnName}: [{cellValue}] at line number: {rowNumber}");
		}

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

						//        switch (property.Name)
						//        {
						//            case "GSM":
						//                if (string.IsNullOrEmpty(cell.ToString()))
						//                    throw new Exception($"Empty {property.Name} at line number: {currentRow + 1}");
						//                string duplicatedGsm = listResult.Where(e => e.GSM == obj.GSM).Select(e => e.GSM).FirstOrDefault();
						//                if (!string.IsNullOrEmpty(duplicatedGsm))
						//                    throw new Exception($"Duplicated GSM {duplicatedGsm} at line number: {currentRow + 1}");
						//                break;

						//            //case "LineType":
						//            //    validateCell(property.Name, currentRow, cell, ConstantValues.lineTypes);
						//            //    break;

						//            //case "CallStatus":
						//            //    validateCell(property.Name, currentRow, cell, ConstantValues.callStatuses);
						//            //    break;

						//            //case "Generation":
						//            //    validateCell(property.Name, currentRow, cell, ConstantValues.generations);
						//            //    break;

						//            //case "Region":
						//            //    validateCell(property.Name, currentRow, cell, ConstantValues.regions);
						//            //    break;

						//            //case "City":
						//            //    validateCell(property.Name, currentRow, cell, ConstantValues.cities);
						//            //    break;
						//        }
						//    }
						//    ////else if(cell is not null)
						//    ////    property.SetValue(obj, Convert.ChangeType(cell.StringCellValue, property.PropertyType));
						//}

						//listResult.Add(obj);
						//currentRow++;
					}
				}

				listResult.Add(obj);
				currentRow++;
			}
			return listResult;
		}
	}
}