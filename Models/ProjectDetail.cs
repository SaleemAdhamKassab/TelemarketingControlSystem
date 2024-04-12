using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class ProjectDetail : BaseModel
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string GSM { get; set; }
		public string? Segment { get; set; }
		public string? SubSegment { get; set; }
		public string? Bundle { get; set; }
		public string? Contract { get; set; }
		public string? AlternativeNumber { get; set; }
		public string? Note { get; set; }
		[Required]
		public int LineTypeId { get; set; }
		[Required]
		public int GenerationId { get; set; }
		[Required]
		public int RegionId { get; set; }
		[Required]
		public int CityId { get; set; }
		[Required]
		public int CallStatusId { get; set; }
		public int ProjectID { get; set; }
		public Project Project { get; set; }
		public int EmployeeID { get; set; }
		public Employee Employee { get; set; }
	}
}