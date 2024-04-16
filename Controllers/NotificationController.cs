using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.ActionFilters;
using TelemarketingControlSystem.Services.Auth;
using TelemarketingControlSystem.Services.NotificationHub;
using static TelemarketingControlSystem.Services.Auth.AuthModels;

namespace TelemarketingControlSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly IJwtService _jwtService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IHubService _hubService;
        public NotificationController(IJwtService jwtService , IHttpContextAccessor contextAccessor, IHubService hubService)
        {
            _contextAccessor = contextAccessor;
            _jwtService = jwtService;
            _hubService= hubService;
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


        [HttpGet("UpdateHubClient")]
        public IActionResult UpdateHubClient(string connectionId)
        {
            var user = authData();
            return Ok(_hubService.UpdateHubClient(user.userName,connectionId));
        }

        [HttpGet("ReadNotification")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]

        public IActionResult ReadNotification(int id)
        {
            return Ok(_hubService.ReadNotification(id));
        }

        [HttpGet("GetUserNotification")]
        [TypeFilter(typeof(AuthTenant), Arguments = ["Admin,Telemarketer"])]

        public IActionResult GetUserNotification()
        {
            var user = authData();
            return Ok(_hubService.GetRecentlyNotification(user.userName));
        }

    }
}
