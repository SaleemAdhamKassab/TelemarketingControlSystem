using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TelemarketingControlSystem.Helper.ExceptionHandling
{
	public class ExceptionHandlingMiddleware(RequestDelegate next)
	{
		private readonly RequestDelegate _next = next;

		private async Task HandleExceptionAsync(HttpContext httpContext, string errorMsg)
		{
			httpContext.Response.ContentType = "application/problem+json";
			httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

			ErrorMsgResponse errorMsgResponse = new()
			{
				Message = errorMsg
			};
			var result = JsonConvert.SerializeObject(errorMsgResponse);
			await httpContext.Response.WriteAsync(result);
		}

		public async Task InvokeAsync(HttpContext httpContext)
		{
			try
			{
				await _next(httpContext);
			}
			catch (Exception e)
			{
				string errorMsg = e.Message;
				await HandleExceptionAsync(httpContext, errorMsg);
			}
		}
	}
}
