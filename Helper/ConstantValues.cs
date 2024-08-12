namespace TelemarketingControlSystem.Helper
{
    public class ConstantValues
    {
        public enum enSortDirection { asc, desc }
        public enum enAllowedFileExtension { xlsx }
        public enum enExcelUploadFolderName { ExcelUploads };
        public enum enAccessType { all, allOnlyMe, viewOnlyMe, denied }
        public enum enRoles { Admin, Telemarketer, Researcher }

        public static List<string> projectTypes = ["", "N/A", "RS", "SG", "TM"];
        //public static List<string> generations = ["", "N/A", "2G", "3G", "4G"];
        //public static List<string> lineTypes = ["", "N/A", "Pre", "Post"];
        //public static List<string> cities = ["", "N/A", "Aleppo", "Damascus", "Damascus Rural", "Deir Alzor", "Hama", "Hasakeh", "Homs", "Idleb", "Kounaitra", "Latakia", "Raqa", "Sweida", "Tartous"];
        public static List<string> regions = ["", "N/A", "Central", "Coastal", "Eastern", "Northern", "Southern"];
    }
}