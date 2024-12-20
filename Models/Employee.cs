﻿using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class Employee : BaseModel
	{
		[Required, MaxLength(50)]
		public string UserName { get; set; }
		public string Name { get; set; }
		public int SyriatelId { get; set; }
		public bool IsActive { get; set; }

		public List<ProjectDetail> ProjectDetails { get; set; }
		public List<EmployeeCall> EmployeeCalls { get; set; }
		public List<EmployeeWorkingHour> EmployeeWorkingHours { get; set; }
		public List<MistakeReport> MistakeReports { get; set; }
	}
}
