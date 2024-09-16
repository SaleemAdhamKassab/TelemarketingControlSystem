using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models;
using TelemarketingControlSystem.Models.Data;
using static TelemarketingControlSystem.Services.Auth.AuthModels;

namespace TelemarketingControlSystem.Services.ProjectsEvaluation
{
    public interface IProjectsEvaluationService
    {
        ResultWithMessage getProjectTypeDictionary(int projecTypeId);
        ResultWithMessage updateProjectTypeDictionary(UpdateProjectTypeDictionaryDto updateProjectTypeDictionaryDto, TenantDto authData);
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
        public ResultWithMessage updateProjectTypeDictionary(UpdateProjectTypeDictionaryDto updateProjectTypeDictionaryDto, TenantDto authData)
        {
            ProjectType projectType = _db.ProjectTypes.Find(updateProjectTypeDictionaryDto.ProjectTypeId);

            if (projectType is null)
                return new ResultWithMessage(null, $"Invalid project type Id: {updateProjectTypeDictionaryDto.ProjectTypeId}");

            List<TypeDictionary> typeDictionariesToDelete = _db.TypeDictionaries.Where(e => e.ProjectTypeId == updateProjectTypeDictionaryDto.ProjectTypeId).ToList();

            if (typeDictionariesToDelete.Count == 0)
                return new ResultWithMessage(null, $"No dictionary found to delete with Id : {updateProjectTypeDictionaryDto.ProjectTypeId}");


            //Disable old dictionary
            foreach (TypeDictionary typeDictionary in typeDictionariesToDelete)
            {
                typeDictionary.IsDeleted = true;
                typeDictionary.LastUpdatedBy = authData.userName;
                typeDictionary.LastUpdatedDate = DateTime.Now;
            }
            _db.UpdateRange(typeDictionariesToDelete);

            //Add new dictionary
            List<TypeDictionary> typeDictionariesToCreate = [];
            foreach (DictionaryRange dictionaryRange in updateProjectTypeDictionaryDto.DictionaryRanges)
            {
                TypeDictionary typeDictionary = new()
                {
                    RangFrom = dictionaryRange.RangFrom,
                    RangTo = dictionaryRange.RangTo,
                    Value = dictionaryRange.Value,
                    IsDeleted = false,
                    CreatedBy = authData.userName,
                    AddedOn = DateTime.Now,
                    ProjectTypeId = updateProjectTypeDictionaryDto.ProjectTypeId
                };

                typeDictionariesToCreate.Add(typeDictionary);
            }

            _db.TypeDictionaries.AddRange(typeDictionariesToCreate);
            _db.SaveChanges();

            return new ResultWithMessage(null, string.Empty);
        }
    }
}