using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
	public class MistakeType
	{
		[Key]
		public string Name { get; set; }
		public double Weight { get; set; }
		public string Description { get; set; }

		public List<MistakeReport> MistakeReports { get; set; }
	}
}
