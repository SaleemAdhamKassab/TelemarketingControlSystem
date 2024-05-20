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

		public ResultWithMessage getProjectStatistics(int projectId, DateTime dateFrom, DateTime dateTo, TenantDto authData)
		{
			if (!authData.tenantAccesses[0].RoleList.Contains("Admin"))
				return new ResultWithMessage(null, "insufficient privileges");

			Project project = _db.Projects.Where(e => e.Id == projectId).Include(e => e.ProjectDetails.Where(e=>e.LastUpdateDate>=dateFrom && e.LastUpdateDate<=dateTo )).FirstOrDefault();

			if (project is null)
				return new ResultWithMessage(null, string.Empty);



			ProjectStatisticsViewModel result = new()
			{
				ProjectName = project.Name,
				CreatedBy = project.CreatedBy,
				TotalGSMCount = project.ProjectDetails.Count(),
				Quota = project.Quota,
				DateFrom = project.DateFrom,
				DateTo = project.DateTo,
				GSMStatusStatistics = project.ProjectDetails
										.GroupBy(g => g.CallStatusId).
										Select(e => new GSMStatusStatistic
										{
											Status = ConstantValues.callStatuses.ElementAt(e.Key.Value - 1),
											GSMCount = e.Count()

										}).ToList()
			};

			return new ResultWithMessage(result, string.Empty);
		}
	}
}