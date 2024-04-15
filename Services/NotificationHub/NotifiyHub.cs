using Microsoft.AspNetCore.SignalR;

namespace TelemarketingControlSystem.Services.NotificationHub
{
    public class NotifiyHub : Hub<INotificationService>
    {
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

      


    }
}
