using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class BaseModel
	{
		[Key]
		public int Id { get; set; }
		public string CreatedBy { get; set; }
		public DateTime AddedOn { get; set; }
		public string? LastUpdatedBy { get; set; }
		public DateTime? LastUpdatedDate { get; set; }
		public bool IsDeleted { get; set; }
	}
}