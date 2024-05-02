namespace TelemarketingControlSystem.Helper
{
	public class ConstantValues
	{
		public enum enSortDirection { asc, desc }
		public enum enAllowedFileExtension { xlsx }
		public enum enExcelUploadFolderName { ExcelUploads };
		public enum enAccessType { all, allOnlyMe, viewOnlyMe, denied }

		public static List<string> projectTypes = ["RS", "SG", "TM"];
		public static List<string> lineTypes = ["Pre", "Post"];
		public static List<string> regions = ["Central", "Coastal", "Eastern", "Northern", "Southern"];
		public static List<string> cities = ["Aleppo", "Damascus", "Damascus Rural", "Deir Alzor", "Hama", "Hasakeh", "Homs", "Idleb", "Kounaitra", "Latakia", "Raqa", "Sweida", "Tartous"];
		public static List<string> callStatuses = ["Initialize", "Completed", "InComplete", "No Answer", "Number is Not Exist", "Out of Coverage", "Quota (Out of Quota)", "Refused to Cooperate", "Canceled", "F1", "F2", "F3", "F4", "Informed", "Done", "Subscribed", "Don't Have Alternative Number", "Subscribed On Another Number", "Service Out", "Another User", "Done - No modification"];
		public static List<string> generations = ["2G", "3G", "4G"];
	}
}