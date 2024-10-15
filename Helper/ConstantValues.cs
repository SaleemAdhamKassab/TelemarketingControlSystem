namespace TelemarketingControlSystem.Helper
{
	public class ConstantValues
	{
		public enum enSortDirection { asc, desc }
		public enum enAllowedFileExtension { xlsx }
		public enum enAccessType { all, allOnlyMe, viewOnlyMe, denied }
		public enum enRoles { Admin, Telemarketer, Researcher, Segmentation }
		public static List<string> regions = ["", "N/A", "Central", "Coastal", "Eastern", "Northern", "Southern"];
	}
}