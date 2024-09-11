using TelemarketingControlSystem.Models;

namespace TelemarketingControlSystem.Services.ProjectsEvaluation
{
    public class ProjectTypeDictionaryViewModel
    {
        public int Id { get; set; }
        public double RangFrom { get; set; }
        public double RangTo { get; set; }
        public string Value { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public string? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }

        public int ProjectTypeId { get; set; }
        public string ProjectType { get; set; }
    }
}
