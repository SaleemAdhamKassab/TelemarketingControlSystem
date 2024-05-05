using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models;
using TelemarketingControlSystem.Models.Data;
using System.Data;
using static TelemarketingControlSystem.Helper.ConstantValues;
using static TelemarketingControlSystem.Services.Auth.AuthModels;
using Microsoft.AspNetCore.SignalR;
using TelemarketingControlSystem.Services.NotificationHub;
using NPOI.SS.Formula.Functions;
using TelemarketingControlSystem.Services.NotificationHub.ViewModel;
using TelemarketingControlSystem.Models.Notification;
using Microsoft.OpenApi.Extensions;
using System.Linq;

namespace TelemarketingControlSystem.Services.Projects
{
    public interface IProjectService
    {
        ResultWithMessage getProjectTypes();
        ResultWithMessage getLineTypes();
        ResultWithMessage getRegions();
        ResultWithMessage getCities();
        ResultWithMessage getCallStatuses();
        ResultWithMessage getLineGenerations();
        ResultWithMessage getEmployees();
        ResultWithMessage getById(int id, TenantDto authData);
        ResultWithMessage getByFilter(ProjectFilterModel model, TenantDto authData);
        Task<ResultWithMessage> create(CreateProjectViewModel model, TenantDto authData);
        Task<ResultWithMessage> update(UpdateProjectViewModel model, TenantDto authData);
        Task<ResultWithMessage> delete(int id, TenantDto authData);
        Task<ResultWithMessage> reDistributeProjectGSMs(int projectId, string EmployeeIds, TenantDto tenantDto);
        Task<ResultWithMessage> updateProjectDetail(ProjectDetailViewModel model, TenantDto tenantDto);
    }

    public class ProjectService(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IHubContext<NotifiyHub, INotificationService> notification) : IProjectService
    {
        private readonly ApplicationDbContext _db = db;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly IHubContext<NotifiyHub, INotificationService> _notification = notification;

        private IQueryable<Project> getProjectData(ProjectFilterModel filter, TenantDto authData)
        {
            IQueryable<Project> query;

            if (authData.tenantAccesses[0].RoleList.Contains("Admin"))
                query = _db.Projects.Where(e => !e.IsDeleted);
            else if (authData.tenantAccesses[0].RoleList.Contains("Telemarketer"))
            {
                Employee employee = _db.Employees.Single(e => e.UserName == authData.userName);
                query = _db.Projects.Where(e => e.ProjectDetails.Any(e => e.EmployeeId == employee.Id) && !e.IsDeleted);
            }

            else
                return null;

            if (!string.IsNullOrEmpty(filter.SearchQuery))
                query = query.Where(e => e.Name.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()));

            return query;
        }
        private IQueryable<ProjectViewModel> convertProjectsToListViewModel(IQueryable<Project> model) =>
          model.Select(e => new ProjectViewModel
          {
              Id = e.Id,
              Name = e.Name,
              DateFrom = e.DateFrom,
              DateTo = e.DateTo,
              Quota = e.Quota,
              TypeId = e.TypeId,
              Type = projectTypes.ElementAt(e.TypeId - 1),
              CreatedBy = e.CreatedBy
          });
        private async Task<string> validateCreateProjectViewModel(CreateProjectViewModel model)
        {
            Project project = await _db.Projects.SingleOrDefaultAsync(e => e.Name.ToLower() == model.Name.Trim().ToLower() && !e.IsDeleted);
            if (project is not null)
                return $"The Project with name: {model.Name} is already exists";

            if (model.DateFrom >= model.DateTo)
                return "End Date Should be greater than Start Date";

            if (model.EmployeeIDs.Length == 0)
                return "Empty Employee IDs";

            return string.Empty;
        }
        private async Task<string> validateUpdateProjectViewModel(UpdateProjectViewModel model)
        {
            if (model.DateFrom >= model.DateTo)
                return "Project End Date Should be greater than Start Date";

            int projectGSMs = _db.ProjectDetails.Where(e => e.ProjectId == model.Id).Count();
            if (model.Quota > projectGSMs)
                return $"The Quota {model.Quota} should be equal or less than GSMs {projectGSMs}";

            return string.Empty;
        }
        private string validateGSMsExcelFile(IFormFile GSMsFile, List<GSMExcel> gsmList, int quota)
        {
            string fileExtension = Path.GetExtension(GSMsFile.FileName).Remove(0, 1);
            if (!fileExtension.Equals(enAllowedFileExtension.xlsx.ToString(), StringComparison.CurrentCultureIgnoreCase))
                return $"Invalid file type, Allow Excel Files only -> {enAllowedFileExtension.xlsx}";

            int excelFileRecordsCount = gsmList.Count();

            if (excelFileRecordsCount == 0)
                return $"Empty Excel file: {GSMsFile.FileName}";

            if (quota > excelFileRecordsCount)
                return $"The Quota {quota} should be equal or less than GSMs {excelFileRecordsCount}";

            return string.Empty;
        }
        private string saveFile(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);

            var webRootPath = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var folderPath = Path.Combine(webRootPath, enExcelUploadFolderName.ExcelUploads.ToString());
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            //var fileName = $"{Guid.NewGuid()}.{extension}";
            var fileName = $"{file.Name}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            return filePath;
        }
        private async Task<int> getCreatedProjectId(CreateProjectViewModel model, TenantDto authData)
        {
            Project project = new()
            {
                Name = model.Name,
                DateFrom = model.DateFrom,
                DateTo = model.DateTo,
                Quota = model.Quota,
                CreatedBy = authData.userName,
                AddedOn = DateTime.Now,
                TypeId = model.TypeId
            };

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            return project.Id;
        }

        private async Task pushNotification(int? projectId, string projectName, List<string> userIds)
        {
            List<string> userNames = _db.Employees.Where(x => userIds.Contains(x.Id.ToString())).Select(x => x.UserName).ToList();

            foreach (string s in userNames)
            {
                var client = _db.HubClients.FirstOrDefault(x => x.userName == s);
                Notification not = new Notification("Create New Project", projectId, projectName + " By :" + s.Substring(s.IndexOf("\\") + 1), s, client != null ? client.connectionId : null);
                _db.Notifications.Add(not);
                _db.SaveChanges();
                if (client != null && !string.IsNullOrEmpty(client.connectionId))
                {
                    await _notification.Clients.Client(client.connectionId).SendMessage(new NotificationListViewModel
                    {
                        Id = not.Id,
                        ProjectId = (int)projectId,
                        ProjectName = projectName,
                        Message = projectName + " By :" + s.Substring(s.IndexOf("\\") + 1),
                        IsRead = false,
                        Type = NotificationType.NotType.CreateNewProject.GetDisplayName(),
                        Title = "Create New Project",
                        CreatedDate = DateTime.Now

                    });
                }


            }
        }
        public List<ListViewModel> convertListToListViewModel(List<string> list)
        {
            List<ListViewModel> result = [];


            for (int i = 0; i < list.Count; i++)
            {
                ListViewModel model = new()
                {
                    Id = i + 1,
                    Name = list.ElementAt(i)
                };

                result.Add(model);
            }

            return result.OrderBy(e => e.Name).ToList();
        }


        ///////////////////////////// Exposed Methods /////////////////////////////
        public ResultWithMessage getProjectTypes() => new(convertListToListViewModel(projectTypes), string.Empty);
        public ResultWithMessage getLineTypes() => new(convertListToListViewModel(lineTypes), string.Empty);
        public ResultWithMessage getRegions() => new(convertListToListViewModel(regions), string.Empty);
        public ResultWithMessage getCities() => new(convertListToListViewModel(cities), string.Empty);
        public ResultWithMessage getCallStatuses() => new(convertListToListViewModel(callStatuses), string.Empty);
        public ResultWithMessage getLineGenerations() => new(convertListToListViewModel(generations), string.Empty);
        public ResultWithMessage getEmployees()
        {
            List<EmployeeViewModel> employees = _db.Employees
                .Select(e => new EmployeeViewModel
                {
                    Id = e.Id,
                    UserName = e.UserName
                })
                .OrderBy(e => e.UserName).ToList();

            if (employees is null)
                return new ResultWithMessage(null, $"Empty Employees");

            return new ResultWithMessage(employees, string.Empty);
        }
        public ResultWithMessage getById(int id, TenantDto authData)
        {
            Project project = new();

            if (authData.tenantAccesses[0].RoleList.Contains("Admin"))
            {
                project = _db.Projects.Include(e => e.ProjectDetails)
                                          .ThenInclude(e => e.Employee)
                                          .Where(e => e.Id == id && !e.IsDeleted).FirstOrDefault();
            }
            else if (authData.tenantAccesses[0].RoleList.Contains("Telemarketer"))
            {
                Employee employee = _db.Employees.Single(e => e.UserName == authData.userName);

                project = _db.Projects.Include(e => e.ProjectDetails.Where(e => e.EmployeeId == employee.Id))
                                          .ThenInclude(e => e.Employee)
                                          .Where(e => e.Id == id && e.ProjectDetails.Any(e => e.EmployeeId == employee.Id) && !e.IsDeleted).FirstOrDefault();
            }

            if (project is null)
                return new ResultWithMessage(null, $"The project with ID: {id} is not Exists");

            ProjectViewModel model = new()
            {
                Id = project.Id,
                Name = project.Name,
                DateFrom = project.DateFrom,
                DateTo = project.DateTo,
                Quota = project.Quota,
                TypeId = project.TypeId,
                Type = projectTypes.ElementAt(project.TypeId - 1),
                CreatedBy = project.CreatedBy,

                ProjectDetails = project.ProjectDetails.Select(e => new ProjectDetailViewModel()
                {
                    Id = e.Id,
                    GSM = e.GSM,
                    AlternativeNumber = e.AlternativeNumber,
                    Bundle = e.Bundle,
                    Note = e.Note,
                    Contract = e.Contract,
                    Segment = e.Segment,
                    SubSegment = e.SubSegment,
                    EmployeeID = e.EmployeeId,
                    EmployeeUserName = e.Employee.UserName,

                    LineTypeId = e.LineTypeId != 0 ? e.LineTypeId : null,
                    LineType = e.LineTypeId != 0 ? lineTypes.ElementAt(int.Parse(e.LineTypeId.ToString()) - 1) : null,

                    GenerationId = e.GenerationId != 0 ? e.GenerationId : null,
                    Generation = e.GenerationId != 0 ? generations.ElementAt(int.Parse(e.GenerationId.ToString()) - 1) : null,

                    RegionId = e.RegionId != 0 ? e.RegionId : null,
                    Region = e.RegionId != 0 ? regions.ElementAt((int)e.RegionId - 1) : null,

                    CityId = e.CityId != 0 ? e.CityId : null,
                    City = e.CityId != 0 ? cities.ElementAt((int)e.CityId - 1) : null,

                    CallStatusId = e.CallStatusId != 0 ? e.CallStatusId : null,
                    CallStatus = e.CallStatusId != 0 ? callStatuses.ElementAt((int)e.CallStatusId - 1) : null
                }).ToList()
            };

            return new ResultWithMessage(model, string.Empty);
        }
        public ResultWithMessage getByFilter(ProjectFilterModel filter, TenantDto authData)
        {
            //1- Apply Filters just search query
            var query = getProjectData(filter, authData);

            //2- Generate List View Model
            var queryViewModel = convertProjectsToListViewModel(query);

            //3- Sorting using our extension
            filter.SortActive = filter.SortActive == string.Empty ? "ID" : filter.SortActive;

            if (filter.SortDirection == enSortDirection.desc.ToString())
                queryViewModel = queryViewModel.OrderByDescending(filter.SortActive);
            else
                queryViewModel = queryViewModel.OrderBy(filter.SortActive);


            //4- pagination
            int resultSize = queryViewModel.Count();
            var resultData = queryViewModel.Skip(filter.PageSize * filter.PageIndex).Take(filter.PageSize).ToList();

            //5- return 
            return new ResultWithMessage(new DataWithSize(resultSize, resultData), "");
        }
        public async Task<ResultWithMessage> create(CreateProjectViewModel model, TenantDto authData)
        {
            var transaction = _db.Database.BeginTransaction();
            try
            {
                string validateCreateProjectModelErrorMessage = await validateCreateProjectViewModel(model);
                if (!string.IsNullOrEmpty(validateCreateProjectModelErrorMessage))
                    return new ResultWithMessage(null, validateCreateProjectModelErrorMessage);

                string filePath = saveFile(model.GSMsFile);
                List<GSMExcel> gsmExcelList = ExcelHelper.Import<GSMExcel>(filePath);
                string validateGsmExcelErrorMessage = validateGSMsExcelFile(model.GSMsFile, gsmExcelList, model.Quota);
                if (!string.IsNullOrEmpty(validateGsmExcelErrorMessage))
                    return new ResultWithMessage(null, validateGsmExcelErrorMessage);

                int createdProjectId = await getCreatedProjectId(model, authData);
                if (createdProjectId < 0)
                    return new ResultWithMessage(null, "Invalid Created Project ID");

                List<string> employeeIDs = model.EmployeeIDs.Split(',').ToList();
                int empIndex = -1;

                foreach (GSMExcel gsmExcel in gsmExcelList)
                {
                    if (empIndex == employeeIDs.Count - 1)
                        empIndex = 0;
                    else
                        empIndex++;

                    ProjectDetail projectDetail = new()
                    {
                        GSM = gsmExcel.GSM,
                        Note = gsmExcel.Note,
                        AddedOn = DateTime.Now,
                        AlternativeNumber = gsmExcel.AlternativeNumber,
                        Bundle = gsmExcel.Bundle,
                        Contract = gsmExcel.Contract,
                        CreatedBy = authData.userName,
                        SubSegment = gsmExcel.SubSegment,
                        Segment = gsmExcel.Segment,
                        ProjectId = createdProjectId,
                        EmployeeId = int.Parse(employeeIDs.ElementAt(empIndex)),
                        RegionId = gsmExcel.Region is not null ? regions.IndexOf(gsmExcel.Region) + 1 : null,
                        LineTypeId = gsmExcel.LineType is not null ? lineTypes.IndexOf(gsmExcel.LineType) + 1 : null,
                        CityId = gsmExcel.City is not null ? cities.IndexOf(gsmExcel.City) + 1 : null,
                        CallStatusId = gsmExcel.CallStatus is not null ? callStatuses.IndexOf(gsmExcel.CallStatus) + 1 : null,
                        GenerationId = gsmExcel.Generation is not null ? generations.IndexOf(gsmExcel.Generation) + 1 : null
                    };

                    _db.ProjectDetails.Add(projectDetail);
                };

                _db.SaveChanges();
                //---------------------Send Notification--------------------------
                await pushNotification(createdProjectId, model.Name, employeeIDs);
                transaction.Commit();

                return getById(createdProjectId, authData);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new ResultWithMessage(null, ex.Message);
            }
        }
        public async Task<ResultWithMessage> update(UpdateProjectViewModel model, TenantDto authData)
        {
            Project projectToUpdate = await _db.Projects
                .Include(e => e.ProjectDetails.Where(x => model.ProjectDetails.Select(e => e.Id).Contains(x.Id)).OrderBy(e => e.Id))
                .SingleOrDefaultAsync(e => e.Id == model.Id);
            if (projectToUpdate is null)
                return new ResultWithMessage(null, $"Invalid Project ID: {model.Id}");

            string modelErrorMessage = await validateUpdateProjectViewModel(model);
            if (!string.IsNullOrEmpty(modelErrorMessage))
                return new ResultWithMessage(null, modelErrorMessage);

            var transaction = _db.Database.BeginTransaction();
            try
            {
                if (authData.tenantAccesses[0].RoleList.Contains("Admin"))
                {
                    projectToUpdate.Name = model.Name;
                    projectToUpdate.DateFrom = model.DateFrom;
                    projectToUpdate.DateTo = model.DateTo;
                    projectToUpdate.Quota = model.Quota;
                    projectToUpdate.TypeId = model.TypeId;
                    projectToUpdate.LastUpdatedBy = authData.userName;
                    projectToUpdate.LastUpdateDate = DateTime.Now;
                }

                // update project details
                if (model.ProjectDetails.Count > 0)
                {
                    model.ProjectDetails = model.ProjectDetails.OrderBy(e => e.Id).ToList();
                    for (int i = 0; i < projectToUpdate.ProjectDetails.Count; i++)
                    {
                        if (authData.tenantAccesses[0].RoleList.Contains("Admin"))
                            projectToUpdate.ProjectDetails.ElementAt(i).EmployeeId = model.ProjectDetails.ElementAt(i).EmployeeID;

                        projectToUpdate.ProjectDetails.ElementAt(i).Note = model.ProjectDetails.ElementAt(i).Note;
                        projectToUpdate.ProjectDetails.ElementAt(i).CallStatusId = model.ProjectDetails.ElementAt(i).CallStatusId;
                        projectToUpdate.ProjectDetails.ElementAt(i).LastUpdatedBy = authData.userName;
                        projectToUpdate.ProjectDetails.ElementAt(i).LastUpdateDate = DateTime.Now;
                    }
                }

                _db.Update(projectToUpdate);
                _db.SaveChanges();
                transaction.Commit();

                return getById(model.Id, authData);
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return new ResultWithMessage(null, e.Message);
            }
        }
        public async Task<ResultWithMessage> delete(int id, TenantDto authData)
        {
            Project project = await _db.Projects.Include(e => e.ProjectDetails).SingleOrDefaultAsync(e => e.Id == id);
            if (project is null)
                return new ResultWithMessage(null, $"Invalid Project ID: {id}");

            if (project.IsDeleted)
                return new ResultWithMessage(null, $"The Project with ID: {id} already deleted");

            var transaction = _db.Database.BeginTransaction();
            try
            {
                project.IsDeleted = true;
                project.LastUpdatedBy = authData.userName;
                project.LastUpdateDate = DateTime.Now;

                foreach (ProjectDetail projectDetail in project.ProjectDetails)
                {
                    projectDetail.IsDeleted = true;
                    projectDetail.LastUpdatedBy = authData.userName;
                    projectDetail.LastUpdateDate = DateTime.Now;
                }

                _db.Update(project);
                _db.ProjectDetails.UpdateRange(project.ProjectDetails);
                _db.SaveChanges();
                transaction.Commit();

                return new ResultWithMessage(id, string.Empty);
            }
            catch (Exception e)
            {
                transaction.Rollback();
                return new ResultWithMessage(null, e.Message);
            }
        }

        public async Task<ResultWithMessage> reDistributeProjectGSMs(int projectId, string EmployeeIds, TenantDto authData)
        {
            if (!authData.tenantAccesses[0].RoleList.Contains("Admin"))
                return new ResultWithMessage(null, $"Insufficient Privilege to do ReDistribute Project GSMs");

            Project project = await _db.Projects.Include(e => e.ProjectDetails)
                .SingleOrDefaultAsync(e => e.Id == projectId && !e.IsDeleted);

            if (project is null)
                return new ResultWithMessage(null, $"Invalid Project Id: {projectId}");

            if (string.IsNullOrEmpty(EmployeeIds))
                return new ResultWithMessage(null, $"Empty Employee Ids");

            List<string> employeeIDs = EmployeeIds.Split(',').ToList();
            int empIndex = -1;

            try
            {
                foreach (ProjectDetail projectDetail in project.ProjectDetails)
                {
                    if (!projectDetail.IsDeleted && projectDetail.CallStatusId == callStatuses.IndexOf("Initialize") + 1)
                    {
                        if (empIndex == employeeIDs.Count - 1)
                            empIndex = 0;
                        else
                            empIndex++;

                        projectDetail.EmployeeId = int.Parse(employeeIDs.ElementAt(empIndex));
                    }
                }

                _db.ProjectDetails.UpdateRange(project.ProjectDetails);
                _db.SaveChanges();

                return getById(projectId, authData);
            }
            catch (Exception e)
            {
                return new ResultWithMessage(null, e.Message);
            }
        }

        public async Task<ResultWithMessage> updateProjectDetail(ProjectDetailViewModel model, TenantDto authData)
        {
            ProjectDetail projectDetailToUpdate = await _db.ProjectDetails.FindAsync(model.Id);

            if (projectDetailToUpdate is null)
                return new ResultWithMessage(null, $"Empty Project Detail");

            try
            {
                projectDetailToUpdate.CallStatusId = model.CallStatusId;
                projectDetailToUpdate.EmployeeId = model.EmployeeID;
                projectDetailToUpdate.Note = model.Note;

                _db.Update(projectDetailToUpdate);
                _db.SaveChanges();
                return getById(projectDetailToUpdate.ProjectId, authData);
            }
            catch (Exception e)
            {
                return new ResultWithMessage(null, e.Message);
            }
        }
    }
}