using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using TelemarketingControlSystem.Services.ProjectService;
using System.Drawing;

namespace TelemarketingControlSystem.Services.ExcelService
{
	public interface IExcelService
	{
		//List<GSMExcel> Import<T>(string filePath, int sheetIndex) where T : new();
		List<T> Import<T>(string filePath, int sheetIndex) where T : new();
		byte[] Export<T>(IEnumerable<T> data, string sheetName);
		string SaveFile(IFormFile file, string folderName);
	}

	public class ExcelService(IWebHostEnvironment webHostEnvironment) : IExcelService
	{
		private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

		public string SaveFile(IFormFile file, string folderName)
		{
			var extension = Path.GetExtension(file.FileName);
			var webRootPath = _webHostEnvironment.WebRootPath;
			if (string.IsNullOrWhiteSpace(webRootPath))
				webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

			//var folderPath = Path.Combine(webRootPath, enExcelUploadFolderName.ExcelUploads.ToString());
			var folderPath = Path.Combine(webRootPath, folderName);
			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			//var fileName = $"{Guid.NewGuid()}.{extension}";
			var fileName = $"{file.Name}_{Guid.NewGuid()}{extension}";
			var filePath = Path.Combine(folderPath, fileName);
			using var stream = new FileStream(filePath, FileMode.Create);
			file.CopyTo(stream);

			return filePath;
		}
		public byte[] Export<T>(IEnumerable<T> data, string sheetName)
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
		public List<T> Import<T>(string filePath, int sheetIndex) where T : new()
		{
			List<T> listResult = [];
			XSSFWorkbook workbook;
			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				workbook = new XSSFWorkbook(stream);

			var sheet = workbook.GetSheetAt(sheetIndex);
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
				T obj = new();
				foreach (var property in typeof(T).GetProperties())
				{
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
		//public List<GSMExcel> Import<T>(string filePath, int sheetIndex) where T : new()
		//{
		//	List<GSMExcel> listResult = [];
		//	XSSFWorkbook workbook;
		//	using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
		//		workbook = new XSSFWorkbook(stream);

		//	var sheet = workbook.GetSheetAt(sheetIndex);
		//	var rowHeader = sheet.GetRow(0);
		//	var colIndexList = new Dictionary<string, int>();

		//	foreach (var cell in rowHeader.Cells)
		//	{
		//		var colName = cell.StringCellValue;
		//		colIndexList.Add(colName, cell.ColumnIndex);
		//	}

		//	var currentRow = 1;
		//	while (currentRow <= sheet.LastRowNum)
		//	{
		//		var row = sheet.GetRow(currentRow);
		//		if (row == null) break;
		//		GSMExcel obj = new();
		//		foreach (var property in typeof(T).GetProperties())
		//		{
		//			if (!colIndexList.ContainsKey(property.Name))
		//				throw new Exception($"The {property.Name} column  is not found");

		//			var colIndex = colIndexList[property.Name];
		//			var cell = row.GetCell(colIndex);

		//			if (cell != null && property.PropertyType == typeof(string))
		//			{
		//				cell.SetCellType(CellType.String);
		//				property.SetValue(obj, cell.StringCellValue);
		//			}
		//		}

		//		listResult.Add(obj);
		//		currentRow++;
		//	}
		//	return listResult;
		//}
	}
}
