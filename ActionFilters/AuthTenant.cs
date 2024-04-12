﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TelemarketingControlSystem.Models.Auth;
using TelemarketingControlSystem.Models.Data;
using TelemarketingControlSystem.Services.Auth;

namespace TelemarketingControlSystem.ActionFilters
{
	public class AuthTenant: Attribute, IActionFilter
	{
		private readonly IJwtService _jwtService;
		private readonly ApplicationDbContext _dbContext;
		private string? roleNames { get; set; }
		public AuthTenant(ApplicationDbContext dbContext, IJwtService jwtService, string? RoleNames)
		{
			_dbContext = dbContext;
			_jwtService = jwtService;
			roleNames = RoleNames;
		}

		public void OnActionExecuted(ActionExecutedContext context)
		{

		}

		public void OnActionExecuting(ActionExecutingContext context)
		{
			List<string> userRoleNames = roleNames.Split(',').ToList();
			string Header = context.HttpContext.Request.Headers["Authorization"];
			var clientIPAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();

			if (context.HttpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
			{
				clientIPAddress = forwardedFor.ToString().Split(", ")[0];
			}

			//Check auth header
			if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
			{
				// The "Authorization" header is missing, so return a 401 Unauthorized response
				AccessLog log = new AccessLog("NA", clientIPAddress, "Token is null in the header request.", DateTime.Now);
				_dbContext.AccessLogs.Add(log);
				_dbContext.SaveChanges();
				context.Result = new BadRequestObjectResult("Please Contact to administrator. ");
				return;
			}

			var token = authHeader.FirstOrDefault()?.Split(' ').Last();
			if (token == null)
			{

				AccessLog log = new AccessLog("NA", clientIPAddress, "Token is null in the header request.", DateTime.Now);
				_dbContext.AccessLogs.Add(log);
				_dbContext.SaveChanges();
				context.Result = new BadRequestObjectResult("Please Contact to administrator. ");
				return;
			}
			// check token and refresh token
			if (_jwtService.CheckExpiredToken(token))
			{

				if (_jwtService.IsGrantAccess(token, userRoleNames))
				{
					return;

				}

				context.Result = new BadRequestObjectResult("Access Denied");
				return;


			}



			context.Result = new BadRequestObjectResult("Token Expired");
			return;
		}

		
	}
}
