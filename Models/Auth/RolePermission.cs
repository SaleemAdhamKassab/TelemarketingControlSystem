using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models.Auth
{
	public class RolePermission
	{
		[Key]
		public int Id { get; set; }
		[ForeignKey("Role")]
		public int RoleId { get; set; }
		[ForeignKey("Permission")]
		public int PermissionId { get; set; }

		public virtual Role Role { get; set; }
		//public virtual Permission Permission { get; set; }
	}
}
