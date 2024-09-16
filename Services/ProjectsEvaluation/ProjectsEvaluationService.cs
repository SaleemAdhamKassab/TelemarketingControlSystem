using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
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
        private List<TypeDictionary> getTypeDictionariesRange(int projecttypeId, List<DictionaryRange> dictionaryRanges, string userName)
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

            //1) Validate range sequence
            if (!isValidRanges(updateProjectTypeDictionaryDto.DictionaryRanges))
                return new ResultWithMessage(null, "Invalid ranges");

            //2) Disable old dictionary
            disableOldDictionary(typeDictionariesToDelete, authData.userName);

            //3) Add new dictionary
            List<TypeDictionary> TypeDictionariesRange = getTypeDictionariesRange(updateProjectTypeDictionaryDto.ProjectTypeId, updateProjectTypeDictionaryDto.DictionaryRanges, authData.userName);
            _db.TypeDictionaries.AddRange(TypeDictionariesRange);

            _db.SaveChanges();

            return new ResultWithMessage(null, string.Empty);
        }
    }
}