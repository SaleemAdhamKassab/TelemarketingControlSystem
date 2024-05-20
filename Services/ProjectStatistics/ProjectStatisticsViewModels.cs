namespace TelemarketingControlSystem.Services.ProjectStatistics
{
	public class ProjectStatisticsViewModels
	{
		public class ProjectStatisticsViewModel
		{
			public string ProjectName { get; set; }
			public string CreatedBy { get; set; }
			public int TotalGSMCount { get; set; }
			public int Quota { get; set; }
			public DateTime DateFrom { get; set; }
			public DateTime DateTo { get; set; }

			public List<GSMStatusStatistic> GSMStatusStatistics { get; set; }
		}

		public class GSMStatusStatistic
		{
			public string Status { get; set; }
			public int GSMCount { get; set; }
		}
	}
}
