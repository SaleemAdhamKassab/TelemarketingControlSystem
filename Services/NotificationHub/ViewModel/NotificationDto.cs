using TelemarketingControlSystem.Models.Notification;

namespace TelemarketingControlSystem.Services.NotificationHub.ViewModel
{
    public class NotificationDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Message { get; set; }

    }

    public class NotificationListViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string duration { get; set; }
    }


}
