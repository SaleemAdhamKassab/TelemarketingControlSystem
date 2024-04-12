﻿using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models.Auth
{
	public class AccessLog
	{
		[Key]
		public int Id { get; set; }
		public string UserName { get; set; }
		public string IP { get; set; }
		public string ResponseMessage { get; set; }
		public DateTime ActionDate { get; set; }

		public AccessLog() { }
		public AccessLog(string userName, string iP, string responseMessage, DateTime actionDate)
		{
			UserName = userName != null ? userName : "NA";
			IP = iP;
			ResponseMessage = responseMessage;
			ActionDate = actionDate;
		}
	}
}
