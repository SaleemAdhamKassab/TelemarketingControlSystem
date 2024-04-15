using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class Project : BaseModel
	{
		[Key]
		public int Id { get; set; }
		[Required, MaxLength(50)]
		public string Name { get; set; }
		[Required]
		public DateTime DateFrom { get; set; }
		[Required]
		public DateTime DateTo { get; set; }
		[Required]
		public int Quota { get; set; }
		[Required]
		public int TypeId { get; set; }

		public List<ProjectDetail> ProjectDetails { get; set; }

		public virtual ICollection<Notification.Notification> Notifications { get; set; }
	}
}