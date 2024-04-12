using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models.Auth
{
	public class TenantDevice
	{
		[Key]
		public int Id { get; set; }
		[Required]
		[ForeignKey("Tenant")]
		public int TenantId { get; set; }
		[Required]
		[ForeignKey("Device")]
		public int DeviceId { get; set; }

		public virtual Tenant Tenant { get; set; }
		public virtual Device Device { get; set; }
	}
}
