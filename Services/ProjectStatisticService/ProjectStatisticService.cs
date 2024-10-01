using Microsoft.EntityFrameworkCore;
using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models;
using TelemarketingControlSystem.Models.Data;
using System.Linq.Expressions;
using static TelemarketingControlSystem.Services.ProjectStatisticService.ProjectStatisticsViewModels;

namespace TelemarketingControlSystem.Services.ProjectStatisticService
{
	public interface IProjectStatisticsService
	{
		ResultWithMessage generalReport(GeneralReportDto generalReportDto);
		ResultWithMessage hourlyTarget(HourlyTargetDto hourlyTargetDto);
	}
	public class ProjectStatisticService(ApplicationDbContext db) : IProjectStatisticsService
	{
		private readonly ApplicationDbContext _db = db;

		private IQueryable<ProjectDetail> getProjectDetailsData(GeneralReportDto generalReportDto)
		{
			if (generalReportDto == null || generalReportDto.ProjectId == 0)
				return Enumerable.Empty<ProjectDetail>().AsQueryable();

			IQueryable<ProjectDetail> query = _db.ProjectDetails
				.Where(e => e.ProjectId == generalReportDto.ProjectId &&
				e.LastUpdateDate.Value.Date >= Utilities.convertDateToArabStandardDate(generalReportDto.DateFrom).Date &&
				e.LastUpdateDate.Value.Date <= Utilities.convertDateToArabStandardDate(generalReportDto.DateTo).Date &&
				!e.IsDeleted);

			var predicates = new List<Expression<Func<ProjectDetail, bool>>>();

			if (generalReportDto.TelemarketerIds?.Count > 0)
				predicates.Add(e => generalReportDto.TelemarketerIds.Contains(e.EmployeeId));

			if (!string.IsNullOrEmpty(generalReportDto.LineType))
				predicates.Add(e => e.LineType.ToLower().Contains(generalReportDto.LineType.ToLower()));

			if (!string.IsNullOrEmpty(generalReportDto.CallStatus))
				predicates.Add(e => e.CallStatus.Name.ToLower().Contains(generalReportDto.CallStatus.ToLower()));

			if (!string.IsNullOrEmpty(generalReportDto.Generation))
				predicates.Add(e => e.Generation.ToLower().Contains(generalReportDto.Generation.ToLower()));

			if (!string.IsNullOrEmpty(generalReportDto.Region))
				predicates.Add(e => e.Region.ToLower().Contains(generalReportDto.Region.ToLower()));

			if (!string.IsNullOrEmpty(generalReportDto.City))
				predicates.Add(e => e.City.ToLower().Contains(generalReportDto.City.ToLower()));

			if (!string.IsNullOrEmpty(generalReportDto.Segment))
				predicates.Add(e => e.SegmentName.ToLower().Contains(generalReportDto.Segment.ToLower()));

			if (!string.IsNullOrEmpty(generalReportDto.SubSegment))
				predicates.Add(e => e.SubSegment.ToLower().Contains(generalReportDto.SubSegment.ToLower()));

			if (!string.IsNullOrEmpty(generalReportDto.Bundle))
				predicates.Add(e => e.Bundle.ToLower().Contains(generalReportDto.Bundle.ToLower()));

			if (!string.IsNullOrEmpty(generalReportDto.Contract))
				predicates.Add(e => e.Contract.ToLower().Contains(generalReportDto.Contract.ToLower()));

			foreach (var predicate in predicates)
				query = query.Where(predicate);

			return query;
		}
		private IQueryable<ProjectDetailCall> getProjectDetailCallsData(HourlyTargetDto hourlyTargetDto)
		{
			if (hourlyTargetDto == null || hourlyTargetDto.ProjectId == 0)
				return Enumerable.Empty<ProjectDetailCall>().AsQueryable();

			hourlyTargetDto.TargetDate = Utilities.convertDateToArabStandardDate(hourlyTargetDto.TargetDate);

			IQueryable<ProjectDetailCall> query = _db.ProjectDetailCalls
							   .Where(e => e.ProjectDetail.ProjectId == hourlyTargetDto.ProjectId &&
										   hourlyTargetDto.TelemarketerIds.Any(tId => e.ProjectDetail.EmployeeId == tId) &&
										   e.CallStartDate >= hourlyTargetDto.TargetDate &&
										   e.CallStartDate <= hourlyTargetDto.TargetDate.AddHours(1) &&
										   !e.ProjectDetail.IsDeleted);

			//exclude calls that (more than the minimum call duration + 2 minutes)
			double minDuration = query.Select(e => e.DurationInSeconds).Min();
			query = query.Where(e => e.DurationInSeconds <= minDuration + 120);

			return query;
		}

		public ResultWithMessage generalReport(GeneralReportDto generalReportDto)
		{
			IQueryable<ProjectDetail> query = getProjectDetailsData(generalReportDto);
			if (query.Count() == 0)
				return new ResultWithMessage(null, "No Data Found");

			//1) Header
			ProjectStatisticsViewModel result = new()
			{
				ProjectName = query.Select(e => e.Project.Name).First(),
				CreatedBy = Utilities.modifyUserName(Utilities.modifyUserName(query.Select(e => e.Project.CreatedBy).First())),
				AddedOn = query.Select(e => e.Project.AddedOn).First()
			};

			//2) GeneralDetails
			int closedGSMs = query.Where(e => e.CallStatus.IsClosed).Count();
			int nonClosed = query.Where(e => !e.CallStatus.IsClosed).Count();

			CardViewModel closedGSMsCard = new()
			{
				Category = "Closed GSMs",
				Count = closedGSMs,
				Total = closedGSMs + nonClosed
			};

			CardViewModel nonClosedGSMsCard = new()
			{
				Category = "Non-Closed GSMs",
				Count = nonClosed,
				Total = closedGSMs + nonClosed
			};

			int quota = query.Select(e => e.Project.Quota).First();
			CardViewModel quotaProgressCard = new()
			{
				Category = "Quota Progress",
				Count = closedGSMs,
				Total = quota
			};

			int totalTelemarketers = _db.Employees.Count(e => !e.IsDeleted);
			int projectTelemarketers = _db.ProjectDetails.Where(e => e.ProjectId == generalReportDto.ProjectId && !e.IsDeleted).Select(e => e.EmployeeId).Distinct().Count();
			CardViewModel telemarketersCard = new()
			{
				Category = "Telemarketers",
				Count = projectTelemarketers,
				Total = totalTelemarketers
			};
			result.GeneralDetails.Add(closedGSMsCard);
			result.GeneralDetails.Add(nonClosedGSMsCard);
			result.GeneralDetails.Add(telemarketersCard);
			result.GeneralDetails.Add(quotaProgressCard);

			//3) Statstic Report
			var statusData = query
				.GroupBy(g => g.CallStatus.Name)
				.Select(e => new StatusData
				{
					Status = e.Key,
					TelemarketerGSMs = e.GroupBy(en => en.Employee.UserName)
					.Select(t => new TelemarketerGSM
					{
						Telemarketer = Utilities.modifyUserName(t.Key),
						AssignedGSMs = t.Count()
					})
					.ToList()
				})
				.ToList();

			var reportFooter = query
				.GroupBy(g => g.Employee.UserName)
				.Select(e => new TelemarketerGSM
				{
					Telemarketer = Utilities.modifyUserName(e.Key),
					AssignedGSMs = e.Count()
				})
				.ToList();

			StatsticReport statsticReport = new()
			{
				Data = statusData,
				Footer = reportFooter
			};

			result.StatsticReport = statsticReport;

			//4) Closed Per Days -> Quota Progress

			List<ClosedPerDay> closedPerDays = query.Where(e => e.CallStatus.IsClosed).GroupBy(g => g.LastUpdateDate.Value.Date)
				.Select(e => new ClosedPerDay()
				{
					Date = e.Key.Date,
					Count = e.Count()
				}).ToList();

			result.ClosedPerDays = closedPerDays;

			return new ResultWithMessage(result, string.Empty);
		}
		public ResultWithMessage hourlyTarget(HourlyTargetDto hourlyTargetDto)
		{
			IQueryable<ProjectDetailCall> query = getProjectDetailCallsData(hourlyTargetDto);
			if (!query.Any())
				return new ResultWithMessage(null, "No Data Found");

			double totalMinutes = query.Select(e => e.DurationInSeconds).Sum() / 60.0;

			double closedCallsAvg = 0;
			var closedCallsAvgResult = query.Where(e => e.ProjectDetail.CallStatus.IsClosed).Select(e => e.DurationInSeconds / 60.0);
			if (closedCallsAvgResult.Any())
				closedCallsAvg = closedCallsAvgResult.Average();

			var hourlyStatusTargets = query
				.GroupBy(g => g.ProjectDetail.CallStatus.Name)
				.Select(e => new HourlyStatusTarget
				{
					Status = e.Key,
					TotalMinutes = e.Sum(x => x.DurationInSeconds) /60.0,
					HourPercentage = totalMinutes / e.Sum(x => x.DurationInSeconds) / 60.0,
					Rate = (totalMinutes / e.Sum(x => x.DurationInSeconds) / 60.0) * 60.0,
					Target = closedCallsAvg != 0 ? ((totalMinutes / e.Sum(x => x.DurationInSeconds) / 60.0) * 60.0) / closedCallsAvg : 0
				})
				.ToList();

			HourlyTargetViewModel result = new()
			{
				ClosedCallsAvg = closedCallsAvg,
				HourlyStatusTargets = hourlyStatusTargets
			};

			return new ResultWithMessage(result, string.Empty);
		}
	}
}