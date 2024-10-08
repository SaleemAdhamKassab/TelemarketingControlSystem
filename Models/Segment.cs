using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class Segment
	{
		[Key]
		public string Name { get; set; }
		public bool IsDefault { get; set; }
		public string CreatedBy { get; set; }
		public DateTime AddedOn { get; set; }
		public bool IsDeleted { get; set; }

		public List<ProjectDetail> ProjectDetails { get; set; }
		public List<EmployeeWorkingHour> EmployeeWorkingHours { get; set; }
	}
}
