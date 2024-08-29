using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models;
using TelemarketingControlSystem.Models.Data;
using System.Data;
using static TelemarketingControlSystem.Helper.ConstantValues;
using static TelemarketingControlSystem.Services.Auth.AuthModels;
using Microsoft.AspNetCore.SignalR;
using TelemarketingControlSystem.Services.NotificationHub;
using TelemarketingControlSystem.Services.NotificationHub.ViewModel;
using TelemarketingControlSystem.Models.Notification;
using Microsoft.OpenApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace TelemarketingControlSystem.Services.Projects
{
    public interface IProjectService
    {
        ResultWithMessage getProjectTypes();
        ResultWithMessage getRegions();
        ResultWithMessage getCallStatuses();
        ResultWithMessage getEmployees();
        ResultWithMessage getById(int id, [FromBody] ProjectFilterModel filter, TenantDto authData);
        ResultWithMessage getByFilter(ProjectFilterModel model, TenantDto authData);
        Task<ResultWithMessage> create(CreateProjectViewModel model, TenantDto authData);
        Task<ResultWithMessage> update(UpdateProjectViewModel model, TenantDto authData);
        Task<ResultWithMessage> delete(int id, TenantDto authData);
        Task<ResultWithMessage> reDistributeProjectGSMs(int projectId, string EmployeeIds, TenantDto tenantDto);
        Task<ResultWithMessage> updateProjectDetail(ProjectDetailViewModel model, TenantDto tenantDto);
        ByteResultWithMessage exportProjectDetailsToExcel(int projectId);
        ByteResultWithMessage exportProjectsToExcel();
    }

    public class ProjectService(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IHubContext<NotifiyHub, INotificationService> notification, IConfiguration config) : IProjectService
    {
        private readonly ApplicationDbContext _db = db;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly IHubContext<NotifiyHub, INotificationService> _notification = notification;
        private readonly IConfiguration _config = config;
        private IQueryable<Project> getProjectData(ProjectFilterModel filter, TenantDto authData)
        {
            IQueryable<Project> query;

            if (authData.tenantAccesses[0].RoleList.Contains(enRoles.Admin.ToString()))
                query = _db.Projects.Where(e => !e.IsDeleted);
            else if (authData.tenantAccesses[0].RoleList.Contains(enRoles.Telemarketer.ToString()))
            {
                Employee employee = _db.Employees.Single(e => e.UserName == authData.userName);
                query = _db.Projects.Where(e => e.ProjectDetails.Any(e => e.EmployeeId == employee.Id) && !e.IsDeleted);
            }

            else
                return null;

            if (!string.IsNullOrEmpty(filter.SearchQuery))
                query = query.Where(e => e.Name.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower())
                              || e.Id.ToString().Contains(filter.SearchQuery.Trim().ToLower()));

            if (filter.DateFrom != null && filter.DateTo != null)
            {
                query = query.Where(x => x.DateFrom >= filter.DateFrom && x.DateTo <= filter.DateTo);
            }

            if (filter.CreatedBy != null && filter.CreatedBy.Count() != 0)
            {
                query = query.Where(x => filter.CreatedBy.Contains(x.CreatedBy));
            }

            if (filter.TypeIds != null && filter.TypeIds.Count() != 0)
            {
                query = query.Where(x => filter.TypeIds.Contains(x.TypeId));
            }


            return query;
        }
        private IQueryable<ProjectDetail> getProjectDetailsData(int id, ProjectFilterModel filter, TenantDto authData)
        {
            IQueryable<ProjectDetail> query;

            if (authData.tenantAccesses[0].RoleList.Contains(enRoles.Admin.ToString()))
                query = _db.ProjectDetails.Include(e => e.CallStatus).Include(e => e.Employee).Where(e => e.ProjectId == id && !e.IsDeleted);
            else if (authData.tenantAccesses[0].RoleList.Contains(enRoles.Telemarketer.ToString()))
            {
                Employee employee = _db.Employees.Single(e => e.UserName == authData.userName);
                query = _db.ProjectDetails.Include(e => e.CallStatus).Include(e => e.Employee).Where(e => e.ProjectId == id && e.EmployeeId == employee.Id && !e.IsDeleted);
            }

            else
                return null;

            if (!string.IsNullOrEmpty(filter.SearchQuery))
                query = query.Where(e => e.GSM.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()) ||
                                         e.Contract.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()) ||
                                         e.AlternativeNumber.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()) ||
                                         e.Note.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()));

            if (filter.ColumnFilters is not null && filter.ColumnFilters.Count > 0)
            {
                foreach (ColumnFilter columnFilter in filter.ColumnFilters)
                {
                    switch (columnFilter.ColumnName)
                    {
                        case "LineType":
                            //foreach (string value in columnFilter.DistinctValues)
                            //{
                            //    query = query.Where(e => e.LineType.Trim().ToLower().Contains(value.Trim().ToLower()));
                            //}
                            query = query.Where(e => columnFilter.ColumnName.Any(lt => e.LineType.Trim().ToLower().Contains(lt.ToString().Trim().ToLower())));
                            break;

                        case "CallStatus":
                            //foreach (string value in columnFilter.DistinctValues)
                            //{
                            //    query = query.Where(e => e.CallStatus.Name.Trim().ToLower().Contains(value.Trim().ToLower()));
                            //}
                            query = query.Where(e => columnFilter.ColumnName.Any(cs => e.CallStatus.Name.Trim().ToLower().Contains(cs.ToString().Trim().ToLower())));
                            break;

                        case "Employee":
                            //foreach (string value in columnFilter.DistinctValues)
                            //{
                            //    query = query.Where(e => e.Employee.UserName.Trim().ToLower().Contains(value.Trim().ToLower()));
                            //}
                            query = query.Where(e => columnFilter.ColumnName.Any(emp => e.Employee.UserName.Trim().ToLower().Contains(emp.ToString().Trim().ToLower())));
                            break;

                        case "Generation":
                            //foreach (string value in columnFilter.DistinctValues)
                            //{
                            //    query = query.Where(e => e.Generation.Trim().ToLower().Contains(value.Trim().ToLower()));
                            //}
                            query = query.Where(e => columnFilter.ColumnName.Any(gen => e.Generation.Trim().ToLower().Contains(gen.ToString().Trim().ToLower())));
                            break;

                        case "Regions":
                            //foreach (string value in columnFilter.DistinctValues)
                            //{
                            //    query = query.Where(e => e.Region.Trim().ToLower().Contains(value.Trim().ToLower()));
                            //}
                            query = query.Where(e => columnFilter.ColumnName.Any(reg => e.Region.Trim().ToLower().Contains(reg.ToString().Trim().ToLower())));
                            break;


                        case "City":
                            //foreach (string value in columnFilter.DistinctValues)
                            //{
                            //    query = query.Where(e => e.City.Trim().ToLower().Contains(value.Trim().ToLower()));
                            //}
                            query = query.Where(e => columnFilter.ColumnName.Any(cit => e.City.Trim().ToLower().Contains(cit.ToString().Trim().ToLower())));

                            break;

                        case "Segment":
                            //foreach (string value in columnFilter.DistinctValues)
                            //{
                            //    query = query.Where(e => e.Segment.Trim().ToLower().Contains(value.Trim().ToLower()));
                            //}
                            query = query.Where(e => columnFilter.ColumnName.Any(seg => e.Segment.Trim().ToLower().Contains(seg.ToString().Trim().ToLower())));
                            break;

                        case "SubSegment":
                            //foreach (string value in columnFilter.DistinctValues)
                            //{
                            //    query = query.Where(e => e.SubSegment.Trim().ToLower().Contains(value.Trim().ToLower()));
                            //}
                            query = query.Where(e => columnFilter.ColumnName.Any(subS => e.SubSegment.Trim().ToLower().Contains(subS.ToString().Trim().ToLower())));
                            break;

                        case "Bundle":
                            //foreach (string value in columnFilter.DistinctValues)
                            //{
                            //    query = query.Where(e => e.Bundle.Trim().ToLower().Contains(value.Trim().ToLower()));
                            //}
                            query = query.Where(e => columnFilter.ColumnName.Any(bun => e.Bundle.Trim().ToLower().Contains(bun.ToString().Trim().ToLower())));
                            break;

                        default:
                            break;
                    }
                }
            }

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
              CreatedBy = Utilities.modifyUserName(e.CreatedBy)
          });
        private IQueryable<ProjectDetailViewModel> convertProjectDetailToListViewModel(IQueryable<ProjectDetail> model)
        {
            return model.Select(e => new ProjectDetailViewModel
            {
                Id = e.Id,
                GSM = e.GSM,
                LineType = e.LineType,
                CallStatusId = e.CallStatusId,
                CallStatus = _db.CallStatuses.Single(x => x.Id == e.CallStatusId).Name,
                Generation = e.Generation,
                Region = e.Region,
                City = e.City,
                Segment = e.Segment,
                SubSegment = e.SubSegment,
                Bundle = e.Bundle,
                Contract = e.Contract,
                AlternativeNumber = e.AlternativeNumber,
                Note = e.Note,
                EmployeeUserName = Utilities.modifyUserName(e.Employee.UserName),
                EmployeeID = e.EmployeeId,
                LastUpdateDate = e.LastUpdateDate,
            });
        }
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
            if (model.DateFrom > model.DateTo)
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
        private async Task pushNotification(int? projectId, string projectName, List<string> userIds, string msg, string title, string createdBy)
        {
            List<string> userNames = _db.Employees.Where(x => userIds.Contains(x.Id.ToString())).Select(x => x.UserName).ToList();

            foreach (string s in userNames)
            {
                var client = _db.HubClients.FirstOrDefault(x => x.userName == s);
                Notification not = new Notification(title, projectId, msg, s, client != null ? client.connectionId : null, _config["ProfileImg"].Replace("VarXXX", createdBy.Substring(createdBy.IndexOf("\\") + 1)));
                _db.Notifications.Add(not);
                _db.SaveChanges();
                if (client != null && !string.IsNullOrEmpty(client.connectionId))
                {
                    await _notification.Clients.Client(client.connectionId).SendMessage(new NotificationListViewModel
                    {
                        Id = not.Id,
                        ProjectId = (int)projectId,
                        ProjectName = projectName,
                        Message = msg,
                        IsRead = false,
                        Type = NotificationType.NotType.CreateNewProject.GetDisplayName(),
                        Title = title,
                        CreatedDate = DateTime.Now,
                        Img = _config["ProfileImg"].Replace("VarXXX", s.Substring(s.IndexOf("\\") + 1))

                    });
                }


            }
        }
        private List<ColumnFilter> getProjectDetailsColumnFilters(int projectId)
        {
            List<ColumnFilter> result = [];

            ColumnFilter lineType = new()
            {
                ColumnName = "LineType",
                DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted).Select(e => e.LineType).Distinct().ToList()
            };

            ColumnFilter callStatus = new()
            {
                ColumnName = "CallStatus",
                DistinctValues = _db.ProjectDetails.Include(e => e.CallStatus).Where(e => e.ProjectId == projectId && !e.IsDeleted).Select(e => e.CallStatus.Name).Distinct().ToList()
            };

            ColumnFilter employee = new()
            {
                ColumnName = "Employee",
                DistinctValues = _db.ProjectDetails.Include(e => e.Employee).Where(e => e.ProjectId == projectId && !e.IsDeleted).Select(e => Utilities.modifyUserName(e.Employee.UserName)).Distinct().ToList()
            };

            ColumnFilter generation = new()
            {
                ColumnName = "Generation",
                DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted).Select(e => e.Generation).Distinct().ToList()
            };

            ColumnFilter region = new()
            {
                ColumnName = "Region",
                DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted).Select(e => e.Region).Distinct().ToList()
            };

            ColumnFilter city = new()
            {
                ColumnName = "City",
                DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted).Select(e => e.City).Distinct().ToList()
            };

            ColumnFilter segment = new()
            {
                ColumnName = "Segment",
                DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted).Select(e => e.Segment).Distinct().ToList()
            };

            ColumnFilter subSegment = new()
            {
                ColumnName = "SubSegment",
                DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted).Select(e => e.SubSegment).Distinct().ToList()
            };

            ColumnFilter bundle = new()
            {
                ColumnName = "Bundle",
                DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted).Select(e => e.Bundle).Distinct().ToList()
            };

            if (lineType is not null && lineType.DistinctValues.Count > 0)
                result.Add(lineType);

            if (callStatus is not null && callStatus.DistinctValues.Count > 0)
                result.Add(callStatus);

            if (generation is not null && generation.DistinctValues.Count > 0)
                result.Add(generation);

            if (region is not null && region.DistinctValues.Count > 0)
                result.Add(region);

            if (city is not null && city.DistinctValues.Count > 0)
                result.Add(city);

            if (segment is not null && segment.DistinctValues.Count > 0)
                result.Add(segment);

            if (subSegment is not null && subSegment.DistinctValues.Count > 0)
                result.Add(subSegment);

            if (employee is not null && employee.DistinctValues.Count > 0)
                result.Add(employee);

            if (bundle is not null && bundle.DistinctValues.Count > 0)
                result.Add(bundle);

            return result;
        }



        public List<ListViewModel> convertListToListViewModel(List<string> list)
        {
            List<ListViewModel> result = [];


            for (int i = 2; i < list.Count; i++)
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
        public ResultWithMessage getProjectTypes() => new(convertListToListViewModel(projectTypes), string.Empty);
        public ResultWithMessage getRegions() => new(convertListToListViewModel(regions), string.Empty);
        public ResultWithMessage getCallStatuses() => new ResultWithMessage(_db.CallStatuses.ToList(), string.Empty);
        public ResultWithMessage getEmployees()
        {
            List<EmployeeViewModel> employees = _db.Employees.Where(e => e.IsActive).ToList()
                .Select(e => new EmployeeViewModel
                {
                    Id = e.Id,
                    UserName = Utilities.modifyUserName(e.UserName)
                })
                .OrderBy(e => e.UserName).ToList();

            if (employees is null)
                return new ResultWithMessage(null, $"Empty Employees");

            return new ResultWithMessage(employees, string.Empty);
        }
        public ResultWithMessage getById(int id, [FromBody] ProjectFilterModel filter, TenantDto authData)
        {
            Project project = _db.Projects.Find(id);
            if (project is null)
                return new ResultWithMessage(null, $"The project with ID: {id} is not Exists");


            //1- Apply Filters just search query
            var query = getProjectDetailsData(id, filter, authData);

            //2- Generate List View Model
            var queryViewModel = convertProjectDetailToListViewModel(query);

            //3- Sorting using our extension
            filter.SortActive = filter.SortActive == string.Empty ? "ID" : filter.SortActive;

            if (filter.SortDirection == enSortDirection.desc.ToString())
                queryViewModel = queryViewModel.OrderByDescending(filter.SortActive);
            else
                queryViewModel = queryViewModel.OrderBy(filter.SortActive);


            //4- pagination
            int resultSize = queryViewModel.Count();
            var resultData = queryViewModel.Skip(filter.PageSize * filter.PageIndex).Take(filter.PageSize).ToList();


            // columns filters
            List<ColumnFilter> columnFilters = getProjectDetailsColumnFilters(project.Id);

            //5- return 
            ProjectViewModel model = new()
            {
                Id = project.Id,
                Name = project.Name,
                DateFrom = project.DateFrom,
                DateTo = project.DateTo,
                Quota = project.Quota,
                TypeId = project.TypeId,
                Type = projectTypes.ElementAt(project.TypeId - 1),
                CreatedBy = Utilities.modifyUserName(project.CreatedBy),
                ProjectDetails = resultData,
                ColumnFilters = columnFilters
            };

            return new ResultWithMessage(new DataWithSize(resultSize, model), string.Empty);
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
            var resultData = queryViewModel.Skip(filter.PageIndex * filter.PageSize).Take(filter.PageSize).ToList();

            //5- return 
            return new ResultWithMessage(new DataWithSize(resultSize, resultData), "");
        }
        public async Task<ResultWithMessage> create(CreateProjectViewModel model, TenantDto authData)
        {
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

                var nullGsm = gsmExcelList.Select((z, i) => new { z.GSM, i }).FirstOrDefault(e => e.GSM == null);
                if (nullGsm != null)
                    return new ResultWithMessage(null, $"Empty GSM at row {nullGsm.i}");

                var duplication = gsmExcelList.GroupBy(x => x.GSM).Select(z => new
                {
                    z.Key,
                    count = z.Count()
                }).FirstOrDefault(y => y.count > 1);

                if (duplication != null)
                    return new ResultWithMessage(null, $"Duplicate GSM {duplication.Key}");

                Project project = new()
                {
                    Name = model.Name,
                    DateFrom = model.DateFrom,
                    DateTo = model.DateTo,
                    Quota = model.Quota,
                    CreatedBy = authData.userName,
                    AddedOn = DateTime.Now,
                    TypeId = model.TypeId,
                    ProjectDetails = []
                };

                List<string> employeeIDs = model.EmployeeIDs.Split(',').ToList();
                int empIndex = -1;

                List<CallStatus> callStatuses = _db.CallStatuses.ToList();


                foreach (GSMExcel gsmExcel in gsmExcelList)
                {
                    if (empIndex == employeeIDs.Count - 1)
                        empIndex = 0;
                    else
                        empIndex++;

                    ProjectDetail projectDetail = new()
                    {
                        GSM = gsmExcel.GSM,
                        LineType = gsmExcel.LineType,
                        CallStatusId = string.IsNullOrEmpty(gsmExcel.CallStatus) ? 1 : callStatuses.SingleOrDefault(e => e.Name == gsmExcel.CallStatus).Id,
                        Generation = gsmExcel.Generation,
                        Region = gsmExcel.Region,
                        City = gsmExcel.City,
                        Segment = gsmExcel.Segment,
                        SubSegment = gsmExcel.SubSegment,
                        Bundle = gsmExcel.Bundle,
                        Contract = gsmExcel.Contract,
                        AlternativeNumber = gsmExcel.AlternativeNumber,
                        Note = gsmExcel.Note,
                        AddedOn = DateTime.Now,
                        CreatedBy = authData.userName,
                        LastUpdateDate = DateTime.Now,
                        LastUpdatedBy = authData.userName,
                        EmployeeId = int.Parse(employeeIDs.ElementAt(empIndex)),
                    };

                    project.ProjectDetails.Add(projectDetail);
                };

                _db.Projects.Add(project);
                _db.SaveChanges();

                //---------------------Send Notification--------------------------
                pushNotification(project.Id, model.Name, employeeIDs, model.Name + " created By : " + authData.userName.Substring(authData.userName.IndexOf("\\") + 1), "Create New Project", authData.userName);

                return new ResultWithMessage(null, string.Empty);
            }
            catch (Exception ex)
            {
                //Add log return Id -> 400
                return new ResultWithMessage(null, ex.Message);
            }
        }
        public async Task<ResultWithMessage> update(UpdateProjectViewModel model, TenantDto authData)
        {
            Project projectToUpdate = await _db.Projects.FindAsync(model.Id);

            if (projectToUpdate is null)
                return new ResultWithMessage(null, $"Invalid Project ID: {model.Id}");

            string modelErrorMessage = await validateUpdateProjectViewModel(model);
            if (!string.IsNullOrEmpty(modelErrorMessage))
                return new ResultWithMessage(null, modelErrorMessage);

            try
            {
                if (authData.tenantAccesses[0].RoleList.Contains(enRoles.Admin.ToString()))
                {
                    projectToUpdate.Name = model.Name;
                    projectToUpdate.DateFrom = model.DateFrom;
                    projectToUpdate.DateTo = model.DateTo;
                    projectToUpdate.Quota = model.Quota;
                    projectToUpdate.TypeId = model.TypeId;
                    projectToUpdate.LastUpdatedBy = authData.userName;
                    projectToUpdate.LastUpdateDate = DateTime.Now;
                }

                _db.Update(projectToUpdate);
                _db.SaveChanges();

                return new ResultWithMessage(null, string.Empty);
            }
            catch (Exception e)
            {
                //transaction.Rollback();
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
            if (!authData.tenantAccesses[0].RoleList.Contains(enRoles.Admin.ToString()))
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
                    if (!projectDetail.IsDeleted && projectDetail.CallStatusId == 2)
                    {
                        if (empIndex == employeeIDs.Count - 1)
                            empIndex = 0;
                        else
                            empIndex++;

                        projectDetail.EmployeeId = int.Parse(employeeIDs.ElementAt(empIndex));
                        projectDetail.LastUpdateDate = DateTime.Now;
                        projectDetail.LastUpdatedBy = authData.userName;
                    }
                }

                _db.ProjectDetails.UpdateRange(project.ProjectDetails);
                _db.SaveChanges();
                //---------------------Send Notification--------------------------
                pushNotification(projectId, project.Name, employeeIDs, project.Name + " has been redistributed", "redistributed project", authData.userName);
                return new ResultWithMessage(null, string.Empty);
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
                projectDetailToUpdate.LastUpdatedBy = authData.userName;
                projectDetailToUpdate.LastUpdateDate = DateTime.Now;


                _db.Update(projectDetailToUpdate);
                _db.SaveChanges();
                return new ResultWithMessage(null, string.Empty);
            }
            catch (Exception e)
            {
                return new ResultWithMessage(null, e.Message);
            }
        }
        public ByteResultWithMessage exportProjectDetailsToExcel(int projectId)
        {
            Project project = _db.Projects.Find(projectId);
            if (project is null)
                return new ByteResultWithMessage(null, $"Invalid project Id: {projectId}");

            var data = _db.ProjectDetails
               .Where(e => e.ProjectId == projectId)
               .Include(e => e.Project)
               .Select(e => new ProjectDetailsDataToExcel
               {
                   Project = e.Project.Name,
                   CreatedBy = Utilities.modifyUserName(e.CreatedBy),
                   Region = e.Region,
                   LineType = e.LineType,
                   AddedOn = e.AddedOn,
                   AlternativeNumber = e.AlternativeNumber,
                   Bundle = e.Bundle,
                   CallStatus = e.CallStatus.Name,
                   City = e.City,
                   Contract = e.Contract,
                   Employee = Utilities.modifyUserName(e.Employee.UserName),
                   Generation = e.Generation,
                   GSM = e.GSM,
                   LastUpdatedby = Utilities.modifyUserName(e.LastUpdatedBy),
                   LastUpdatedDate = e.LastUpdateDate,
                   Note = e.Note,
                   Segment = e.Segment,
                   SubSegment = e.SubSegment
               })
               .ToList();

            if (data.Count == 0)
                return new ByteResultWithMessage(null, $"No data found");

            var exportService = new ExportService();
            byte[] excelData = exportService.ExportToExcel(data, "Project Details");

            return new ByteResultWithMessage(excelData, string.Empty);
        }
        public ByteResultWithMessage exportProjectsToExcel()
        {
            var data = _db.Projects
               .Select(e => new ProjectDataToExcel
               {
                   Name = e.Name,
                   AddedOn = e.AddedOn,
                   DateFrom = e.DateFrom,
                   DateTo = e.DateTo,
                   Quota = e.Quota,
                   Type = projectTypes.ElementAt(e.TypeId)
               })
               .ToList();

            if (data.Count == 0)
                return new ByteResultWithMessage(null, $"No data found");

            var exportService = new ExportService();
            byte[] excelData = exportService.ExportToExcel(data, "Projects");

            return new ByteResultWithMessage(excelData, string.Empty);
        }
    }
}