using System.ComponentModel.DataAnnotations;

namespace TelemarketingControlSystem.Models.Notification
{
    public class HubClient
    {
        [Key]
        public int Id { get; set; }
        public string userName { get;set; }
        public string connectionId { get;set; }

    }
}
