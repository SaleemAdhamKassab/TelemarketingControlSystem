using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Models.Auth;
using TelemarketingControlSystem.Models.Notification;

namespace TelemarketingControlSystem.Models.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<ProjectDetail> ProjectDetails { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<CallStatus> CallStatuses { get; set; }
        public DbSet<EmployeeCall> EmployeeCalls { get; set; }
        public DbSet<ProjectDetailCall> ProjectDetailCalls { get; set; }
        public DbSet<Segment> Segments { get; set; }
        public DbSet<ProjectType> ProjectTypes { get; set; }
        public DbSet<TypeDictionary> TypeDictionaries { get; set; }
        public DbSet<ProjectDictionary> ProjectDictionaries { get; set; }

        //Auth Models
        public DbSet<AccessLog> AccessLogs { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<GroupTenantRole> GroupTenantRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantDevice> TenantDevices { get; set; }
        public DbSet<UserTenantRole> UserTenantRoles { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        //-----------------------Notification--------------------------
        public DbSet<HubClient> HubClients { get; set; }
        public DbSet<Notification.Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Project>().ToTable(e => e.HasCheckConstraint("Ck_Project_Dates", "[DateTo]>[DateFrom]"));
        }
    }
}