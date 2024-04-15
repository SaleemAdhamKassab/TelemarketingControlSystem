using TelemarketingControlSystem.Services.NotificationHub.ViewModel;

namespace TelemarketingControlSystem.Services.NotificationHub
{
    public interface INotificationService
    {
         Task SendMessage(NotificationDto notification);


    }
}
