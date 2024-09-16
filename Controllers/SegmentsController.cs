using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.Auth;
using TelemarketingControlSystem.Services.Segments;
using static TelemarketingControlSystem.Services.Auth.AuthModels;

namespace TelemarketingControlSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SegmentsController : BaseController
    {
        private readonly ISegmentsService _segmentsService;
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

        public SegmentsController(ISegmentsService segmentsService, IJwtService jwtService, IHttpContextAccessor contextAccessor)
        {
            _segmentsService = segmentsService;
            _jwtService = jwtService;
            _contextAccessor = contextAccessor;
        }

        [HttpGet("getSegments")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
        public IActionResult getSegments() => _returnResultWithMessage(_segmentsService.getSegments());

        [HttpPost("addSegment")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin"])]
        public IActionResult addSegment(SegmentDto segmentDto) => _returnResultWithMessage(_segmentsService.addSegment(segmentDto, authData()));
    }
}
