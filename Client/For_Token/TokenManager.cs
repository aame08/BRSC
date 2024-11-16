using System.IO;

namespace Client.For_Token
{
    // хуета для сохранения токенов
    public class TokenManager
    {
        public static void SaveToken(int id_user, string token)
        {
            string protectedToken = TokenStorage.Protect(token);

            string fileName = $"token_{id_user}.dat";
            File.WriteAllText(fileName, protectedToken);
        }
        public static string? GetToken(int id_user)
        {
            string fileName = $"token_{id_user}.dat";

            if (!File.Exists(fileName)) { return null; }

            string protectedToken = File.ReadAllText(fileName);
            return TokenStorage.Unprotect(protectedToken);
        }
        public static void DeleteToken(int id_user)
        {
            string fileName = $"token_{id_user}.dat";

            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }
}
