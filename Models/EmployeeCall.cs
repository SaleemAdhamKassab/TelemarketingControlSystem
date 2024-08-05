using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models
{
    public class EmployeeCall
    {
        [Key]
        public int Id { get; set; }
        public string GSM { get; set; }
        public DateTime CallStartDate { get; set; }
        public int DurationInSeconds { get; set; }
        public DateTime AddedOn { get; set; }
        public string CallType { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
}