namespace TelemarketingControlSystem.Helper
{
    public class ConstantValues
    {
        public enum enSortDirection { asc, desc }
        public enum enAllowedFileExtension { xlsx } 
        public enum enExcelUploadFolderName { ExcelUploads };
        public enum enAccessType { all, allOnlyMe, viewOnlyMe, denied }
        public enum enRoles { Admin, Telemarketer, Researcher, Segmentation }

        //public static List<string> projectTypes = ["", "N/A", "RS", "SG", "TM"];
        public static List<string> regions = ["", "N/A", "Central", "Coastal", "Eastern", "Northern", "Southern"];
    }
}