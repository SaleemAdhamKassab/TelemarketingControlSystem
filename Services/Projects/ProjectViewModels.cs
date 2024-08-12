using TelemarketingControlSystem.Helper;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;

namespace TelemarketingControlSystem.Services.Projects
{
	public class EmployeeViewModel
	{
		[Required]
		public int Id { get; set; }
		[Required, MaxLength(50)]
		public string UserName { get; set; }
	}
	public class ProjectViewModel
	{
		[Required]
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
		public string CreatedBy { get; set; }
		[Required]
		public int TypeId { get; set; }
		[Required]
		public string Type { get; set; }
		public List<ProjectDetailViewModel> ProjectDetails { get; set; }
	}
	public class SharedProjectDetailsAndGSMExcel
	{
		[Required]
		public string GSM { get; set; }
		public string? Segment { get; set; }
		public string? SubSegment { get; set; }
		public string? Bundle { get; set; }
		public string? Contract { get; set; }
		public string? AlternativeNumber { get; set; }
		public string? Note { get; set; }
		public string? LineType { get; set; }
		public string? Generation { get; set; }
		public string? Region { get; set; }
		public string? City { get; set; }
		public string? CallStatus { get; set; } = "";
	}
	public class ProjectDetailViewModel : SharedProjectDetailsAndGSMExcel
	{
		[Required]
		public int Id { get; set; }
		[Required]
		public int EmployeeID { get; set; }
		[Required]
		public string EmployeeUserName { get; set; }
		public int? LineTypeId { get; set; }
		public int? GenerationId { get; set; }
		public int? RegionId { get; set; }
		public int? CityId { get; set; }
		public int CallStatusId { get; set; }
        public DateTime? LastUpdateDate { get; set; }
    }
	public class GSMExcel : SharedProjectDetailsAndGSMExcel { }
	public class UpsertProjectViewModel
	{
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
	}
	public class CreateProjectViewModel : UpsertProjectViewModel
	{
		[Required]
		public IFormFile GSMsFile { get; set; }

		[Required]
		public string EmployeeIDs { get; set; }
	}
	public class UpdateProjectViewModel : UpsertProjectViewModel
	{
		[Required]
		public int Id { get; set; }

		[Required]
		public List<ProjectDetailViewModel> ProjectDetails { get; set; }
	}
	public class ProjectFilterModel : GeneralFilterModel
	{
		public DateTime? DateFrom { get; set; }
		public DateTime? DateTo { get; set; }
		public string[]? CreatedBy { get; set; }
		public int[]? TypeIds { get; set; }

	}
	public class ListViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}