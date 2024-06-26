namespace TelemarketingControlSystem.Services.ProjectStatistics
{
	public class ProjectStatisticsViewModels
	{
		public class ProjectStatisticsViewModel
		{
			public List<CardViewModel> ProjectGeneralDetails { get; set; } = [];
			public List<CardViewModel> CallStatuses { get; set; } = [];
			public List<CardViewModel> TelemarketersProductivity { get; set; } = [];
			public List<CompletedQuotaPerDay> CompletedQuotaPerDays { get; set; } = [];
		}

		public class CardViewModel
		{
			public string Category { get; set; }
			public int Count { get; set; }
			public int Total { get; set; }
		}
		public class CompletedQuotaPerDay
		{
			public DateOnly Date { get; set; }
			public int Count { get; set; }
		}
	}
}