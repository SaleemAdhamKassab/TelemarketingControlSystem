﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models.Auth
{
	public class UserTenantRole
	{
		[Key]
		public int Id { get; set; }
		[Required]
		[StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 char")]
		public string UserName { get; set; }
		[ForeignKey("Tenant")]
		public int? TenantId { get; set; }
		[Required]
		[ForeignKey("Role")]
		public int RoleId { get; set; }

		public virtual Role Role { get; set; }
		public virtual Tenant? Tenant { get; set; }
	}
}
