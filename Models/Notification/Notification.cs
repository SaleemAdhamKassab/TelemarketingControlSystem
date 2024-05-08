using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static TelemarketingControlSystem.Models.Notification.NotificationType;

namespace TelemarketingControlSystem.Models.Notification
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public NotType Type { get;set; }
        [ForeignKey("Project")]
        public int ? ProjectId { get; set; }
        public string Message { get; set;}
        public string ? UserName { get; set; }
        public string ? connectionId { get; set; }
        public DateTime CreatedDate { get; set; }=DateTime.Now;
        public bool IsRead { get; set; }
        public string ? Img { get; set; }

        public virtual Project Project { get; set; }

        public Notification() { }
        public Notification(string title, int? projectId, string message, string? userName, string? connectionid ,string ? img)
        {
            Title=title;
            ProjectId=projectId;
            Message=message;
            UserName=userName;
            connectionId=connectionid;
            Type = NotType.CreateNewProject;
            CreatedDate = DateTime.Now;
            IsRead = false;
            Img = img;
        }

    }
}
