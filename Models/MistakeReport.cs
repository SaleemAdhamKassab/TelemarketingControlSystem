using System.ComponentModel.DataAnnotations.Schema;

namespace TelemarketingControlSystem.Models
{
	public class MistakeReport : BaseModel
	{
		public string GSM { get; set; }
		public string Serial { get; set; }
		public string QuestionNumber { get; set; }
		public string Segment { get; set; }
		public string Controller { get; set; }


		public int ProjectId { get; set; }
		public Project Project { get; set; }

		public int EmployeeId { get; set; }
		public Employee Employee { get; set; }

		[ForeignKey("MistakeType")]
		public string? MistakeTypeName { get; set; }
		public MistakeType MistakeType { get; set; }
	}
}
