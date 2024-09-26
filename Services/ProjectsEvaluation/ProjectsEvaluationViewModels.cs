namespace TelemarketingControlSystem.Services.ProjectsEvaluation
{
	public class ProjectTypeDictionaryViewModel
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

	public class UpdateProjectTypeDictionaryDto
	{
		public int ProjectTypeId { get; set; }
		public List<DictionaryRange> DictionaryRanges { get; set; }
	}
	public class DictionaryRange
	{
		public double RangFrom { get; set; }
		public double RangTo { get; set; }
		public double Value { get; set; }
	}

	public class ProjectDictionaryViewModel
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

		public int ProjectId { get; set; }
		public string Project { get; set; }
	}
	public class UpdateProjectDictionaryDto
	{
		public int projectId { get; set; }
		public List<DictionaryRange> DictionaryRanges { get; set; }
	}
	public class ProjectSegmentEvaluationCardDetails
	{
		public string CardTitle { get; set; }
		public double Value { get; set; }
	}
	public class TelemarketerSegmentEvaluationViewModel
	{
		public int TelemarketerId { get; set; }
		public string TelemarketerFullName { get; set; }
		public string TelemarketerUserName { get; set; }
		public string Segment { get; set; }
		public double WorkingHours { get; set; }
		public int Closed { get; set; }

	}
	public class ProjectSegmentTelemarketersEvaluationsDto
	{
		public int ProjectId { get; set; }
		public string SegmentName { get; set; }
		//public GeneralFilterModel Filter { get; set; }
	}
	public class ProjectSegmentTelemarketersEvaluationsViewModel
	{
		public string EmployeeUserName { get; set; }
		public string Segment { get; set; }
		public double WorkingHours { get; set; }
		public double Closed { get; set; }
		public double ClosedPerHour { get; set; }
		public double SegmentTarget { get; set; }
		public double Achievement { get; set; }
		public double? Mark { get; set; }
	}
}