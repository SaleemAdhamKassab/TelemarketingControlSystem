using System.Security.Cryptography;
using System.Text;

namespace TelemarketingControlSystem.Helper
{
    public static class ComputeSHA256
    {
       public static string ComputeSHA256Func(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
