using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models;
using TelemarketingControlSystem.Models.Data;
using static TelemarketingControlSystem.Services.Auth.AuthModels;
using static TelemarketingControlSystem.Services.ProjectStatistics.ProjectStatisticsViewModels;

namespace TelemarketingControlSystem.Services.ProjectStatistics
{
	public interface IProjectStatisticsService
	{
		ResultWithMessage getProjectStatistics(int projectId, DateTime dateFrom, DateTime dateTo, TenantDto authData);
	}
	public class ProjectStatisticsService : IProjectStatisticsService
	{
		private readonly ApplicationDbContext _db;
		public ProjectStatisticsService(ApplicationDbContext db) => _db = db;

		private List<CardViewModel> getProjectGeneralDetails(Project project)
		{
			List<CardViewModel> result = [];

			result.Add(new CardViewModel()
			{
				Category = "Actual Completion",
				Count = project.ProjectDetails.Where(e => e.CallStatusId == ConstantValues.callStatuses.IndexOf("Completed")).Count(),
				Total = project.ProjectDetails.Count,
			});

			result.Add(new CardViewModel()
			{
				Category = "Actual Non-Completion",
				Count = result.ElementAt(0).Total - result.ElementAt(0).Count,
				Total = result.ElementAt(0).Total,
			});

			result.Add(new CardViewModel()
			{
				Category = "Quota Progress",
				Count = result.ElementAt(0).Count,
				Total = project.Quota,
			});

			result.Add(new CardViewModel()
			{
				Category = "Telemarketer Count",
				Count = project.ProjectDetails.Select(e => e.EmployeeId).Distinct().Count(),
				Total = _db.Employees.Count()
			});

			return result;
		}
		private List<CardViewModel> getCallStatuses(Project project)
		{
			List<CardViewModel> result = [];

			result = project.ProjectDetails
										.GroupBy(g => g.CallStatusId).
										Select(e => new CardViewModel
										{
											Category = ConstantValues.callStatuses.ElementAt(e.Key.Value - 1),
											Count = e.Count(),
											Total = 0
										}).ToList();

			return result;
		}
		private List<CardViewModel> getTelemarketersProductivity(Project project)
		{
			List<CardViewModel> result = [];

			result = project.ProjectDetails
					.GroupBy(e => e.Employee.UserName)
					.Select(e => new CardViewModel
					{
						Category = e.Key,
						Count = 0,
						Total = 0
					}).ToList();

			return result;
		}
		private List<CompletedQuotaPerDay> getCompletedQuotaPerDay(Project project)
		{
			List<CompletedQuotaPerDay> result = [];

			result = project.ProjectDetails
				.Where(e => e.CallStatusId == ConstantValues.callStatuses.IndexOf("Completed"))
				.GroupBy(g => DateOnly.FromDateTime(g.LastUpdateDate.GetValueOrDefault()))
				.Select(e => new CompletedQuotaPerDay
				{
					Date = e.Key,
					Count = e.Count()
				}).ToList();

			return result;
		}


		public ResultWithMessage getProjectStatistics(int projectId, DateTime dateFrom, DateTime dateTo, TenantDto authData)
		{
			if (dateFrom > dateTo)
				return new ResultWithMessage(null, "Date To should be greater or equal than Date From");

			Project project = _db.Projects
				.Include(e => e.ProjectDetails)
				.ThenInclude(e => e.Employee)
				.SingleOrDefault(e => e.Id == projectId && !e.IsDeleted && (e.DateFrom >= dateFrom && e.DateTo <= dateTo));

			if (project is null)
				return new ResultWithMessage(null, $"Invalid project id: {projectId}");

			ProjectStatisticsViewModel result = new()
			{
				ProjectGeneralDetails = getProjectGeneralDetails(project),
				CallStatuses = getCallStatuses(project),
				TelemarketersProductivity = getTelemarketersProductivity(project),
				CompletedQuotaPerDays = getCompletedQuotaPerDay(project)
			};

			return new ResultWithMessage(result, string.Empty);
		}
	}
}