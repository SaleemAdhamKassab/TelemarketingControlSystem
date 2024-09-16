using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.Auth;
using TelemarketingControlSystem.Services.ProjectsEvaluation;
using static TelemarketingControlSystem.Services.Auth.AuthModels;

namespace TelemarketingControlSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsEvaluationController : BaseController
    {
        private readonly IProjectsEvaluationService _projectsEvaluationService;
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _contextAccessor;

        private TenantDto authData()
        {
            string Header = _contextAccessor.HttpContext.Request.Headers["Authorization"];
            var token = Header.Split(' ').Last();
            TenantDto result = _jwtService.TokenConverter(token);
            if (result is null)
                return null;
            return result;
        }

        public ProjectsEvaluationController(IProjectsEvaluationService projectsEvaluationService, IJwtService jwtService, IHttpContextAccessor contextAccessor)
        {
            _projectsEvaluationService = projectsEvaluationService;
            _jwtService = jwtService;
            _contextAccessor = contextAccessor;
        }


        [HttpGet("getProjectTypeDictionary")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
        public IActionResult getProjectTypeDictionary(int projecTypeId) => _returnResultWithMessage(_projectsEvaluationService.getProjectTypeDictionary(projecTypeId));

        [HttpPut("updateProjectTypeDictionary")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
        public IActionResult updateProjectTypeDictionary(UpdateProjectTypeDictionaryDto updateProjectTypeDictionaryDto) => _returnResultWithMessage(_projectsEvaluationService.updateProjectTypeDictionary(updateProjectTypeDictionaryDto, authData()));
    }
}
