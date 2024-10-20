namespace TelemarketingControlSystem.Helper
{
	public class GeneralFilterModel
	{
		public string? SearchQuery { get; set; }
		public int PageIndex { get; set; }
		public int PageSize { get; set; } = 5;
		public string? SortActive { get; set; }
		public string? SortDirection { get; set; }
	}

	public class LookUpResponse
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}
