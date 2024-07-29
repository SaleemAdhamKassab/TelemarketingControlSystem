using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.Auth;
using TelemarketingControlSystem.Services.ProjectStatistics;
using static TelemarketingControlSystem.Services.Auth.AuthModels;

namespace TelemarketingControlSystem.Controllers
{

	public class ProjectsStatisticsController : BaseController
	{
		private readonly IProjectStatisticsService _projectStatisticsService;
		private readonly IJwtService _jwtService;
		private readonly IHttpContextAccessor _contextAccessor;

		public ProjectsStatisticsController(IProjectStatisticsService projectStatisticsService, IJwtService jwtService, IHttpContextAccessor contextAccessor)
		{
			_projectStatisticsService = projectStatisticsService;
			_jwtService = jwtService;
			_contextAccessor = contextAccessor;
		}

		private TenantDto authData()
		{
			string Header = _contextAccessor.HttpContext.Request.Headers["Authorization"];
			var token = Header.Split(' ').Last();
			TenantDto result = _jwtService.TokenConverter(token);
			if (result is null)
				return null;
			return result;
		}

		[HttpGet("getProjectStatistics")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public IActionResult getProjectStatistics(int projectId, DateTime dateFrom, DateTime dateTo) => _returnResultWithMessage(_projectStatisticsService.getProjectStatistics(projectId, dateFrom, dateTo, authData()));

		[HttpGet("hourlyTelemarketerTarget")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public IActionResult hourlyTelemarketerTarget(int projectId, int telemarketerId, DateTime targetDate, int hour) => _returnResultWithMessage(_projectStatisticsService.hourlyTelemarketerTarget(projectId, telemarketerId, targetDate, hour));
	}
}