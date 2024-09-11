using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
    public class TypeDictionary
    {
        [Key]
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
        public ProjectType ProjectType { get; set; }
    }
}
