using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.Auth;
using TelemarketingControlSystem.Services.ProjectEvaluationService;
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
		public IActionResult getProjectTypeDictionary(int projectTypeId) => _returnResultWithMessage(_projectsEvaluationService.getProjectTypeDictionary(projectTypeId));

		[HttpPut("updateProjectTypeDictionary")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public IActionResult updateProjectTypeDictionary(UpdateProjectTypeDictionaryDto updateProjectTypeDictionaryDto) => _returnResultWithMessage(_projectsEvaluationService.updateProjectTypeDictionary(updateProjectTypeDictionaryDto, authData()));

		[HttpGet("getProjectDictionary")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public IActionResult getProjectDictionary(int projectId) => _returnResultWithMessage(_projectsEvaluationService.getProjectDictionary(projectId));

		[HttpPut("updateProjectDictionary")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public IActionResult updateProjectDictionary(UpdateProjectDictionaryDto updateProjectDictionaryDto) => _returnResultWithMessage(_projectsEvaluationService.updateProjectDictionary(updateProjectDictionaryDto, authData()));

		[HttpGet("getProjectSegmentEvaluationCards")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public IActionResult getProjectSegmentEvaluationCards(int projectId, string segmentName) => _returnResultWithMessage(_projectsEvaluationService.getProjectSegmentEvaluationCards(projectId, segmentName));

		[HttpPost("getProjectSegmentTelemarketersEvaluations")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public async Task<IActionResult> getProjectSegmentTelemarketersEvaluations(ProjectSegmentTelemarketersEvaluationsDto dto) => _returnResultWithMessage(_projectsEvaluationService.getProjectSegmentTelemarketersEvaluations(dto));
	}
}