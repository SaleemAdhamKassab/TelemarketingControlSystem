using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.Auth;
using TelemarketingControlSystem.Services.ProjectStatisticService;
using static TelemarketingControlSystem.Services.Auth.AuthModels;
using static TelemarketingControlSystem.Services.ProjectStatisticService.ProjectStatisticsViewModels;

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

      
        [HttpPost("generalReport")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer,Researcher"])]
        public IActionResult generalReport(GeneralReportDto generalReportDto) => _returnResultWithMessage(_projectStatisticsService.generalReport(generalReportDto));

        [HttpPost("hourlyTarget")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
        public IActionResult hourlyTarget(HourlyTargetDto hourlyTargetDto) => _returnResultWithMessage(_projectStatisticsService.hourlyTarget(hourlyTargetDto));
    }
}