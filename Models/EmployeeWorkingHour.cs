using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelemarketingControlSystem.Models
{
	public class EmployeeWorkingHour
	{
		[Key]
		public int Id { get; set; }
		public DateTime Day { get; set; }
		public double WorkingHours { get; set; }


		public int ProjectId { get; set; }
		public Project Project { get; set; }
		public int EmployeeId { get; set; }
		public Employee Employee { get; set; }
		[ForeignKey("Segment")]
		public string? SegmentName { get; set; }
		public Segment Segment { get; set; }
	}
}
