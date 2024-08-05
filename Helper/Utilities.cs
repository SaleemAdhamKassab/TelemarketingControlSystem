namespace TelemarketingControlSystem.Helper
{
    public abstract class Utilities
    {
        public static string CapitalizeFirstLetter(string str) => char.ToUpper(str[0]) + str.Substring(1).ToLower();

    }
}
