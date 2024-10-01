using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class TypeMistakeDictionary: BaseModel
	{
		[Key]
		public int Id { get; set; }
		public double RangFrom { get; set; }
		public double RangTo { get; set; }
		public double Value { get; set; }

		public int ProjectTypeId { get; set; }
		public ProjectType ProjectType { get; set; }
	}
}
