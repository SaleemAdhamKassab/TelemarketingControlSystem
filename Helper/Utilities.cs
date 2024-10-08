using TelemarketingControlSystem.Services.ProjectEvaluationService;

namespace TelemarketingControlSystem.Helper
{
	public static class Utilities
    {
        public static string modifyUserName(string userName)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                userName = userName.Substring(userName.IndexOf('\\') + 1);
                return char.ToUpper(userName[0]) + userName.Substring(1).ToLower();
            }

            return string.Empty;
        }

        public static DateTime convertDateToArabStandardDate(DateTime dateTime)
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
			dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZone);

            return dateTime;
		}

		public static bool isValidDictionaryRanges(List<DictionaryRange> dictionaryRanges)
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
	}
}
