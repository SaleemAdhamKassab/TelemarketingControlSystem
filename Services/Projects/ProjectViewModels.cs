using MathNet.Numerics.Statistics.Mcmc;
using TelemarketingControlSystem.Helper;
using System.ComponentModel.DataAnnotations;
using static TelemarketingControlSystem.Helper.ConstantValues;

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
		[Required]
		public string LineType { get; set; }
		[Required]
		public string Generation { get; set; }
		[Required]
		public string Region { get; set; }
		[Required]
		public string City { get; set; }
		[Required]
		public string CallStatus { get; set; }
	}
	public class ProjectDetailViewModel : SharedProjectDetailsAndGSMExcel
	{
		[Required]
		public int Id { get; set; }
		[Required]
		public int EmployeeID { get; set; }
		[Required]
		public string EmployeeUserName { get; set; }
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
	public class ProjectFilterModel : GeneralFilterModel { }
	public class ListViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}
}