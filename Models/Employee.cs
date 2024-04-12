using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class Employee : BaseModel
	{
		[Key]
		public int Id { get; set; }
		[Required, MaxLength(50)]
		public string UserName { get; set; }
		public List<ProjectDetail> ProjectDetails { get; set; }
	}
}
