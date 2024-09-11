using Microsoft.AspNetCore.Mvc;
using TelemarketingControlSystem.Helper;

namespace TelemarketingControlSystem.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BaseController : ControllerBase
	{
		protected IActionResult _returnResultWithMessage(ResultWithMessage result)
		{
			if (string.IsNullOrEmpty(result.Message))
				return Ok(result.Data);

			return BadRequest(new { message = result.Message });
		}
	}
}
