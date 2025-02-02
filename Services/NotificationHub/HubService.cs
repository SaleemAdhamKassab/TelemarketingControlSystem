using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models.Data;
using TelemarketingControlSystem.Models.Notification;
using TelemarketingControlSystem.Services.NotificationHub.ViewModel;

namespace TelemarketingControlSystem.Services.NotificationHub
{

    public interface IHubService
    {
        Task<ResultWithMessage> UpdateHubClient(string userName,string connectionId);
        ResultWithMessage GetRecentlyNotification(string userName);
        Task ReadNotification(int notificationId);
    }
    public class HubService : IHubService
    {
        private readonly ApplicationDbContext _db;

        public HubService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ResultWithMessage> UpdateHubClient(string userName, string connectionId)
        {
            try
            {
                var client = await _db.HubClients.AsNoTracking().FirstOrDefaultAsync(x => x.userName == userName);

                HubClient item = new HubClient();

                if (client != null)
                {
                    item.connectionId = connectionId;
                    item.userName = userName;
                    item.Id = client.Id;
                    _db.HubClients.Update(item);
                    await _db.SaveChangesAsync();
                    return new ResultWithMessage(null, null);
                }
                item.connectionId = connectionId;
                item.userName = userName;
                await _db.HubClients.AddAsync(item);
                await _db.SaveChangesAsync();
                return new ResultWithMessage(null, null);
            }
            catch(Exception ex)
            {
                return new ResultWithMessage(null,"Notifications : Please connect to system admin.");

            }



        }

        public ResultWithMessage GetRecentlyNotification(string userName)
        {
            var notifications = _db.Notifications.Where(x => x.UserName == userName && !x.IsRead)
                .OrderByDescending(x => x.CreatedDate);

           
            var result = notifications.Select(x => new NotificationListViewModel()
            {
                Id=x.Id,
                ProjectId = (int)x.ProjectId,
                ProjectName = x.Project.Name,
                Title=x.Title,
                Message=x.Message,
                Type=x.Type.GetDisplayName(),
                CreatedDate=x.CreatedDate,
                IsRead=x.IsRead,
                duration= DateTime.Now.Subtract(x.CreatedDate).ToString("hh\\:mm\\:ss"),
                Img=x.Img

            }).ToList();

            return new ResultWithMessage(result,null);

        }

        public Task ReadNotification(int notificationId)
        {
            var notification=_db.Notifications.AsNoTracking().FirstOrDefault(x=>x.Id==notificationId);
            if(notification!=null)
            {
                notification.IsRead = true;
                _db.Notifications.Update(notification);
                _db.SaveChanges();
            } 

            return Task.CompletedTask;
        }
    }
}
