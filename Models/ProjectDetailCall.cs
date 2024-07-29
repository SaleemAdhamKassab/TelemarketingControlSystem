using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class ProjectDetailCall
	{
		[Key]
		public int Id { get; set; }
		public DateTime CallStartDate { get; set; }
		public int DurationInSeconds { get; set; }
		public DateTime AddedOn { get; set; }

		public int ProjectDetailId { get; set; }
		public ProjectDetail ProjectDetail { get; set; }
	}
}
