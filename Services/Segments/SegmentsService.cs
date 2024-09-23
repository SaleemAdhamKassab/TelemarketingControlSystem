using TelemarketingControlSystem.Helper;
using TelemarketingControlSystem.Models.Data;
using static TelemarketingControlSystem.Services.Auth.AuthModels;
using TelemarketingControlSystem.Models;

namespace TelemarketingControlSystem.Services.Segments
{
	public interface ISegmentsService
	{
		ResultWithMessage getSegments();
		ResultWithMessage addSegment(SegmentDto segmentDto, TenantDto authData);
		ResultWithMessage projectSegments(int projectId);
	}
	public class SegmentsService(ApplicationDbContext db) : ISegmentsService
	{
		private readonly ApplicationDbContext _db = db;

		public ResultWithMessage getSegments()
		{
			var segments = _db.Segments
				.Where(e => !e.IsDeleted)
				.OrderBy(e => e.Name)
				.Select(e => e.Name)
				.ToList();
			return new ResultWithMessage(segments, string.Empty);
		}
		public ResultWithMessage addSegment(SegmentDto segmentDto, TenantDto authData)
		{
			Segment segment = new()
			{
				Name = segmentDto.Name.Trim(),
				IsDefault = false,
				CreatedBy = authData.userName,
				AddedOn = DateTime.Now,
				IsDeleted = false
			};

			_db.Segments.Add(segment);
			_db.SaveChanges();
			return new ResultWithMessage(null, string.Empty);
		}

		public ResultWithMessage projectSegments(int projectId)
		{
			Project project = _db.Projects.Find(projectId);
			if (project is null)
				return new ResultWithMessage(null, $"Invalid project Id: {projectId}");

			List<string> segments = _db.ProjectDetails.Where(e => e.ProjectId == projectId && !e.IsDeleted)
				.Select(e => e.SegmentName)
				.Distinct()
				.ToList();

			return new ResultWithMessage(segments, string.Empty);
		}
	}
}