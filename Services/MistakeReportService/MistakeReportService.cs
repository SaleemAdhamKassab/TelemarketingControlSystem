using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models;
using TelemarketingControlSystem.Models.Data;
using static TelemarketingControlSystem.Services.Auth.AuthModels;
using TelemarketingControlSystem.Services.ProjectEvaluationService;

namespace TelemarketingControlSystem.Services.MistakeReportService
{
	public interface IMistakeReportService
	{
		ResultWithMessage projectTypeMistakeDictionary(int projectTypeId);
		ResultWithMessage updateProjectTypeMistakeDictionary(UpdateProjectTypeMistakeDictionaryDto dto, TenantDto authData);
		ResultWithMessage projectMistakeDictionary(int projectId);
		ResultWithMessage updateProjectMistakeDictionary(UpdateProjectMistakeDictionaryDto dto, TenantDto authData);
		ResultWithMessage getProjectMistakeDictionary(int projectId);
	}
	public class MistakeReportService : IMistakeReportService
	{
		private readonly ApplicationDbContext _db;
		public MistakeReportService(ApplicationDbContext db)
		{
			_db = db;
		}

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
	}
}
