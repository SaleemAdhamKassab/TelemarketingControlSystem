using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class CallStatus:BaseModel
	{
		public string Name { get; set; }
		public bool IsClosed { get; set; }

		public List<ProjectDetail> ProjectDetails { get; set; }
	}
}
