using System.Security.Cryptography;
using System.Text;

namespace Client.For_Token
{
    // хуйня для защиты токенов
    public class TokenStorage
    {
        public static string Protect(string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] protectedData = ProtectedData.Protect(dataBytes, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedData);
        }
        public static string Unprotect(string protectedData)
        {
            byte[] protectedDataBytes = Convert.FromBase64String(protectedData);
            byte[] dataBytes = ProtectedData.Unprotect(protectedDataBytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(dataBytes);
        }
    }
}
