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
				e.LastUpdatedDate.Value.Date >= Utilities.convertDateToArabStandardDate(generalReportDto.DateFrom).Date &&
				e.LastUpdatedDate.Value.Date <= Utilities.convertDateToArabStandardDate(generalReportDto.DateTo).Date &&
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
			if (hourlyTargetDto is null || hourlyTargetDto.ProjectId == 0)
				return Enumerable.Empty<ProjectDetailCall>().AsQueryable();

			hourlyTargetDto.TargetDate = Utilities.convertDateToArabStandardDate(hourlyTargetDto.TargetDate);

			IQueryable<ProjectDetailCall> query = _db.ProjectDetailCalls
							   .Where(e => e.ProjectDetail.ProjectId == hourlyTargetDto.ProjectId &&
										   hourlyTargetDto.TelemarketerIds.Any(tId => e.ProjectDetail.EmployeeId == tId) &&
										   e.CallStartDate >= hourlyTargetDto.TargetDate &&
										   e.CallStartDate <= hourlyTargetDto.TargetDate.AddHours(1) &&
										   !e.ProjectDetail.IsDeleted);
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

			List<ClosedPerDay> closedPerDays = query.Where(e => e.CallStatus.IsClosed).GroupBy(g => g.LastUpdatedDate.Value.Date)
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
			var closedCallsAvgQuery = query
				.Where(e => e.ProjectDetail.CallStatus.IsClosed)
				.GroupBy(e => new
				{
					gsm = e.ProjectDetail.GSM,
					empName = e.ProjectDetail.Employee.Name,
					callStatus = e.ProjectDetail.CallStatus.Name
				})
				.Select(e => new
				{
					totalMin = e.Sum(x => x.DurationInSeconds) / 60.0
				});

			if (closedCallsAvgQuery.Any())
				closedCallsAvg = closedCallsAvgQuery.Average(e => e.totalMin);


			var hourlyStatusTargets = query
				.GroupBy(g => g.ProjectDetail.CallStatus.Name)
				.Select(e => new HourlyStatusTarget
				{
					Status = e.Key,
					TotalMinutes = e.Sum(x => x.DurationInSeconds) / 60.0,
					HourPercentage = (e.Sum(x => x.DurationInSeconds) / 60.0) / totalMinutes,
					Rate = ((e.Sum(x => x.DurationInSeconds) / 60.0) / totalMinutes) * 60.0,
					Target = closedCallsAvg != 0 ? (((e.Sum(x => x.DurationInSeconds) / 60.0) / totalMinutes) * 60.0) / closedCallsAvg : 0
				})
				.ToList();


			// get N/A value
			//1) Total working hours
			int employeesCount = query.Select(e => e.ProjectDetail.EmployeeId).Distinct().Count();
			double totalWorkingMinutes = employeesCount * 60.0;
			//2) Has status
			double sumHasStatusMinutes = query.Where(e => e.ProjectDetail.CallStatus.Name != null).Sum(e => e.DurationInSeconds) / 60.0;
			//3) N/A total
			double naTotal = totalWorkingMinutes - sumHasStatusMinutes;
			//4) assign to result
			HourlyStatusTarget naStatusTarget = new()
			{
				Status = "N/A",
				TotalMinutes = naTotal,
				HourPercentage = naTotal / totalMinutes,
				Rate = (naTotal / totalMinutes) * 60.0,
				Target = closedCallsAvg != 0 ? (naTotal / totalMinutes) * 60.0 / closedCallsAvg : 0
			};

			hourlyStatusTargets.Add(naStatusTarget);

			//return Result
			HourlyTargetViewModel result = new()
			{
				TotalMinutes = totalMinutes,
				ClosedCallsDurationAvg = closedCallsAvg,
				HourlyStatusTargets = hourlyStatusTargets
			};

			return new ResultWithMessage(result, string.Empty);
		}
	}
}