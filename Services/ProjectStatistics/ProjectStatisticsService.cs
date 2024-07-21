using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
				//Count = project.ProjectDetails.Where(e => e.CallStatusId == ConstantValues.callStatuses.IndexOf("Completed")).Count(),
				Count = -1,
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
			result = _db.CallStatuses
					 .GroupBy(g => g)
					 .Select(e => new CardViewModel
					 {
						 Category = e.Key.Name,
						 //Count = project.ProjectDetails.Where(x => x.CallStatusId == ConstantValues.callStatuses.IndexOf(e.Key)).Count(),
						 Count = -1,
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

			//result = project.ProjectDetails
			//	.Where(e => e.CallStatusId == ConstantValues.callStatuses.IndexOf("Completed"))
			//	.GroupBy(g => DateOnly.FromDateTime(g.LastUpdateDate.GetValueOrDefault()))
			//	.Select(e => new CompletedQuotaPerDay
			//	{
			//		Date = e.Key,
			//		Count = e.Count()
			//	}).ToList();

			return result;
		}

		private List<TelemarketerProductivityCardViewModel> getTelemarketerProductivities()
		{
			List<TelemarketerProductivityCardViewModel> result = [];





			return result;
		}

		public ResultWithMessage getProjectStatistics(int projectId, DateTime dateFrom, DateTime dateTo, TenantDto authData)
		{
			var mainObj = _db.ProjectDetails
				.Include(e => e.Project)
				.Include(e => e.CallStatus)
				.Include(e => e.Employee)
				.Where(e => e.ProjectId == projectId)
				.GroupBy(g => new
				{
					projectName = g.Project.Name,
					addedOn = g.Project.AddedOn,
					createdBy = g.Project.CreatedBy,
					telemarketer = g.Employee.UserName,
					callStatus = g.CallStatus.Name,
					quota = g.Project.Quota
				})
				.Select(e => new
				{
					ProjectName = e.Key.projectName,
					AddedOn = e.Key.addedOn,
					CreatedBy = e.Key.createdBy,
					Telemarketer = e.Key.telemarketer,
					CallStatus = e.Key.callStatus,
					Quota = e.Key.quota,
					AssignedGsms = e.Count()
				})
				.ToList();


			if (mainObj is null)
				return new ResultWithMessage(null, "No Data Found");

			//////1) GeneralDetails
			int totalGSMs = mainObj.Select(e => e.AssignedGsms).Sum();
			int closedGSMs = mainObj.Where(e => e.CallStatus == "Completed").Select(e => e.AssignedGsms).Sum();
			int telemarketerCount = mainObj.Select(e => e.Telemarketer).Distinct().Count();

			//////2) GSMSCountPerCallStatues
			List<CardViewModel> gsmsCountPerCallStatues = mainObj
				.GroupBy(g => g.CallStatus)
				.Select(e => new CardViewModel
				{
					Category = e.Key,
					Count = e.Sum(x => x.AssignedGsms),
					Total = totalGSMs
				}).ToList();

			/////3) Telemarketer productivity
			List<TelemarketerProductivityCardViewModel> telemarketerProductivities = mainObj.GroupBy(g => g.Telemarketer)
				.Select(e => new TelemarketerProductivityCardViewModel()
				{
					Telemarketer = e.Key,
					AssignedGSMs = e.Sum(e => e.AssignedGsms),
					Closed = e.Count(e => e.CallStatus == "Completed"),
					Completed = e.Count(e => e.CallStatus == "Completed"),

					CompletedRate = Convert.ToDouble(e.Count(e => e.CallStatus == "Completed")) / Convert.ToDouble(e.Sum(e => e.AssignedGsms)),
					ClosedRate = Convert.ToDouble(e.Count(e => e.CallStatus == "Completed")) / Convert.ToDouble(e.Sum(e => e.AssignedGsms))
				}).ToList();


			///////4) Quota Progress -> closed by date
			List<CompletedQuotaPerDay> quotaProgress = _db.ProjectDetails.
				Where(e => e.ProjectId == projectId && e.CallStatusId == 3 && !e.IsDeleted && e.LastUpdateDate != null && e.LastUpdateDate.Value.Date >= dateFrom.Date && e.LastUpdateDate.Value.Date <= dateTo.Date)
				.GroupBy(g => g.LastUpdateDate.Value.Date)
				.Select(e => new CompletedQuotaPerDay()
				{
					Date = e.Key.Date,
					Count = e.Count()
				}).ToList();

			///////5) Result
			ProjectStatisticsViewModel result = new()
			{
				ProjectName = mainObj[0].ProjectName,
				AddedOn = mainObj[0].AddedOn,
				CreatedBy = mainObj[0].CreatedBy,
				GeneralDetails = [
					new CardViewModel {Category = "Completed",Count=closedGSMs,Total =totalGSMs},
					new CardViewModel {Category = "Non-Completed",Count=totalGSMs -  closedGSMs,Total =totalGSMs},
					new CardViewModel {Category = "Quota Progress",Count=closedGSMs,Total =mainObj[0].Quota},
					new CardViewModel {Category = "Telemarketer",Count=telemarketerCount,Total=0}
				],
				CallStatuses = gsmsCountPerCallStatues,
				TelemarketerProductivities = telemarketerProductivities,
				CompletedQuotaPerDays = quotaProgress
			};

			return new ResultWithMessage(result, string.Empty);
		}
	}
}