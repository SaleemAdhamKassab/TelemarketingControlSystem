using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models;
using TelemarketingControlSystem.Models.Data;
using static TelemarketingControlSystem.Services.Auth.AuthModels;
using TelemarketingControlSystem.Services.ProjectEvaluationService;
using TelemarketingControlSystem.Services.ExcelService;
using TelemarketingControlSystem.Services.ProjectService;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection.Metadata.Ecma335;

namespace TelemarketingControlSystem.Services.MistakeReportService
{
	public interface IMistakeReportService
	{
		ResultWithMessage projectTypeMistakeDictionary(int projectTypeId);
		ResultWithMessage updateProjectTypeMistakeDictionary(UpdateProjectTypeMistakeDictionaryDto dto, TenantDto authData);
		ResultWithMessage projectMistakeDictionary(int projectId);
		ResultWithMessage updateProjectMistakeDictionary(UpdateProjectMistakeDictionaryDto dto, TenantDto authData);
		ResultWithMessage getProjectMistakeDictionary(int projectId);
		Task<ResultWithMessage> UploadMistakeReportAsync(UploadMistakeReportRequest request, TenantDto authData);
	}

	public class MistakeReportService(ApplicationDbContext db, IExcelService excelService) : IMistakeReportService
	{
		private readonly ApplicationDbContext _db = db;
		private readonly IExcelService _excelService = excelService;

		private void disableOldProjectTypeMistakeDictionary(List<ProjectTypeMistakeDictionary> projectTypeMistakeDictionaries, string userName)
		{
			foreach (ProjectTypeMistakeDictionary projectTypeMistakeDictionary in projectTypeMistakeDictionaries)
			{
				projectTypeMistakeDictionary.IsDeleted = true;
				projectTypeMistakeDictionary.LastUpdatedBy = userName;
				projectTypeMistakeDictionary.LastUpdatedDate = DateTime.Now;
			}

			_db.UpdateRange(projectTypeMistakeDictionaries);
		}
		private List<ProjectTypeMistakeDictionary> getProjectTypeMistakeDictionary(int projecttypeId, List<DictionaryRange> dictionaryRanges, string userName)
		{
			List<ProjectTypeMistakeDictionary> projectTypeMistakeDictionaries = [];

			foreach (DictionaryRange dictionaryRange in dictionaryRanges)
			{
				ProjectTypeMistakeDictionary projectTypeMistakeDictionary = new()
				{
					RangFrom = dictionaryRange.RangFrom,
					RangTo = dictionaryRange.RangTo,
					Value = dictionaryRange.Value,
					IsDeleted = false,
					CreatedBy = userName,
					AddedOn = DateTime.Now,
					ProjectTypeId = projecttypeId
				};

				projectTypeMistakeDictionaries.Add(projectTypeMistakeDictionary);
			}

			return projectTypeMistakeDictionaries;
		}

		private void disableOldMistakeProjectDictionary(List<ProjectMistakeDictionary> projectMistakeDictionaries, string userName)
		{
			foreach (ProjectMistakeDictionary projectMistakeDictionary in projectMistakeDictionaries)
			{
				projectMistakeDictionary.IsDeleted = true;
				projectMistakeDictionary.LastUpdatedBy = userName;
				projectMistakeDictionary.LastUpdatedDate = DateTime.Now;
			}
			_db.UpdateRange(projectMistakeDictionaries);
		}
		private List<ProjectMistakeDictionary> getProjectMistakeDictionaryRanges(int projectId, List<DictionaryRange> dictionaryRanges, string userName)
		{
			List<ProjectMistakeDictionary> projectMistakeDictionaries = [];

			foreach (DictionaryRange dictionaryRange in dictionaryRanges)
			{
				ProjectMistakeDictionary projectMistakeDictionary = new()
				{
					RangFrom = dictionaryRange.RangFrom,
					RangTo = dictionaryRange.RangTo,
					Value = dictionaryRange.Value,
					IsDeleted = false,
					CreatedBy = userName,
					AddedOn = DateTime.Now,
					ProjectId = projectId
				};

				projectMistakeDictionaries.Add(projectMistakeDictionary);
			}

			return projectMistakeDictionaries;
		}


		public ResultWithMessage projectTypeMistakeDictionary(int projectTypeId)
		{
			ProjectType projectType = _db.ProjectTypes.Find(projectTypeId);

			if (projectType == null)
				return new ResultWithMessage(null, $"Invalid project type Id: {projectTypeId}");

			List<MistakeDictionaryTypeViewModel> result = _db
				.ProjectTypeMistakeDictionaries
				.Where(e => e.ProjectTypeId == projectTypeId && !e.IsDeleted)
				.Include(e => e.ProjectType)
				.Select(e => new MistakeDictionaryTypeViewModel
				{
					Id = e.Id,
					RangFrom = e.RangFrom,
					RangTo = e.RangTo,
					Value = e.Value,
					IsDeleted = e.IsDeleted,
					CreatedBy = Utilities.modifyUserName(e.CreatedBy),
					AddedOn = e.AddedOn,
					LastUpdatedBy = Utilities.modifyUserName(e.LastUpdatedBy),
					LastUpdatedDate = e.LastUpdatedDate,
					ProjectTypeId = e.ProjectTypeId,
					ProjectType = e.ProjectType.Name
				})
				.OrderBy(e => e.RangFrom)
				.ToList();

			return new ResultWithMessage(result, string.Empty);

		}
		public ResultWithMessage getProjectTypeMistakeDictionary(int projectTypeId)
		{
			ProjectType projectType = _db.ProjectTypes.Find(projectTypeId);

			if (projectType == null)
				return new ResultWithMessage(null, $"Invalid project type Id: {projectTypeId}");

			List<ProjectTypeMistakeDictionaryViewModel> result = _db
				.ProjectTypeMistakeDictionaries
				.Where(e => e.ProjectTypeId == projectTypeId && !e.IsDeleted)
				.Include(e => e.ProjectType)
				.Select(e => new ProjectTypeMistakeDictionaryViewModel
				{
					Id = e.Id,
					RangFrom = e.RangFrom,
					RangTo = e.RangTo,
					Value = e.Value,
					IsDeleted = e.IsDeleted,
					CreatedBy = Utilities.modifyUserName(e.CreatedBy),
					AddedOn = e.AddedOn,
					LastUpdatedBy = Utilities.modifyUserName(e.LastUpdatedBy),
					LastUpdatedDate = e.LastUpdatedDate,
					ProjectTypeId = e.ProjectTypeId,
					ProjectType = e.ProjectType.Name
				})
				.OrderBy(e => e.RangFrom)
				.ToList();

			return new ResultWithMessage(result, string.Empty);
		}
		public ResultWithMessage updateProjectTypeMistakeDictionary(UpdateProjectTypeMistakeDictionaryDto dto, TenantDto authData)
		{
			ProjectType projectType = _db.ProjectTypes.Find(dto.ProjectTypeId);

			if (projectType is null)
				return new ResultWithMessage(null, $"Invalid project type Id: {dto.ProjectTypeId}");

			List<ProjectTypeMistakeDictionary> projectTypeMistakeDictionaryToDelete =
				_db.ProjectTypeMistakeDictionaries
				.Where(e => e.ProjectTypeId == dto.ProjectTypeId)
				.ToList();

			if (projectTypeMistakeDictionaryToDelete.Count == 0)
				return new ResultWithMessage(null, $"No Project Type Mistake Dictionary found to delete with Id : {dto.ProjectTypeId}");

			//1) Validate range sequence
			if (!Utilities.isValidDictionaryRanges(dto.DictionaryRanges))
				return new ResultWithMessage(null, "Invalid ranges");

			//2) Disable Old Project Type Mistake Dictionary
			disableOldProjectTypeMistakeDictionary(projectTypeMistakeDictionaryToDelete, authData.userName);

			//3) Add new Project Type Mistake Dictionary
			List<ProjectTypeMistakeDictionary> projectTypeDictionaryRanges = getProjectTypeMistakeDictionary(dto.ProjectTypeId, dto.DictionaryRanges, authData.userName);
			_db.ProjectTypeMistakeDictionaries.AddRange(projectTypeDictionaryRanges);

			_db.SaveChanges();

			return getProjectTypeMistakeDictionary(dto.ProjectTypeId);
		}
		public ResultWithMessage projectMistakeDictionary(int projectId)
		{
			Project project = _db.Projects.Find(projectId);

			if (project == null)
				return new ResultWithMessage(null, $"Invalid project Id: {projectId}");

			List<ProjectMistakeDictionaryViewModel> result = _db
				.ProjectMistakeDictionaries
				.Where(e => e.ProjectId == projectId && !e.IsDeleted)
				.Include(e => e.Project)
				.Select(e => new ProjectMistakeDictionaryViewModel
				{
					Id = e.Id,
					RangFrom = e.RangFrom,
					RangTo = e.RangTo,
					Value = e.Value,
					IsDeleted = e.IsDeleted,
					CreatedBy = Utilities.modifyUserName(e.CreatedBy),
					AddedOn = e.AddedOn,
					LastUpdatedBy = e.LastUpdatedBy,
					LastUpdatedDate = e.LastUpdatedDate,
					ProjectId = e.ProjectId,
					Project = e.Project.Name
				})
				.OrderBy(e => e.RangFrom)
				.ToList();

			return new ResultWithMessage(result, string.Empty);
		}
		public ResultWithMessage updateProjectMistakeDictionary(UpdateProjectMistakeDictionaryDto dto, TenantDto authData)
		{
			Project project = _db.Projects.Find(dto.projectId);

			if (project is null)
				return new ResultWithMessage(null, $"Invalid project Id: {dto.projectId}");

			//1) Validate range sequence
			if (!Utilities.isValidDictionaryRanges(dto.DictionaryRanges))
				return new ResultWithMessage(null, "Invalid ranges");

			//2) disable Project old mistake dictionary
			List<ProjectMistakeDictionary> projectMistakeDictionariesToDelete = _db.ProjectMistakeDictionaries.Where(e => e.ProjectId == dto.projectId).ToList();
			disableOldMistakeProjectDictionary(projectMistakeDictionariesToDelete, authData.userName);

			//3) Add new mistake dictionary
			List<ProjectMistakeDictionary> ProjectMistakeDictionaries = getProjectMistakeDictionaryRanges(dto.projectId, dto.DictionaryRanges, authData.userName);
			_db.ProjectMistakeDictionaries.AddRange(ProjectMistakeDictionaries);

			_db.SaveChanges();

			return getProjectMistakeDictionary(dto.projectId);
		}
		public ResultWithMessage getProjectMistakeDictionary(int projectId)
		{
			Project project = _db.Projects.Find(projectId);

			if (project is null)
				return new ResultWithMessage(null, $"Invalid project Id: {projectId}");

			List<ProjectMistakeDictionaryViewModel> result = _db
				.ProjectMistakeDictionaries
				.Where(e => e.ProjectId == projectId && !e.IsDeleted)
				.Include(e => e.Project)
				.Select(e => new ProjectMistakeDictionaryViewModel
				{
					Id = e.Id,
					RangFrom = e.RangFrom,
					RangTo = e.RangTo,
					Value = e.Value,
					IsDeleted = e.IsDeleted,
					CreatedBy = Utilities.modifyUserName(e.CreatedBy),
					AddedOn = e.AddedOn,
					LastUpdatedBy = e.LastUpdatedBy,
					LastUpdatedDate = e.LastUpdatedDate,
					ProjectId = e.ProjectId,
					Project = e.Project.Name
				})
				.OrderBy(e => e.RangFrom)
				.ToList();

			return new ResultWithMessage(result, string.Empty);
		}
		public async Task<ResultWithMessage> UploadMistakeReportAsync(UploadMistakeReportRequest request, TenantDto authData)
		{
			Project project = await _db.Projects.FindAsync(request.ProjectId);
			if (project is null)
				return new ResultWithMessage(null, $"Invalid project id: {request.ProjectId}");

			//------------------------------------ Sheet1: Mistakes Validations ------------------------------------//
			string filePath = _excelService.SaveFile(request.MistakeReport, "MistakeReports");
			List<ExcelMistakeReport> mistakeReportList = _excelService.Import<ExcelMistakeReport>(filePath, 0);

			//1) Survey Name
			var nullSurveyName = mistakeReportList.Select((z, i) => new { z.SurveyName, i }).FirstOrDefault(e => string.IsNullOrEmpty(e.SurveyName));
			if (nullSurveyName is not null)
				return new ResultWithMessage(null, $"Empty Survey Name at row number: [{nullSurveyName.i + 2}]");


			//2) Telemarketers
			var nullTelemarketer = mistakeReportList.Select((z, i) => new { z.TelemarketerName, i }).FirstOrDefault(e => string.IsNullOrEmpty(e.TelemarketerName));
			if (nullTelemarketer is not null)
				return new ResultWithMessage(null, $"Empty Telemarketer at row number: [{nullTelemarketer.i + 2}]");

			var invalidTelemarketrName = mistakeReportList
					.Select((z, i) => new { z.TelemarketerName, i })
					.FirstOrDefault(e => !_db.Employees.Any(s => s.UserName.Trim().ToLower() == "Syriatel\\" + e.TelemarketerName.Trim().ToLower()));

			//3) Mistake Types
			var nullMistakeType = mistakeReportList.Select((z, i) => new { z.MistakeType, i }).FirstOrDefault(e => string.IsNullOrEmpty(e.MistakeType));
			if (nullMistakeType is not null)
				return new ResultWithMessage(null, $"Empty Mistake Type");

			var invalidMistakeType = mistakeReportList
					.Select((z, i) => new { z.MistakeType, i })
					.FirstOrDefault(e => !_db.MistakeTypes.Any(s => s.Name.Trim().ToLower() == e.MistakeType.Trim().ToLower()));

			if (invalidMistakeType is not null)
				return new ResultWithMessage(null, $"Invalid Telemarketer Name at row number: {invalidMistakeType.i + 2}, '{invalidMistakeType.MistakeType}'");

			//------------------------------- Create Mistake Report -------------------------------//

			List<MistakeReport> mistakeReportData = [];

			foreach (ExcelMistakeReport row in mistakeReportList)
			{
				MistakeReport mistakeReport = new()
				{
					GSM = row.GSM,
					Controller = row.Controller,
					QuestionNumber = row.QuestionNumber,
					Segment = row.Segment,
					Serial = row.Serial,
					IsDeleted = false,
					AddedOn = DateTime.Now,
					ProjectId = project.Id,
					EmployeeId = _db.Employees.FirstOrDefault(e => e.UserName.Trim().ToLower() == ("Syriatel\\" + row.TelemarketerName).Trim().ToLower()).Id,
					MistakeTypeName = _db.MistakeTypes.FirstOrDefault(e => e.Name.Trim().ToLower() == row.MistakeType.Trim().ToLower()).Name,
					CreatedBy = authData.userName,
				};

				mistakeReportData.Add(mistakeReport);
			};

			_db.MistakeReports.AddRange(mistakeReportData);
			_db.SaveChanges();

			return new ResultWithMessage(null, string.Empty);
		}
	}
}
