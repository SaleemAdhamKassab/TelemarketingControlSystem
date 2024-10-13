namespace TelemarketingControlSystem.Services.ProjectStatisticService
{
	public class ProjectStatisticsViewModels
	{
		public class ProjectStatisticsViewModel
		{
			public string ProjectName { get; set; }
			public string CreatedBy { get; set; }
			public DateTime AddedOn { get; set; }
			public List<CardViewModel> GeneralDetails { get; set; } = [];
			public StatsticReport StatsticReport { get; set; }
			public List<ClosedPerDay> ClosedPerDays { get; set; } = [];
		}

		public class CardViewModel
		{
			public string Category { get; set; }
			public int Count { get; set; }
			public int Total { get; set; }
		}

		public class StatsticReport
		{
			public List<StatusData> Data { get; set; }
			public List<TelemarketerGSM> Footer { get; set; }
		}

		public class StatusData
		{
			public string Status { get; set; }
			public List<TelemarketerGSM> TelemarketerGSMs { get; set; }
		}

		public class TelemarketerGSM
		{
			public string Telemarketer { get; set; }
			public int AssignedGSMs { get; set; }
		}

		public class ClosedPerDay
		{
			public DateTime Date { get; set; }
			public int Count { get; set; }
		}
		public class HourlyTargetViewModel
		{
			public double TotalMinutes { get; set; }
			public double ClosedCallsDurationAvg { get; set; }
			public List<HourlyStatusTarget> HourlyStatusTargets { get; set; }
		}
		public class HourlyStatusTarget
		{
			public string Status { get; set; }
			public double TotalMinutes { get; set; }
			public double HourPercentage { get; set; }
			public double Rate { get; set; }
			public double Target { get; set; }
		}
		public class HourlyTargetDto
		{
			public int ProjectId { get; set; }
			public DateTime TargetDate { get; set; }
			public List<int> TelemarketerIds { get; set; }
		}
		public class GeneralReportDto
		{
			public int ProjectId { get; set; }
			public DateTime DateFrom { get; set; }
			public DateTime DateTo { get; set; }
			public List<int>? TelemarketerIds { get; set; }
			public string? LineType { get; set; }
			public string? CallStatus { get; set; }
			public string? Generation { get; set; }
			public string? Region { get; set; }
			public string? City { get; set; }
			public string? Segment { get; set; }
			public string? SubSegment { get; set; }
			public string? Bundle { get; set; }
			public string? Contract { get; set; }
		}
	}
}