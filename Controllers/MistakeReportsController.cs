using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.Auth;
using TelemarketingControlSystem.Services.MistakeReportService;
using TelemarketingControlSystem.Services.ProjectEvaluationService;
using static TelemarketingControlSystem.Services.Auth.AuthModels;

namespace TelemarketingControlSystem.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MistakeReportsController : BaseController
	{
		private readonly IMistakeReportService _mistakeReportService;
		private readonly IJwtService _jwtService;
		private readonly IHttpContextAccessor _contextAccessor;

		public MistakeReportsController(IMistakeReportService mistakeReportService, IJwtService jwtService, IHttpContextAccessor contextAccessor)
		{
			_mistakeReportService = mistakeReportService;
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

		[HttpGet("projectTypeMistakeDictionary")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public IActionResult projectTypeMistakeDictionary(int projectTypeId) => _returnResultWithMessage(_mistakeReportService.projectTypeMistakeDictionary(projectTypeId));


		[HttpPut("updateProjectTypeMistakeDictionary")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public IActionResult updateProjectTypeMistakeDictionary(UpdateProjectTypeMistakeDictionaryDto dto) => _returnResultWithMessage(_mistakeReportService.updateProjectTypeMistakeDictionary(dto, authData()));


		[HttpGet("projectMistakeDictionary")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public IActionResult projectMistakeDictionary(int projectId) => _returnResultWithMessage(_mistakeReportService.projectMistakeDictionary(projectId));

		[HttpPut("updateProjectMistakeDictionary")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public IActionResult updateProjectMistakeDictionary(UpdateProjectMistakeDictionaryDto dto) => _returnResultWithMessage(_mistakeReportService.updateProjectMistakeDictionary(dto, authData()));


		[HttpPost("UploadMistakeReport")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public async Task<IActionResult> UploadMistakeReport(UploadMistakeReportRequest dto)
		{
			var result = await _mistakeReportService.UploadMistakeReportAsync(dto, authData());

			if (!string.IsNullOrEmpty(result.Message))
				return BadRequest(new { message = result.Message });

			return Ok(result.Data);
		}

		[HttpPost("MistakeReport")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public async Task<IActionResult> MistakeReport(MistakeReportRequest request)
		{
			var result = await _mistakeReportService.GetMistakeReportAsync(request);

			if (!string.IsNullOrEmpty(result.Message))
				return BadRequest(new { message = result.Message });

			return Ok(result.Data);
		}

		[HttpGet("MistakeTypes")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public async Task<IActionResult> MistakeTypes()
		{
			var result = await _mistakeReportService.GetMistakeTypesAsync();

			if (!string.IsNullOrEmpty(result.Message))
				return BadRequest(new { message = result.Message });

			return Ok(result.Data);
		}


		[HttpPost("GetTeamMistakeReport")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public async Task<IActionResult> GetMistakeTypes(TeamMistakeReportRequest request)
		{
			var result = await _mistakeReportService.GetTeamMistakeReportAsync(request);

			if (!string.IsNullOrEmpty(result.Message))
				return BadRequest(new { message = result.Message });

			return Ok(result.Data);
		}

		[HttpPost("WeightVsSurveyReport")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public async Task<IActionResult> WeightVsSurveyReport(WeightVsSurveyReportRequest request)
		{
			var result = await _mistakeReportService.GetWeightVsSurveyReportAsync(request);

			if (!string.IsNullOrEmpty(result.Message))
				return BadRequest(new { message = result.Message });

			return Ok(result.Data);
		}
	}
}
