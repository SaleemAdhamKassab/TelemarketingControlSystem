using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models;
using TelemarketingControlSystem.Models.Data;
using static TelemarketingControlSystem.Services.Auth.AuthModels;
using static TelemarketingControlSystem.Services.ProjectStatistics.ProjectStatisticsViewModels;

namespace TelemarketingControlSystem.Services.ProjectStatistics
{
    public interface IProjectStatisticsService
    {
        ResultWithMessage getProjectStatistics(int projectId, DateTime dateFrom, DateTime dateTo, TenantDto authData);
        ResultWithMessage hourlyTelemarketerTarget(HourlyTargetDto hourlyTargetDto);
    }
    public class ProjectStatisticsService(ApplicationDbContext db) : IProjectStatisticsService
    {
        private readonly ApplicationDbContext _db = db;

        public ResultWithMessage getProjectStatistics(int projectId, DateTime dateFrom, DateTime dateTo, TenantDto authData)
        {
            dateTo = dateTo.AddSeconds(86399);

            var projectDetails = _db.ProjectDetails
            .Where(e => e.ProjectId == projectId && e.LastUpdateDate >= dateFrom && e.LastUpdateDate <= dateTo && !e.IsDeleted)
            .Include(e => e.Project)
            .Include(e => e.CallStatus)
            .Include(e => e.Employee).ToList();

            if (projectDetails.Count == 0)
                return new ResultWithMessage(null, "No Data Found");

            var mainObj = projectDetails.GroupBy(g => new
            {
                projectName = g.Project.Name,
                addedOn = g.Project.AddedOn,
                createdBy = Utilities.CapitalizeFirstLetter(g.Project.CreatedBy.Substring(g.Project.CreatedBy.IndexOf("\\") + 1)),
                telemarketer = Utilities.CapitalizeFirstLetter(g.Employee.UserName.Substring(g.Employee.UserName.IndexOf("\\") + 1)),
                callStatus = g.CallStatus.Name,
                quota = g.Project.Quota
            })
            .Select(e => new
            {
                ProjectName = e.Key.projectName,
                AddedOn = e.Key.addedOn,
                CreatedBy = e.Key.createdBy,
                Telemarketer = e.Key.telemarketer,
                CallStatus = e.Key.callStatus,
                Quota = e.Key.quota,
                AssignedGsms = e.Count()
            }).ToList();


            //////1) GeneralDetails
            int totalGSMs = mainObj.Select(e => e.AssignedGsms).Sum();
            int closedGSMs = mainObj.Where(e => e.CallStatus == "Completed").Select(e => e.AssignedGsms).Sum();
            int telemarketerCount = mainObj.Select(e => e.Telemarketer).Distinct().Count();

            //////2) GSMSCountPerCallStatus
            List<CardViewModel> gsmsCountPerCallStatus = mainObj
                .GroupBy(g => g.CallStatus)
                .Select(e => new CardViewModel
                {
                    Category = e.Key,
                    Count = e.Sum(x => x.AssignedGsms),
                    Total = totalGSMs
                }).ToList();

            /////3) Telemarketer productivity
            List<TelemarketerProductivityCardViewModel> telemarketerProductivities = mainObj.GroupBy(g => g.Telemarketer)
                .Select(e => new TelemarketerProductivityCardViewModel()
                {
                    Telemarketer = e.Key,
                    AssignedGSMs = e.Sum(e => e.AssignedGsms),
                    Closed = e.Count(e => e.CallStatus == "Completed"),
                    Completed = e.Count(e => e.CallStatus == "Completed"),

                    CompletedRate = e.Count(e => e.CallStatus == "Completed") / Convert.ToDouble(e.Sum(e => e.AssignedGsms)),
                    ClosedRate = e.Count(e => e.CallStatus == "Completed") / Convert.ToDouble(e.Sum(e => e.AssignedGsms))
                }).ToList();


            ///////4) Quota Progress -> closed by date
            List<CompletedQuotaPerDay> quotaProgress = _db.ProjectDetails.
                Where(e => e.ProjectId == projectId && e.CallStatusId == 3 && !e.IsDeleted && e.LastUpdateDate != null && e.LastUpdateDate.Value.Date >= dateFrom.Date && e.LastUpdateDate.Value.Date <= dateTo.Date)
                .GroupBy(g => g.LastUpdateDate.Value.Date)
                .Select(e => new CompletedQuotaPerDay()
                {
                    Date = e.Key.Date,
                    Count = e.Count()
                }).ToList();

            ///////5) Result
            int totalTelemarketer = _db.Employees.Where(e => e.IsActive).Count();
            ProjectStatisticsViewModel result = new()
            {
                ProjectName = mainObj[0].ProjectName,
                AddedOn = mainObj[0].AddedOn,
                CreatedBy = Utilities.CapitalizeFirstLetter(mainObj[0].CreatedBy),
                GeneralDetails = [
                    new CardViewModel {Category = "Completed",Count=closedGSMs,Total =totalGSMs},
                    new CardViewModel {Category = "Non-Completed",Count=totalGSMs -  closedGSMs,Total =totalGSMs},
                    new CardViewModel {Category = "Quota Progress",Count=closedGSMs,Total =mainObj[0].Quota},
                    new CardViewModel {Category = "Telemarketer",Count=telemarketerCount,Total=totalTelemarketer}
                ],
                CallStatuses = gsmsCountPerCallStatus,
                TelemarketerProductivities = telemarketerProductivities,
                CompletedQuotaPerDays = quotaProgress
            };

            return new ResultWithMessage(result, string.Empty);
        }
        public ResultWithMessage hourlyTelemarketerTarget(HourlyTargetDto hourlyTargetDto)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            hourlyTargetDto.TargetDate = TimeZoneInfo.ConvertTimeFromUtc(hourlyTargetDto.TargetDate, timeZone);

            var pdc = _db.ProjectDetailCalls
                                          .Where(e => e.ProjectDetail.ProjectId == hourlyTargetDto.ProjectId
                                          && hourlyTargetDto.TelemarketerIds.Any(tId => e.ProjectDetail.EmployeeId == tId)
                                          && e.CallStartDate >= hourlyTargetDto.TargetDate
                                          && e.CallStartDate <= hourlyTargetDto.TargetDate.AddHours(1)
                                          && !e.ProjectDetail.IsDeleted)
                                          .Include(e => e.ProjectDetail.CallStatus)
                                          .ToList();

            if (pdc.Count == 0)
                return new ResultWithMessage(null, "No data found");

            var minDuration = pdc.Select(e => e.DurationInSeconds).Min();

            var pdcDurationRang = pdc.Where(e => e.DurationInSeconds >= minDuration && e.DurationInSeconds <= minDuration + 120).ToList();
            if (pdcDurationRang.Count == 0)
                return new ResultWithMessage(null, "No Date Found");

            var telemarketerCalls = pdcDurationRang.Select(e => new
            {
                callStatus = e.ProjectDetail.CallStatus.Name,
                totalMinutes = e.DurationInSeconds / 60.0
            }).ToList();



            var callStatusMinutes = telemarketerCalls
                .GroupBy(g => g.callStatus)
                .Select(e => new
                {
                    callStatus = e.Key,
                    totalMinutes = e.Sum(x => x.totalMinutes),
                    avergeMinutes = e.Average(x => x.totalMinutes)
                }).ToList();

            var callStatusesTotalMinutes = callStatusMinutes.Sum(g => g.totalMinutes);
            double averageCompletedCalls = 0;
            var averageCompletedCallsResult = callStatusMinutes.Where(e => e.callStatus.ToLower() == "completed").Select(e => e.avergeMinutes).FirstOrDefault();
            if (averageCompletedCallsResult != null)
                averageCompletedCalls = averageCompletedCallsResult;

            HourlyTelemarketerTargetViewModel result = new()
            {
                AverageCompletedCalls = averageCompletedCalls,
                Data = callStatusMinutes.Select(e => new HourlyTelemarketerTargetCallStatusViewModel
                {
                    Status = e.callStatus,
                    TotalMinutes = e.totalMinutes,
                    HourPercentage = e.totalMinutes / callStatusesTotalMinutes,
                    Rate = (e.totalMinutes / callStatusesTotalMinutes) * 60.0,
                    Target = averageCompletedCalls != 0 ? ((e.totalMinutes / callStatusesTotalMinutes) * 60.0) / averageCompletedCalls : 0
                }).ToList()
            };

            return new ResultWithMessage(result, string.Empty);
        }
    }
}