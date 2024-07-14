using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class CallStatus
	{
		[Key]
		public int Id { get; set; }

		public string Name { get; set; }

		public List<ProjectDetail> ProjectDetails { get; set; }
	}
}
