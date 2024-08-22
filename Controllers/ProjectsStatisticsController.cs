using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.Auth;
using TelemarketingControlSystem.Services.ProjectStatistics;
using static TelemarketingControlSystem.Services.Auth.AuthModels;
using static TelemarketingControlSystem.Services.ProjectStatistics.ProjectStatisticsViewModels;

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

        [HttpPost("generalReport")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer,Researcher,Segmentation"])]
        public IActionResult generalReport(GeneralReportDto generalReportDto) => _returnResultWithMessage(_projectStatisticsService.generalReport(generalReportDto));

        [HttpPost("hourlyTarget")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
        public IActionResult hourlyTarget(HourlyTargetDto hourlyTargetDto) => _returnResultWithMessage(_projectStatisticsService.hourlyTarget(hourlyTargetDto));
    }
}