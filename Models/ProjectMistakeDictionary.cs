using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class ProjectMistakeDictionary : BaseModel
	{
		public double RangFrom { get; set; }
		public double RangTo { get; set; }
		public double Value { get; set; }

		public int ProjectId { get; set; }
		public Project Project { get; set; }
	}
}
