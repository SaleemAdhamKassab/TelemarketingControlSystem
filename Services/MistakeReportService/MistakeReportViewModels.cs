using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Services.ProjectEvaluationService;

namespace TelemarketingControlSystem.Services.MistakeReportService
{
	public class MistakeDictionaryTypeViewModel
	{
		public int Id { get; set; }
		public double RangFrom { get; set; }
		public double RangTo { get; set; }
		public double Value { get; set; }
		public bool IsDeleted { get; set; }
		public string CreatedBy { get; set; }
		public DateTime AddedOn { get; set; }
		public string? LastUpdatedBy { get; set; }
		public DateTime? LastUpdatedDate { get; set; }

		public int ProjectTypeId { get; set; }
		public string ProjectType { get; set; }
	}
	public class UpdateProjectTypeMistakeDictionaryDto
	{
		public int ProjectTypeId { get; set; }
		public List<DictionaryRange> DictionaryRanges { get; set; }
	}
	public class ProjectTypeMistakeDictionaryViewModel
	{
		public int Id { get; set; }
		public double RangFrom { get; set; }
		public double RangTo { get; set; }
		public double Value { get; set; }
		public bool IsDeleted { get; set; }
		public string CreatedBy { get; set; }
		public DateTime AddedOn { get; set; }
		public string? LastUpdatedBy { get; set; }
		public DateTime? LastUpdatedDate { get; set; }

		public int ProjectTypeId { get; set; }
		public string ProjectType { get; set; }
	}
	public class UploadMistakeReportRequest
	{
		public IFormFile MistakeReport { get; set; }
	}
	public class ExcelMistakeReport
	{
		public string SurveyName { get; set; }
		public string TelemarketerName { get; set; }
		public string MistakeType { get; set; }
		public string GSM { get; set; }
		public string Serial { get; set; }
		public string QuestionNumber { get; set; }
		public string Segment { get; set; }
		public string Controller { get; set; }
	}
	public class MistakeReportRequest
	{
		public List<int> ProjectIds { get; set; }
		public List<int> TelemarketerIds { get; set; }
		public List<string> MistakeTypes { get; set; }
		public GeneralFilterModel Filter { get; set; }
	}
	public class MistakeReportResponse
	{
		public string ProjectName { get; set; }
		public string TelemarketerName { get; set; }
		public string MistakeType { get; set; }
		public string GSM { get; set; }
		public string Serial { get; set; }
		public string QuestionNumber { get; set; }
		public string Segment { get; set; }
		public string MistakeDescription { get; set; }
		public double MistakeWeight { get; set; }
		public string Controller { get; set; }
	}

	public class MistakeTypeResponse
	{
		public string Name { get; set; }
		public double Weight { get; set; }
		public string Description { get; set; }
	}
	public class TeamMistakeReportRequest
	{
		public List<int> ProjectsIds { get; set; } = [];
		public List<int> TelemarketersIds { get; set; } = [];
		public GeneralFilterModel Filter { get; set; }
	}
	public class TeamMistakeReportResponse
	{
		public string projectName { get; set; } = string.Empty;
		public string Telemarketer { get; set; } = string.Empty;
		public int CompletedQuestionnaire { get; set; }
		public int MistakesCount { get; set; }
		public decimal? MistakesPercentage { get; set; }
	}

	public class WeightVsSurveyReportRequest
	{
		public List<string> MistakeTypes { get; set; } = [];
		public List<int> EmployeeIds { get; set; } = [];
	}

	public class GetAllViewModel
	{
		public string MistakeType { get; set; } = string.Empty;
		public int Count { get; set; }
	}
}