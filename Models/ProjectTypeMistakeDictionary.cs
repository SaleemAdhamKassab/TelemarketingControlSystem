using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class ProjectTypeMistakeDictionary: BaseModel
	{
		public double RangFrom { get; set; }
		public double RangTo { get; set; }
		public double Value { get; set; }

		public int ProjectTypeId { get; set; }
		public ProjectType ProjectType { get; set; }
	}
}
