
    using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.Auth;
using TelemarketingControlSystem.Services.ProjectService;
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

        [HttpGet("getRegions")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
        public IActionResult getRegions() => _returnResultWithMessage(_projectService.getRegions());

        [HttpGet("getCallStatuses")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
        public IActionResult getCallStatuses() => _returnResultWithMessage(_projectService.getCallStatuses());

        [HttpGet("getEmployees")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
        public IActionResult getEmployees() => _returnResultWithMessage(_projectService.getEmployees());

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

        [HttpPut("update")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> update(UpdateProjectViewModel model) => _returnResultWithMessage(await _projectService.update(model, authData()));

        [HttpDelete("delete")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
        public async Task<IActionResult> delete(int id) => _returnResultWithMessage(await _projectService.delete(id, authData()));

        [HttpGet("reDistributeProjectGSMs")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
        public async Task<IActionResult> reDistributeProjectGSMs(int projectId, string EmployeeIds) => _returnResultWithMessage(await _projectService.reDistributeProjectGSMs(projectId, EmployeeIds, authData()));

        [HttpPut("updateProjectDetail")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]
        public  IActionResult updateProjectDetail(ProjectDetailViewModel model) => _returnResultWithMessage( _projectService.updateProjectDetail(model, authData()));

        [HttpGet("exportProjectDetailsToExcel")]
        public IActionResult exportProjectDetailsToExcel(int projectId)
        {
            var excelData = _projectService.exportProjectDetailsToExcel(projectId, authData());
            if (!string.IsNullOrEmpty(excelData.Message))
                return BadRequest(excelData.Message);

            return File(excelData.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Project Details.xlsx");
        }

        [HttpGet("exportProjectsToExcel")]
        public IActionResult exportProjectsToExcel()
        {
            var excelData = _projectService.exportProjectsToExcel();
            if (!string.IsNullOrEmpty(excelData.Message))
                return BadRequest(excelData.Message);

            return File(excelData.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Projects.xlsx");
        }

		[HttpGet("getAdmins")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public async Task<IActionResult> getAdmins() => _returnResultWithMessage(_projectService.getAdmins());

		[HttpGet("expectedRemainingDays")]
		[TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
		public async Task<IActionResult> expectedRemainingDays(int projectId) => _returnResultWithMessage(_projectService.projectExpectedRemainingDays(projectId));
	}
}