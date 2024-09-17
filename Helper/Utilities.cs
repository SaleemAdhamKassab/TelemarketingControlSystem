namespace TelemarketingControlSystem.Helper
{
    public abstract class Utilities
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
    }
}
