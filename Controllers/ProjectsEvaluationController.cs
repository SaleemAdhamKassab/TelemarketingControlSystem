using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.ProjectsEvaluation;

namespace TelemarketingControlSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsEvaluationController(IProjectsEvaluationService projectsEvaluationService) : BaseController
    {
        private readonly IProjectsEvaluationService _projectsEvaluationService = projectsEvaluationService;


        [HttpGet("getProjectTypeDictionary")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
        public IActionResult getProjectTypeDictionary(int projecTypeId) => _returnResultWithMessage(_projectsEvaluationService.getProjectTypeDictionary(projecTypeId));
    }
}
