using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models;
using TelemarketingControlSystem.Models.Data;

namespace TelemarketingControlSystem.Services.ProjectsEvaluation
{
    public interface IProjectsEvaluationService
    {
        ResultWithMessage getProjectTypeDictionary(int projecTypeId);
    }

    public class ProjectsEvaluationService(ApplicationDbContext db) : IProjectsEvaluationService
    {
        private readonly ApplicationDbContext _db = db;

        public ResultWithMessage getProjectTypeDictionary(int projecTypeId)
        {
            ProjectType projectType = _db.ProjectTypes.Find(projecTypeId);

            if (projectType == null)
                return new ResultWithMessage(null, $"Invalid project type Id: {projecTypeId}");

            List<ProjectTypeDictionaryViewModel> result = _db
                .TypeDictionaries
                .Where(e => e.ProjectTypeId == projecTypeId && !e.IsDeleted)
                .Include(e => e.ProjectType)
                .Select(e => new ProjectTypeDictionaryViewModel
                {
                    Id = e.Id,
                    RangFrom = e.RangFrom,
                    RangTo = e.RangTo,
                    Value = e.Value,
                    IsDeleted = e.IsDeleted,
                    CreatedBy = e.CreatedBy,
                    AddedOn = e.AddedOn,
                    LastUpdatedBy = e.LastUpdatedBy,
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