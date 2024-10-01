using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.MistakeReportService;

namespace TelemarketingControlSystem.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MistakeReportsController : BaseController
	{
		private readonly IMistakeReportService _mistakeReportService;
		public MistakeReportsController(IMistakeReportService mistakeReportService)
		{
			_mistakeReportService = mistakeReportService;
		}

		[HttpGet("getMistakeDictionaryType")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public IActionResult getMistakeDictionaryType(int projectTypeId) => _returnResultWithMessage(_mistakeReportService.getMistakeDictionaryType(projectTypeId));
	}
}
