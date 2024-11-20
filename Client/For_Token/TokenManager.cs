using System.IdentityModel.Tokens.Jwt;
using System.IO;

namespace Client.For_Token
{
    public class TokenManager
    {
        public static void SaveToken(int userID, string token)
        {
            string protectedToken = TokenStorage.Protect(token);

            string fileName = $"token_{userID}.dat";
            File.WriteAllText(fileName, protectedToken);
        }
        public static string? GetToken(int userID)
        {
            string fileName = $"token_{userID}.dat";

            if (!File.Exists(fileName)) { return null; }

            string protectedToken = File.ReadAllText(fileName);
            return TokenStorage.Unprotect(protectedToken);
        }
        public static void DeleteToken(int userID)
        {
            string fileName = $"token_{userID}.dat";

            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        public static string GetRoleByToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");

            if (roleClaim != null) { return roleClaim.Value; }
            return "";
        }
        public static int GetIdUserByToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var idUserClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "id_user");

            if (idUserClaim != null && int.TryParse(idUserClaim.Value, out int id_user)) { return id_user; }
            return 0;
        }

        public static void SaveRefreshToken(int userID, string refreshToken)
        {
            string protectedToken = TokenStorage.Protect(refreshToken);
            string fileName = $"refresh_{userID}.dat";
            File.WriteAllText(fileName, protectedToken);
        }
        public static string? GetRefreshToken(int userID)
        {
            string fileName = $"refresh_{userID}.dat";
            if (!File.Exists(fileName)) { return null; }

            string protectedToken = File.ReadAllText(fileName);
            return TokenStorage.Unprotect(protectedToken);
        }
        public static void DeleteRefreshToken(int userID)
        {
            string fileName = $"refresh_{userID}.dat";

            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }
}
