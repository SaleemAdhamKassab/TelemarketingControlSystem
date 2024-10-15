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
using TelemarketingControlSystem.Services.ExcelService;

namespace TelemarketingControlSystem.Services.ProjectService
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
		ResultWithMessage updateProjectDetail(ProjectDetailViewModel model, TenantDto tenantDto);
		ByteResultWithMessage exportProjectDetailsToExcel(int projectId, TenantDto tenantDto);
		ByteResultWithMessage exportProjectsToExcel();
		ResultWithMessage getAdmins();
		ResultWithMessage projectExpectedRemainingDays(int projectId);
	}

	public class ProjectService(ApplicationDbContext db,

		IHubContext<NotifiyHub, INotificationService> notification,
		IConfiguration config,
		IExcelService excelService) : IProjectService
	{
		private readonly ApplicationDbContext _db = db;
		private readonly IHubContext<NotifiyHub, INotificationService> _notification = notification;
		private readonly IConfiguration _config = config;
		private readonly IExcelService _excelService = excelService;

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
				query = query.Where(x => x.DateFrom >= filter.DateFrom && x.DateTo <= filter.DateTo);

			if (filter.CreatedBy != null && filter.CreatedBy.Count() != 0)
				query = query.Where(x => filter.CreatedBy.Contains(x.CreatedBy));

			if (filter.TypeIds != null && filter.TypeIds.Count() != 0)
				query = query.Where(x => filter.TypeIds.Contains(x.ProjectTypeId));

			return query;
		}
		private IQueryable<ProjectDetail> getProjectDetailsData(int id, ProjectFilterModel filter, TenantDto authData)
		{
			IQueryable<ProjectDetail> query =
				 _db.ProjectDetails
				 .Include(e => e.Employee)
				 .Where(e => e.ProjectId == id &&
					   !e.IsDeleted);


			//if (authData.tenantAccesses[0].RoleList.Contains(enRoles.Admin.ToString()))
			//    query = query.Where(e => e.ProjectId == id && !e.IsDeleted);
			//else 

			if (authData.tenantAccesses[0].RoleList.Contains(enRoles.Telemarketer.ToString()))
			{
				Employee employee = _db.Employees.Single(e => e.UserName == authData.userName);
				query = query.Where(e => e.EmployeeId == employee.Id);
			}


			if (!string.IsNullOrEmpty(filter.SearchQuery))
				query = query.Where(e => e.GSM.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()) ||
										 e.Contract.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()) ||
										 e.AlternativeNumber.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()) ||
										 e.Note.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()) ||
										 e.CreatedBy.Trim().ToLower().Contains(filter.SearchQuery.Trim().ToLower()));

			if (filter.ColumnFilters == null)
			{
				return query;
			}

			foreach (ColumnFilter columnFilter in filter.ColumnFilters)
			{
				if (columnFilter.ColumnName == "LineType" && columnFilter.DistinctValues.Count > 0)
				{
					query = query.Where(x => (columnFilter.DistinctValues.Contains("NA") ? string.IsNullOrEmpty(x.LineType) : false) || columnFilter.DistinctValues.Contains(x.LineType));
				}

				if (columnFilter.ColumnName == "CallStatus" && columnFilter.DistinctValues.Count > 0)
				{
					query = query.Where(x => (columnFilter.DistinctValues.Contains(x.CallStatus.Name)));
				}

				if (columnFilter.ColumnName == "Employee" && columnFilter.DistinctValues.Count > 0)
				{
					query = query.Where(x => (columnFilter.DistinctValues.Select(u => "syriatel\\" + u.ToLower()).Contains(x.Employee.UserName.ToLower())));
				}

				if (columnFilter.ColumnName == "Generation" && columnFilter.DistinctValues.Count > 0)
				{
					query = query.Where(x => (columnFilter.DistinctValues.Contains("NA") ? string.IsNullOrEmpty(x.Generation) : false) || columnFilter.DistinctValues.Contains(x.Generation));
				}

				if (columnFilter.ColumnName == "Region" && columnFilter.DistinctValues.Count > 0)
				{
					query = query.Where(x => (columnFilter.DistinctValues.Contains("NA") ? string.IsNullOrEmpty(x.Region) : false) || columnFilter.DistinctValues.Contains(x.Region));
				}

				if (columnFilter.ColumnName == "City" && columnFilter.DistinctValues.Count > 0)
				{
					query = query.Where(x => (columnFilter.DistinctValues.Contains("NA") ? string.IsNullOrEmpty(x.City) : false) || columnFilter.DistinctValues.Contains(x.City));
				}

				if (columnFilter.ColumnName == "Segment" && columnFilter.DistinctValues.Count > 0)
				{
					query = query.Where(x => (columnFilter.DistinctValues.Contains("NA") ? string.IsNullOrEmpty(x.SegmentName) : false) || columnFilter.DistinctValues.Contains(x.SegmentName));
				}

				if (columnFilter.ColumnName == "SubSegment" && columnFilter.DistinctValues.Count > 0)
				{
					query = query.Where(x => (columnFilter.DistinctValues.Contains("NA") ? string.IsNullOrEmpty(x.SubSegment) : false) || columnFilter.DistinctValues.Contains(x.SubSegment));
				}

				if (columnFilter.ColumnName == "Bundle" && columnFilter.DistinctValues.Count > 0)
				{
					query = query.Where(x => (columnFilter.DistinctValues.Contains("NA") ? string.IsNullOrEmpty(x.Bundle) : false) || columnFilter.DistinctValues.Contains(x.Bundle));
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
			  TypeId = e.ProjectTypeId,
			  Type = e.ProjectType.Name,
			  CreatedBy = Utilities.modifyUserName(e.CreatedBy),
			  IsClosed = e.DateTo.Date < DateTime.Now.Date,
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
				//Segment = e.Segment,
				Segment = e.SegmentName,
				SubSegment = e.SubSegment,
				Bundle = e.Bundle,
				Contract = e.Contract,
				AlternativeNumber = e.AlternativeNumber,
				Note = e.Note,
				EmployeeUserName = Utilities.modifyUserName(e.Employee.UserName),
				EmployeeID = e.EmployeeId,
				LastUpdateDate = e.LastUpdatedDate
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
				return $"The Quota {model.Quota} should be equal or less than GSMs count: {projectGSMs}";

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
				DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted && !string.IsNullOrEmpty(e.LineType)).Select(e => e.LineType).Distinct().ToList()
			};

			ColumnFilter callStatus = new()
			{
				ColumnName = "CallStatus",
				DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted).Select(e => e.CallStatus.Name).Distinct().ToList()
			};

			ColumnFilter employee = new()
			{
				ColumnName = "Employee",
				DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted).Select(e => Utilities.modifyUserName(e.Employee.UserName)).Distinct().ToList()
			};

			ColumnFilter generation = new()
			{
				ColumnName = "Generation",
				DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted && !string.IsNullOrEmpty(e.Generation)).Select(e => e.Generation).Distinct().ToList()
			};

			ColumnFilter region = new()
			{
				ColumnName = "Region",
				DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted && !string.IsNullOrEmpty(e.Region)).Select(e => e.Region).Distinct().ToList()
			};

			ColumnFilter city = new()
			{
				ColumnName = "City",
				DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted && !string.IsNullOrEmpty(e.City)).Select(e => e.City).Distinct().ToList()
			};

			ColumnFilter segment = new()
			{
				ColumnName = "Segment",
				//DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted && !string.IsNullOrEmpty(e.Segment)).Select(e => e.Segment).Distinct().ToList()
				DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted && !string.IsNullOrEmpty(e.SegmentName)).Select(e => e.SegmentName).Distinct().ToList()
			};

			ColumnFilter subSegment = new()
			{
				ColumnName = "SubSegment",
				DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted && !string.IsNullOrEmpty(e.SubSegment)).Select(e => e.SubSegment).Distinct().ToList()
			};

			ColumnFilter bundle = new()
			{
				ColumnName = "Bundle",
				DistinctValues = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted && !string.IsNullOrEmpty(e.Bundle)).Select(e => e.Bundle).Distinct().ToList()
			};

			result.Add(lineType);
			result.Add(callStatus);
			result.Add(generation);
			result.Add(region);
			result.Add(city);
			result.Add(segment);
			result.Add(subSegment);
			result.Add(employee);
			result.Add(bundle);

			return result;
		}
		private projectExpectedRemainingDaysViewModel getProjectExpectedRemainingDays(Project project)
		{
			IQueryable<ProjectDetail> query = _db.ProjectDetails
				.Where(e => e.ProjectId == project.Id &&
							!e.Project.IsDeleted &&
							!e.IsDeleted &&
							e.CallStatus.IsClosed);

			//1) Avg closed per day
			var closedPerDay = query
				.GroupBy(g => g.LastUpdatedDate.Value.Date)
				.Select(e => new
				{
					date = e.Key.Date,
					closedCount = e.Count()
				})
				.ToList();

			if (closedPerDay.Count == 0)
				return new projectExpectedRemainingDaysViewModel()
				{
					Key = "Remaining Days",
					RemainingDays = (project.DateTo - project.DateFrom).TotalDays + 1
				};

			double avgClosedPerDay = closedPerDay.Select(e => e.closedCount).ToList().Average();

			//2) Total Closed
			double totalClosed = query.Count();

			//3) project quota
			int quota = _db.Projects.Find(project.Id).Quota;

			//4) remainingDays
			double remainingDays = (quota - totalClosed) / avgClosedPerDay;

			//5) return result
			return new projectExpectedRemainingDaysViewModel()
			{
				Key = "Remaining Days",
				RemainingDays = remainingDays
			};
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
		public ResultWithMessage getProjectTypes()
		{
			var projectTypes = _db
				.ProjectTypes
				.Where(e => e.Name.ToLower() != "n/a")
				.Select(e => new ListViewModel
				{
					Id = e.Id,
					Name = e.Name
				}).ToList();

			return new ResultWithMessage(projectTypes, string.Empty);
		}
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
			Project project = _db.Projects.Include(e => e.ProjectType).SingleOrDefault(e => e.Id == id);
			if (project is null)
				return new ResultWithMessage(null, $"The project with ID: {id} is not Exists");


			//1- Apply Filters just search query
			var query = getProjectDetailsData(id, filter, authData);



			//2- Generate List View Model
			var queryViewModel = convertProjectDetailToListViewModel(query);

			//3- Sorting using our extension
			//filter.SortActive = filter.SortActive == string.Empty ? "ID" : filter.SortActive;

			//if (filter.SortDirection == enSortDirection.desc.ToString())
			//    queryViewModel = queryViewModel.OrderByDescending(filter.SortActive);
			//else
			//    queryViewModel = queryViewModel.OrderBy(filter.SortActive);


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
				//TypeId = project.TypeId,
				TypeId = project.ProjectTypeId,
				//Type = projectTypes.ElementAt(project.TypeId - 1),
				Type = project.ProjectType.Name,
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

				//string filePath = saveFile(model.GSMsFile);
				string filePath = _excelService.SaveFile(model.GSMsFile, "ExcelUploads");
				//List<GSMExcel> gsmExcelList = ExcelHelper.Import<GSMExcel>(filePath);
				List<GSMExcel> gsmExcelList = _excelService.Import<GSMExcel>(filePath,0);

				string validateGsmExcelErrorMessage = validateGSMsExcelFile(model.GSMsFile, gsmExcelList, model.Quota);
				if (!string.IsNullOrEmpty(validateGsmExcelErrorMessage))
					return new ResultWithMessage(null, validateGsmExcelErrorMessage);



				//------------------------------- Excel Validations -------------------------------//
				//1) GSMs validation
				var nullGsm = gsmExcelList.Select((z, i) => new { z.GSM, i }).FirstOrDefault(e => string.IsNullOrEmpty(e.GSM));
				if (nullGsm != null)
					return new ResultWithMessage(null, $"Empty GSM at row number: [{nullGsm.i + 2}]");

				var duplication = gsmExcelList.GroupBy(x => x.GSM).Select(z => new
				{
					z.Key,
					count = z.Count()
				}).FirstOrDefault(y => y.count > 1);

				if (duplication != null)
					return new ResultWithMessage(null, $"Duplicate GSM: [{duplication.Key}]");


				//2) Call Status Validation
				var nullCallStatus = gsmExcelList.Select((z, i) => new { z.CallStatus, i }).FirstOrDefault(e => string.IsNullOrEmpty(e.CallStatus));
				if (nullCallStatus != null)
					return new ResultWithMessage(null, $"Empty Call Status at row number: [{nullCallStatus.i + 2}]");

				var invalidExcelCallStatuses = gsmExcelList
					.Select((z, i) => new { z.CallStatus, i })
					.FirstOrDefault(e => !_db.CallStatuses.Any(s => s.Name.Trim().ToLower() == e.CallStatus.Trim().ToLower()));

				if (invalidExcelCallStatuses != null)
					return new ResultWithMessage(null, $"Invalid Call Status at row number: {invalidExcelCallStatuses.i + 2}, '{invalidExcelCallStatuses.CallStatus}'");


				//3) Regions Validation
				var nullRegions = gsmExcelList.Select((z, i) => new { z.Region, i }).FirstOrDefault(e => string.IsNullOrEmpty(e.Region));
				if (nullRegions != null)
					return new ResultWithMessage(null, $"Empty Region at row number: [{nullRegions.i + 2}]");

				var invalidExcelRegions = gsmExcelList
					.Select((z, i) => new { z.Region, i })
					.FirstOrDefault(e => !regions.Any(r => r.Trim().ToLower() == e.Region.Trim().ToLower()));

				if (invalidExcelRegions != null)
					return new ResultWithMessage(null, $"Invalid Region at row number: {invalidExcelRegions.i + 2}, '{invalidExcelRegions.Region}'");


				//4) Segments Validation
				var nullSegments = gsmExcelList.Select((z, i) => new { z.Segment, i }).FirstOrDefault(e => string.IsNullOrEmpty(e.Segment));
				if (nullSegments != null)
					return new ResultWithMessage(null, $"Empty Segment at row number: [{nullSegments.i + 2}]");

				var invalidExcelSegments = gsmExcelList
					.Select((z, i) => new { z.Segment, i })
					.FirstOrDefault(e => !_db.Segments.Any(s => s.Name.Trim().ToLower() == e.Segment.Trim().ToLower()));

				if (invalidExcelSegments != null)
					return new ResultWithMessage(null, $"Invalid Segment at row number: {invalidExcelSegments.i + 2}, '{invalidExcelSegments.Segment}'");

				//------------------------------- Create Project -------------------------------//

				Project project = new()
				{
					Name = model.Name,
					DateFrom = model.DateFrom,
					DateTo = model.DateTo,
					Quota = model.Quota,
					CreatedBy = authData.userName,
					AddedOn = DateTime.Now,
					ProjectTypeId = model.TypeId,
					ProjectDetails = [],
					ProjectDictionaries = [],
					ProjectMistakeDictionaries = []
				};


				//1) create project details
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
						SegmentName = gsmExcel.Segment,
						SubSegment = gsmExcel.SubSegment,
						Bundle = gsmExcel.Bundle,
						Contract = gsmExcel.Contract,
						AlternativeNumber = gsmExcel.AlternativeNumber,
						Note = gsmExcel.Note,
						AddedOn = DateTime.Now,
						CreatedBy = authData.userName,
						LastUpdatedDate = DateTime.Now,
						LastUpdatedBy = authData.userName,
						EmployeeId = int.Parse(employeeIDs.ElementAt(empIndex)),
					};

					project.ProjectDetails.Add(projectDetail);
				};

				//2) create project default dictionary
				List<ProjectDictionary> projectDefaultDictionary = _db.ProjectTypeDictionaries
					.Where(e => e.ProjectTypeId == model.TypeId && !e.IsDeleted)
					.Select(e => new ProjectDictionary
					{
						RangFrom = e.RangFrom,
						RangTo = e.RangTo,
						Value = e.Value,
						IsDeleted = false,
						CreatedBy = authData.userName,
						AddedOn = DateTime.Now
					})
					.ToList();

				project.ProjectDictionaries.AddRange(projectDefaultDictionary);

				//3) create project default mistake dictionary
				List<ProjectMistakeDictionary> projectDefaultMistakeDictionary = _db.ProjectTypeMistakeDictionaries
					.Where(e => e.ProjectTypeId == model.TypeId && !e.IsDeleted)
					.Select(e => new ProjectMistakeDictionary
					{
						RangFrom = e.RangFrom,
						RangTo = e.RangTo,
						Value = e.Value,
						IsDeleted = false,
						CreatedBy = authData.userName,
						AddedOn = DateTime.Now
					})
					.ToList();

				project.ProjectMistakeDictionaries.AddRange(projectDefaultMistakeDictionary);

				//4) save project
				_db.Projects.Add(project);
				_db.SaveChanges();

				//------------------------------- Send Notification -------------------------------//
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
					//projectToUpdate.DateFrom = model.DateFrom;
					projectToUpdate.DateFrom = Utilities.convertDateToArabStandardDate(model.DateFrom);
					//projectToUpdate.DateTo = model.DateTo;
					projectToUpdate.DateTo = Utilities.convertDateToArabStandardDate(model.DateTo);
					projectToUpdate.Quota = model.Quota;
					//projectToUpdate.TypeId = model.TypeId;
					projectToUpdate.ProjectTypeId = model.TypeId;
					projectToUpdate.LastUpdatedBy = authData.userName;
					projectToUpdate.LastUpdatedDate = DateTime.Now;
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
				project.LastUpdatedDate = DateTime.Now;

				foreach (ProjectDetail projectDetail in project.ProjectDetails)
				{
					projectDetail.IsDeleted = true;
					projectDetail.LastUpdatedBy = authData.userName;
					projectDetail.LastUpdatedDate = DateTime.Now;
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
						projectDetail.LastUpdatedDate = DateTime.Now;
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
		public ResultWithMessage updateProjectDetail(ProjectDetailViewModel model, TenantDto authData)
		{
			ProjectDetail projectDetailToUpdate = _db.ProjectDetails.AsNoTracking().SingleOrDefault(e => e.Id == model.Id);

			if (projectDetailToUpdate is null)
				return new ResultWithMessage(null, $"Empty Project Detail");

			projectDetailToUpdate.CallStatusId = model.CallStatusId;
			projectDetailToUpdate.EmployeeId = model.EmployeeID;
			projectDetailToUpdate.Note = model.Note;
			projectDetailToUpdate.LastUpdatedBy = authData.userName;
			projectDetailToUpdate.LastUpdatedDate = DateTime.Now;

			_db.ProjectDetails.Update(projectDetailToUpdate);
			_db.SaveChanges();

			ProjectDetail updatedProjectDetail = _db.ProjectDetails
				.Include(e => e.Project)
				.Include(e => e.CallStatus)
				.Include(e => e.Employee)
				.SingleOrDefault(e => e.Id == model.Id);

			UpdatedProjectDetailViewModel result = new()
			{
				Id = updatedProjectDetail.Id,
				City = updatedProjectDetail.City,
				AlternativeNumber = updatedProjectDetail.AlternativeNumber,
				Bundle = updatedProjectDetail.Bundle,
				CallStatusId = updatedProjectDetail.CallStatusId,
				CallStatus = updatedProjectDetail.CallStatus.Name,
				Contract = updatedProjectDetail.Contract,
				EmployeeId = updatedProjectDetail.EmployeeId,
				Employee = Utilities.modifyUserName(updatedProjectDetail.Employee.UserName),
				Generation = updatedProjectDetail.Generation,
				GSM = updatedProjectDetail.GSM,
				LineType = updatedProjectDetail.LineType,
				Note = updatedProjectDetail.Note,
				ProjectId = updatedProjectDetail.ProjectId,
				Project = updatedProjectDetail.Project.Name,
				Region = updatedProjectDetail.Region,
				SegmentName = updatedProjectDetail.SegmentName,
				SubSegment = updatedProjectDetail.SubSegment,
				AddedOn = updatedProjectDetail.AddedOn,
				CreatedBy = Utilities.modifyUserName(updatedProjectDetail.CreatedBy),
				IsDeleted = updatedProjectDetail.IsDeleted,
				LastUpdatedBy = Utilities.modifyUserName(updatedProjectDetail.LastUpdatedBy),
				LastUpdateDate = updatedProjectDetail.LastUpdatedDate,
			};

			return new ResultWithMessage(result, string.Empty);
		}
		public ByteResultWithMessage exportProjectDetailsToExcel(int projectId, TenantDto authData)
		{
			var query = _db.ProjectDetails
				.Include(e => e.Project)
				.Where(e => e.ProjectId == projectId && !e.IsDeleted);

			if (!query.Any())
				return new ByteResultWithMessage(null, $"Invalid project Id: {projectId}");


			if (authData.tenantAccesses[0].RoleList.Contains(enRoles.Telemarketer.ToString()))
			{
				Employee employee = _db.Employees.Single(e => e.UserName == authData.userName);
				query = query.Where(e => e.EmployeeId == employee.Id);
			}

			List<ProjectDetailsDataToExcel> projectDetailsDataToExcels = [.. query.Select(e => new ProjectDetailsDataToExcel
			{
				CreatedBy = Utilities.modifyUserName(e.CreatedBy),
				Region = e.Region,
				LineType = e.LineType,
				AddedOn = e.AddedOn.ToString("dd/MM/yyyy HH:mm:ss"),
				AlternativeNumber = e.AlternativeNumber,
				Bundle = e.Bundle,
				CallStatus = e.CallStatus.Name,
				City = e.City,
				Contract = e.Contract,
				Employee = Utilities.modifyUserName(e.Employee.UserName),
				Generation = e.Generation,
				GSM = e.GSM,
				LastUpdatedby = Utilities.modifyUserName(e.LastUpdatedBy),
				Note = e.Note,
				Segment = e.SegmentName,
				SubSegment = e.SubSegment,
				LastUpdatedDate = e.LastUpdatedDate.Value.ToString("dd/MM/yyyy HH:mm:ss")
			})];

			string projectName = query.FirstOrDefault().Project.Name;
			byte[] excelData = _excelService.Export(projectDetailsDataToExcels, $"{projectName}'s Details");

			return new ByteResultWithMessage(excelData, string.Empty);
		}
		public ByteResultWithMessage exportProjectsToExcel()
		{
			var data = _db.Projects
				.Include(e => e.ProjectType)
				.Where(e => !e.IsDeleted)
			   .Select(e => new ProjectDataToExcel
			   {
				   Id = e.Id,
				   Name = e.Name,
				   Type = e.ProjectType.Name,
				   DateFrom = e.DateFrom.ToString("dd/MM/yyyy"),
				   DateTo = e.DateTo.ToString("dd/MM/yyyy"),
				   Quota = e.Quota,
				   CreatedBy = Utilities.modifyUserName(e.CreatedBy),
				   AddedOn = e.AddedOn.ToString("dd/MM/yyyy HH:mm:ss"),
				   LastUpdatedBy = Utilities.modifyUserName(e.LastUpdatedBy),
				   LastUpdateDate = e.LastUpdatedDate.Value.ToString("dd/MM/yyyy HH:mm:ss"),
			   }).ToList();

			if (data.Count == 0)
				return new ByteResultWithMessage(null, $"No data found");

			byte[] excelData = _excelService.Export(data, "Projects");

			return new ByteResultWithMessage(excelData, string.Empty);
		}
		public ResultWithMessage getAdmins()
		{
			List<EmployeeViewModel> admins = _db.UserTenantRoles
				.Where(e => e.RoleId == 1)
				.Select(e => new EmployeeViewModel
				{
					Id = e.Id,
					UserName = Utilities.modifyUserName(e.UserName)
				})
				.ToList();

			return new ResultWithMessage(admins, string.Empty);
		}

		public ResultWithMessage projectExpectedRemainingDays(int projectId)
		{
			Project project = _db.Projects.Find(projectId);
			if (project is null)
				return new ResultWithMessage(null, $"Invalid Project Id: {projectId}");

			return new ResultWithMessage(getProjectExpectedRemainingDays(project), string.Empty);
		}
	}
}