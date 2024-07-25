using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Services.ProjectStatistics
{
	public class ProjectStatisticsViewModels
	{
		public class ProjectStatisticsViewModel
		{
			public string ProjectName { get; set; }
			public string CreatedBy { get; set; }
			public DateTime AddedOn { get; set; }

			public List<CardViewModel> GeneralDetails { get; set; } = [];
			public List<CardViewModel> CallStatuses { get; set; } = [];
			public List<TelemarketerProductivityCardViewModel> TelemarketerProductivities { get; set; } = [];
			public List<CompletedQuotaPerDay> CompletedQuotaPerDays { get; set; } = [];
		}

		public class CardViewModel
		{
			public string Category { get; set; }
			public int Count { get; set; }
			public int Total { get; set; }
		}
		public class TelemarketerProductivityCardViewModel
		{
			public string Telemarketer { get; set; }
			public int AssignedGSMs { get; set; }
			public int Completed { get; set; }
			public int Closed { get; set; }
			public double CompletedRate { get; set; }
			public double ClosedRate { get; set; }
		}
		public class CompletedQuotaPerDay
		{
			public DateTime Date { get; set; }
			public int Count { get; set; }
		}
		public class HourlyTelemarketerTargetViewModel
		{
			public double AverageCompletedCalls { get; set; }
			public List<HourlyTelemarketerTargetCallStatusViewModel> Data { get; set; }
		}
		public class HourlyTelemarketerTargetCallStatusViewModel
		{
			public string Status { get; set; }
			public double TotalMinutes { get; set; }
			public double HourPercentage { get; set; }
			public double Rate { get; set; }
			public double Target { get; set; }

		}
	}
}