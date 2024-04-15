using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Models.Data;
using TelemarketingControlSystem.Models.Notification;

namespace TelemarketingControlSystem.Services.NotificationHub
{

    public interface IHubService
    {
        Task UpdateHubClient(string userName,string connectionId);
    }
    public class HubService : IHubService
    {
        private readonly ApplicationDbContext _db;

        public HubService(ApplicationDbContext db)
        {
            _db = db;
        }

        public  Task UpdateHubClient(string userName, string connectionId)
        {
            var client=  _db.HubClients.AsNoTracking().FirstOrDefault(x=>x.userName== userName);
            HubClient item = new HubClient();

            if (client!=null)
            {
                item.connectionId = connectionId;
                item.userName = userName;
                item.Id = client.Id;
                _db.HubClients.Update(item);
                _db.SaveChanges();
               return Task.CompletedTask;
            }
            item.connectionId = connectionId;
            item.userName = userName;
            _db.HubClients.Add(item);
            _db.SaveChanges();
            return Task.CompletedTask;


        }
    }
}
