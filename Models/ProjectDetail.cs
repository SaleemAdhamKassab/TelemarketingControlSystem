using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelemarketingControlSystem.Models
{
    public class ProjectDetail : BaseModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string GSM { get; set; }
        public string? Region { get; set; }
        public string? LineType { get; set; }
        public string? Generation { get; set; }
        public string? City { get; set; }
        public string? SubSegment { get; set; }
        public string? Bundle { get; set; }
        public string? Contract { get; set; }
        public string? AlternativeNumber { get; set; }
        public string? Note { get; set; }



        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public int CallStatusId { get; set; }
        public CallStatus CallStatus { get; set; }
        [ForeignKey("Segment")]
        public string? SegmentName { get; set; }
        public Segment Segment { get; set; }
        public List<ProjectDetailCall> ProjectDetailCalls { get; set; }
    }
}