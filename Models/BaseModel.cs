namespace TelemarketingControlSystem.Models
{
	public class BaseModel
	{
		public string CreatedBy { get; set; }
		public DateTime AddedOn { get; set; }
		public string? LastUpdatedBy { get; set; }
		public DateTime? LastUpdateDate { get; set; }
		public bool IsDeleted { get; set; }
	}
}