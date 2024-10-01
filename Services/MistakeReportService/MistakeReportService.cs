using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models;
using TelemarketingControlSystem.Models.Data;

namespace TelemarketingControlSystem.Services.MistakeReportService
{
	public interface IMistakeReportService
	{
		ResultWithMessage getMistakeDictionaryType(int projectTypeId);
	}
	public class MistakeReportService : IMistakeReportService
	{
		private readonly ApplicationDbContext _db;
		public MistakeReportService(ApplicationDbContext db)
		{
			_db = db;
		}

		public ResultWithMessage getMistakeDictionaryType(int projectTypeId)
		{
			ProjectType projectType = _db.ProjectTypes.Find(projectTypeId);

			if (projectType == null)
				return new ResultWithMessage(null, $"Invalid project type Id: {projectTypeId}");

			List<MistakeDictionaryTypeViewModel> result = _db
				.TypeDictionaries
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
	}
}
