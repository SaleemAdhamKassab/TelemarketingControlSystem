using Microsoft.EntityFrameworkCore;
using System.DirectoryServices.Protocols;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models;
using TelemarketingControlSystem.Models.Data;
using static TelemarketingControlSystem.Services.Auth.AuthModels;

namespace TelemarketingControlSystem.Services.ProjectsEvaluation
{
	public interface IProjectsEvaluationService
	{
		ResultWithMessage getProjectTypeDictionary(int projectTypeId);
		ResultWithMessage updateProjectTypeDictionary(UpdateProjectTypeDictionaryDto updateProjectTypeDictionaryDto, TenantDto authData);
		ResultWithMessage getProjectDictionary(int projectId);
		ResultWithMessage updateProjectDictionary(UpdateProjectDictionaryDto updateProjectDictionaryDto, TenantDto authData);
		ResultWithMessage getProjectSegmentEvaluationCards(int projectId, string segmentName);
		ResultWithMessage getProjectSegmentTelemarketersEvaluations(ProjectSegmentTelemarketersEvaluationsDto dto);
	}

	public class ProjectsEvaluationService(ApplicationDbContext db) : IProjectsEvaluationService
	{
		private readonly ApplicationDbContext _db = db;
		private void disableOldDictionary(List<TypeDictionary> typeDictionariesToDelete, string userName)
		{
			foreach (TypeDictionary typeDictionary in typeDictionariesToDelete)
			{
				typeDictionary.IsDeleted = true;
				typeDictionary.LastUpdatedBy = userName;
				typeDictionary.LastUpdatedDate = DateTime.Now;
			}
			_db.UpdateRange(typeDictionariesToDelete);
		}
		private void disableProjectOldDictionary(List<ProjectDictionary> projectDictionaries, string userName)
		{
			foreach (ProjectDictionary projectDictionary in projectDictionaries)
			{
				projectDictionary.IsDeleted = true;
				projectDictionary.LastUpdatedBy = userName;
				projectDictionary.LastUpdatedDate = DateTime.Now;
			}
			_db.UpdateRange(projectDictionaries);
		}
		private List<TypeDictionary> getTypeDictionaryRanges(int projecttypeId, List<DictionaryRange> dictionaryRanges, string userName)
		{
			List<TypeDictionary> typeDictionaries = [];
			foreach (DictionaryRange dictionaryRange in dictionaryRanges)
			{
				TypeDictionary typeDictionary = new()
				{
					RangFrom = dictionaryRange.RangFrom,
					RangTo = dictionaryRange.RangTo,
					Value = dictionaryRange.Value,
					IsDeleted = false,
					CreatedBy = userName,
					AddedOn = DateTime.Now,
					ProjectTypeId = projecttypeId
				};

				typeDictionaries.Add(typeDictionary);
			}

			return typeDictionaries;
		}
		private List<ProjectDictionary> getProjectDictionaryRanges(int projectId, List<DictionaryRange> dictionaryRanges, string userName)
		{
			List<ProjectDictionary> projectDictionaries = [];
			foreach (DictionaryRange dictionaryRange in dictionaryRanges)
			{
				ProjectDictionary projectDictionary = new()
				{
					RangFrom = dictionaryRange.RangFrom,
					RangTo = dictionaryRange.RangTo,
					Value = dictionaryRange.Value,
					IsDeleted = false,
					CreatedBy = userName,
					AddedOn = DateTime.Now,
					ProjectId = projectId
				};

				projectDictionaries.Add(projectDictionary);
			}

			return projectDictionaries;
		}

		private bool isValidRanges(List<DictionaryRange> dictionaryRanges)
		{
			List<double> ranges = [];

			foreach (DictionaryRange range in dictionaryRanges)
			{
				ranges.Add(range.RangFrom);
				ranges.Add(range.RangTo);
			}

			List<double> sortedRanges = ranges.OrderBy(e => e).ToList();

			if (ranges.SequenceEqual(sortedRanges))
				return true;

			return false;
		}

		private double getProjectSegmentTotalWorkingHours(int projectId, string segmentName)
		{
			List<EmployeeWorkingHour> employeeWorkingHours = _db.EmployeeWorkingHours
				.Include(e => e.Project)
				.Where(e => e.ProjectId == projectId &&
							!e.Project.IsDeleted &&
							e.SegmentName == segmentName)
				.ToList();

			if (employeeWorkingHours is null)
				return 0;

			double result = employeeWorkingHours.Sum(e => e.WorkingHours);
			//return Math.Round(result);
			return result;
		}
		private double getProjectSegmentTotalClosed(int projectId, string segmentName)
		{
			double projectSegmentTotalClosed = _db
				.ProjectDetails
				.Include(e => e.Project)
				.Where(e => e.ProjectId == projectId &&
							!e.Project.IsDeleted &&
							!e.IsDeleted &&
							e.SegmentName == segmentName &&
							e.CallStatus.IsClosed)
				.Count();

			//return Math.Round(projectSegmentTotalClosed);
			return projectSegmentTotalClosed;
		}

		private double getProjecSegmentTarget(int projectId, string segmentName)
		{
			double projectSegmentTotalWorkingHours = getProjectSegmentTotalWorkingHours(projectId, segmentName);

			double projectSegmentTotalClosed = getProjectSegmentTotalClosed(projectId, segmentName);

			double projecSegmentTarget = projectSegmentTotalWorkingHours > 0 ? (projectSegmentTotalClosed / projectSegmentTotalWorkingHours) : 0;

			//return Math.Round(projecSegmentTarget);
			return projecSegmentTarget;
		}







		public ResultWithMessage getProjectTypeDictionary(int projectTypeId)
		{
			ProjectType projectType = _db.ProjectTypes.Find(projectTypeId);

			if (projectType == null)
				return new ResultWithMessage(null, $"Invalid project type Id: {projectTypeId}");

			List<ProjectTypeDictionaryViewModel> result = _db
				.TypeDictionaries
				.Where(e => e.ProjectTypeId == projectTypeId && !e.IsDeleted)
				.Include(e => e.ProjectType)
				.Select(e => new ProjectTypeDictionaryViewModel
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
		public ResultWithMessage updateProjectTypeDictionary(UpdateProjectTypeDictionaryDto updateProjectTypeDictionaryDto, TenantDto authData)
		{
			ProjectType projectType = _db.ProjectTypes.Find(updateProjectTypeDictionaryDto.ProjectTypeId);

			if (projectType is null)
				return new ResultWithMessage(null, $"Invalid project type Id: {updateProjectTypeDictionaryDto.ProjectTypeId}");

			List<TypeDictionary> typeDictionariesToDelete = _db.TypeDictionaries.Where(e => e.ProjectTypeId == updateProjectTypeDictionaryDto.ProjectTypeId).ToList();

			if (typeDictionariesToDelete.Count == 0)
				return new ResultWithMessage(null, $"No dictionary found to delete with Id : {updateProjectTypeDictionaryDto.ProjectTypeId}");

			//1) Validate range sequence
			if (!isValidRanges(updateProjectTypeDictionaryDto.DictionaryRanges))
				return new ResultWithMessage(null, "Invalid ranges");

			//2) Disable old dictionary
			disableOldDictionary(typeDictionariesToDelete, authData.userName);

			//3) Add new dictionary
			List<TypeDictionary> typeDictionaryRanges = getTypeDictionaryRanges(updateProjectTypeDictionaryDto.ProjectTypeId, updateProjectTypeDictionaryDto.DictionaryRanges, authData.userName);
			_db.TypeDictionaries.AddRange(typeDictionaryRanges);

			_db.SaveChanges();

			return getProjectTypeDictionary(updateProjectTypeDictionaryDto.ProjectTypeId);
		}
		public ResultWithMessage getProjectDictionary(int projectId)
		{
			Project project = _db.Projects.Find(projectId);

			if (project == null)
				return new ResultWithMessage(null, $"Invalid project Id: {projectId}");

			List<ProjectDictionaryViewModel> result = _db
				.ProjectDictionaries
				.Where(e => e.ProjectId == projectId && !e.IsDeleted)
				.Include(e => e.Project)
				.Select(e => new ProjectDictionaryViewModel
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
		public ResultWithMessage updateProjectDictionary(UpdateProjectDictionaryDto updateProjectDictionaryDto, TenantDto authData)
		{
			Project project = _db.Projects.Find(updateProjectDictionaryDto.projectId);

			if (project is null)
				return new ResultWithMessage(null, $"Invalid project Id: {updateProjectDictionaryDto.projectId}");

			//1) Validate range sequence
			if (!isValidRanges(updateProjectDictionaryDto.DictionaryRanges))
				return new ResultWithMessage(null, "Invalid ranges");

			//2) disable Project old dictionary
			List<ProjectDictionary> projectDictionariesToDelete = _db.ProjectDictionaries.Where(e => e.ProjectId == updateProjectDictionaryDto.projectId).ToList();
			disableProjectOldDictionary(projectDictionariesToDelete, authData.userName);

			//3) Add new dictionary
			List<ProjectDictionary> ProjectDictionaries = getProjectDictionaryRanges(updateProjectDictionaryDto.projectId, updateProjectDictionaryDto.DictionaryRanges, authData.userName);
			_db.ProjectDictionaries.AddRange(ProjectDictionaries);

			_db.SaveChanges();

			return getProjectDictionary(updateProjectDictionaryDto.projectId);
		}
		public ResultWithMessage getProjectSegmentEvaluationCards(int projectId, string segmentName)
		{
			Project project = _db.Projects.Where(e => e.Id == projectId && !e.IsDeleted).FirstOrDefault();
			if (project is null)
				return new ResultWithMessage(null, $"Invalid project Id: {projectId}");

			Segment segment = _db.Segments.ToList().FirstOrDefault(e => e.Name == segmentName);
			if (segment is null)
				return new ResultWithMessage(null, $"Invalid segment name: '{segmentName}'");


			//1) Total working hours
			ProjectSegmentEvaluationCardDetails totalWorkingHoursCard = new()
			{
				CardTitle = "Total Working Hours",
				Value = getProjectSegmentTotalWorkingHours(projectId, segmentName)
			};

			//2) Total Closed
			ProjectSegmentEvaluationCardDetails totalClosedCard = new()
			{
				CardTitle = "Total Closed",
				Value = getProjectSegmentTotalClosed(projectId, segmentName)
			};

			//3) Target
			ProjectSegmentEvaluationCardDetails target = new()
			{
				CardTitle = "Segment Target",
				Value = getProjecSegmentTarget(projectId, segmentName)
			};

			//3) Project Quota
			ProjectSegmentEvaluationCardDetails quota = new()
			{
				CardTitle = "Project Quota",
				Value = project.Quota
			};

			//4) Result
			List<ProjectSegmentEvaluationCardDetails> result = [];
			result.Add(totalWorkingHoursCard);
			result.Add(totalClosedCard);
			result.Add(target);
			result.Add(quota);


			return new ResultWithMessage(result, string.Empty);
		}

		public ResultWithMessage getProjectSegmentTelemarketersEvaluations(ProjectSegmentTelemarketersEvaluationsDto dto)
		{
			Project project = _db.Projects.Where(e => e.Id == dto.ProjectId && !e.IsDeleted).FirstOrDefault();
			if (project is null)
				return new ResultWithMessage(null, $"Invalid project Id: {dto.ProjectId}");

			Segment segment = _db.Segments.SingleOrDefault(e => e.Name == dto.SegmentName);
			if (segment is null)
				return new ResultWithMessage(null, $"Invalid segment name: '{dto.SegmentName}'");

			double projecSegmentTarget = getProjecSegmentTarget(dto.ProjectId, dto.SegmentName);

			var result = _db.EmployeeWorkingHours
				.Include(e => e.Project)
				.Include(e => e.Employee)
				.Where(e => e.ProjectId == dto.ProjectId &&
							!e.Project.IsDeleted &&
							e.SegmentName == dto.SegmentName)
				.GroupBy(g => g.Employee.UserName)
				.Select(e => new ProjectSegmentTelemarketersEvaluationsViewModel
				{
					EmployeeUserName = Utilities.modifyUserName(e.Key),
					Segment = dto.SegmentName,
					WorkingHours = e.Sum(w => w.WorkingHours),
					Closed = _db.ProjectDetails
							.Include(e => e.CallStatus)
							.Where(pd => pd.ProjectId == dto.ProjectId &&
										pd.Employee.UserName == e.Key &&
										pd.SegmentName == dto.SegmentName &&
										pd.CallStatus.IsClosed)
							.Count(),
					ClosedPerHour =
							e.Sum(w => w.WorkingHours) > 0 ?
							_db.ProjectDetails
							.Include(e => e.CallStatus)
							.Where(pd => pd.ProjectId == dto.ProjectId &&
										pd.Employee.UserName == e.Key &&
										pd.SegmentName == dto.SegmentName &&
										pd.CallStatus.IsClosed)
							.Count() / e.Sum(w => w.WorkingHours)
							: 0,
					SegmentTarget = projecSegmentTarget,
					Achievement =
							projecSegmentTarget > 0 ?
							(_db.ProjectDetails
							.Include(e => e.CallStatus)
							.Where(pd => pd.ProjectId == dto.ProjectId &&
										pd.Employee.UserName == e.Key &&
										pd.SegmentName == dto.SegmentName &&
										pd.CallStatus.IsClosed)
							.Count() / projecSegmentTarget)
							: 0,
					Mark = _db.ProjectDictionaries
							  .Where(pdic => pdic.ProjectId == dto.ProjectId &&
										   !pdic.IsDeleted &&
											(projecSegmentTarget > 0 ? (_db.ProjectDetails.Include(e => e.CallStatus).Where(pd => pd.ProjectId == dto.ProjectId && pd.Employee.UserName == e.Key && pd.SegmentName == dto.SegmentName && pd.CallStatus.IsClosed).Count() / projecSegmentTarget) : 0) >= pdic.RangFrom &&
											(projecSegmentTarget > 0 ? (_db.ProjectDetails.Include(e => e.CallStatus).Where(pd => pd.ProjectId == dto.ProjectId && pd.Employee.UserName == e.Key && pd.SegmentName == dto.SegmentName && pd.CallStatus.IsClosed).Count() / projecSegmentTarget) : 0) <= pdic.RangTo)
							  .FirstOrDefault() == null ?null
							  : _db.ProjectDictionaries
							  .Where(pdic => pdic.ProjectId == dto.ProjectId &&
										   !pdic.IsDeleted &&
											(projecSegmentTarget > 0 ? (_db.ProjectDetails.Include(e => e.CallStatus).Where(pd => pd.ProjectId == dto.ProjectId && pd.Employee.UserName == e.Key && pd.SegmentName == dto.SegmentName && pd.CallStatus.IsClosed).Count() / projecSegmentTarget) : 0) >= pdic.RangFrom &&
											(projecSegmentTarget > 0 ? (_db.ProjectDetails.Include(e => e.CallStatus).Where(pd => pd.ProjectId == dto.ProjectId && pd.Employee.UserName == e.Key && pd.SegmentName == dto.SegmentName && pd.CallStatus.IsClosed).Count() / projecSegmentTarget) : 0) <= pdic.RangTo)
							  .FirstOrDefault().Value
				})
				.ToList();

			return new ResultWithMessage(result, string.Empty);
		}
	}
}