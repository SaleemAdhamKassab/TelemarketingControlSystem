using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.Auth;
using TelemarketingControlSystem.Services.Projects;
using static TelemarketingControlSystem.Services.Auth.AuthModels;

namespace TelemarketingControlSystem.Controllers
{

	public class ProjectsController : BaseController
	{
		private readonly IProjectService _projectService;
		private readonly IJwtService _jwtService;
		private readonly IHttpContextAccessor _contextAccessor;

		public ProjectsController(IProjectService projectService, IJwtService jwtService, IHttpContextAccessor contextAccessor)
		{
			_projectService = projectService;
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

		[HttpGet("getProjectTypes")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
		public IActionResult getProjectTypes() => _returnResultWithMessage(_projectService.getProjectTypes());


		[HttpGet("getLineTypes")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
		public IActionResult getLineTypes() => _returnResultWithMessage(_projectService.getLineTypes());

		[HttpGet("getRegions")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
		public IActionResult getRegions() => _returnResultWithMessage(_projectService.getRegions());

		[HttpGet("getCities")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
		public IActionResult getCities() => _returnResultWithMessage(_projectService.getCities());

		[HttpGet("getCallStatuses")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
		public IActionResult getCallStatuses() => _returnResultWithMessage(_projectService.getCallStatuses());

		[HttpGet("getEmployees")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
		public IActionResult getEmployees() => _returnResultWithMessage(_projectService.getEmployees());

		[HttpGet("getLineGenerations")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
		public IActionResult getLineGenerations() => _returnResultWithMessage(_projectService.getLineGenerations());

		[HttpPost("getById")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
		public IActionResult getById(int id, [FromBody] ProjectFilterModel filter) => _returnResultWithMessage(_projectService.getById(id, filter, authData()));


		[HttpPost("getByFilter")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
		public IActionResult getByFilter([FromBody] ProjectFilterModel filter) => _returnResultWithMessage(_projectService.getByFilter(filter, authData()));

		[HttpPost("create")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		[DisableRequestSizeLimit]
		public async Task<IActionResult> create([FromForm] CreateProjectViewModel model) => _returnResultWithMessage(await _projectService.create(model, authData()));

		//[HttpPut("update")]
		//[TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
		//[DisableRequestSizeLimit]
		//public async Task<IActionResult> update(UpdateProjectViewModel model) => _returnResultWithMessage(await _projectService.update(model, authData()));

		[HttpDelete("delete")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public async Task<IActionResult> delete(int id) => _returnResultWithMessage(await _projectService.delete(id, authData()));

		[HttpGet("reDistributeProjectGSMs")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public async Task<IActionResult> reDistributeProjectGSMs(int projectId, string EmployeeIds) => _returnResultWithMessage(await _projectService.reDistributeProjectGSMs(projectId, EmployeeIds, authData()));

		[HttpPut("updateProjectDetail")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
		public async Task<IActionResult> updateProjectDetail(ProjectDetailViewModel model) => _returnResultWithMessage(await _projectService.updateProjectDetail(model, authData()));
	}
}