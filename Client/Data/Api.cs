using Client.DTOs;
using Client.For_Token;
using Client.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Client.Data
{
    public class RegisterResponse
    {
        public User? User { get; set; }
        public string? Token { get; set; }
    }
    public class LoginResponse
    {
        public User User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class Api
    {
        public static async Task<(User?, string? token)> Register(UserDTO userDTO)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(userDTO);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://localhost:7195/api/Auth/register", content);
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    var resultContent = await response.Content.ReadAsStringAsync();
                    var resultObject = JsonConvert.DeserializeObject<RegisterResponse>(resultContent);

                    if (resultObject != null)
                    {
                        if (resultObject.Token != null) { TokenManager.SaveToken(resultObject.User.IdUser, resultObject.Token); }
                        return (resultObject.User, resultObject.Token);
                    }
                }

                return (null, null);
            }
        }
        public static async Task<string?> Login(string email, string password)
        {
            using (var client = new HttpClient())
            {
                var url = $"https://localhost:7195/api/Auth/login?email={email}&password={password}";

                var result = await client.PostAsync(url, null);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = JsonConvert.DeserializeObject<LoginResponse>(await result.Content.ReadAsStringAsync());
                    string accessToken = response.AccessToken;
                    int userID = response.User.IdUser;

                    TokenManager.SaveToken(userID, accessToken);
                    TokenManager.SaveRefreshToken(userID, response.RefreshToken);
                    return accessToken;
                }
                return null;
            }
        }
        public static async Task<int> GetUserIdByEmail(string email)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://localhost:7195/api/Auth/{email}");
                if (response.StatusCode != System.Net.HttpStatusCode.OK) { return 0; }

                var content = await response.Content.ReadAsStringAsync();
                var userID = JsonConvert.DeserializeObject<int>(content);
                return userID;
            }
        }
        public static async Task<bool> VerifyToken(string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var result = await client.GetAsync("https://localhost:7195/api/Auth/verify-token");
                return result.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        public static async Task<List<User>> GetUsers(int userID, string token)
        {
            userID = TokenManager.GetIdUserByToken(token);
            if (userID == null) { return []; }
            token = await TokenValid(userID, token);
            if (token == null) { return []; }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var result = await client.GetAsync("https://localhost:7195/api/Users/users");

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<User>>(response);
                }
                return [];
            }
        }

        public static async Task<User> GetUser(int userID, string token)
        {
            token = await TokenValid(userID, token);
            if (token == null) { return null; }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var url = $"https://localhost:7195/api/Users/{userID}";

                var response = await client.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<User>(json);
                    return user;
                }
                return null;
            }
        }
        public static async Task<bool> UpdateUser(int adminID, int userID, UserDTO user, string token)
        {
            if (adminID != 0)
            {
                token = await TokenValid(adminID, token);
                if (token == null) { return false; }
            }
            token = await TokenValid(userID, token);
            if (token == null) { return false; }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await client.PutAsync($"https://localhost:7195/api/Users/{userID}", content);
                if (result.StatusCode == System.Net.HttpStatusCode.OK) { return true; }
                return false;
            }
        }
        public static async Task<bool> DeleteUser(int adminID, int userID, string token)
        {
            if (adminID != 0)
            {
                token = await TokenValid(adminID, token);
                if (token == null) { return false; }
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var result = await client.DeleteAsync($"https://localhost:7195/api/Users/{userID}");
                if (result.StatusCode == System.Net.HttpStatusCode.NoContent) { return true; }
                return false;
            }
        }

        public static async Task<List<Role>> GetRoles(int userID, string token)
        {
            userID = TokenManager.GetIdUserByToken(token);
            if (userID == null) { return []; }
            token = await TokenValid(userID, token);
            if (token == null) { return []; }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var result = await client.GetAsync("https://localhost:7195/api/Users/roles");
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Role>>(response);
                }
                return [];
            }
        }

        public static async Task<string?> RefreshToken(int userID)
        {
            string? refreshToken = TokenManager.GetRefreshToken(userID);
            if (refreshToken == null) { return null; }

            using (var client = new HttpClient())
            {
                var url = $"https://localhost:7195/api/Auth/refresh-token?token={refreshToken}";
                var response = await client.PostAsync(url, null);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<RefreshTokenResponse>(await response.Content.ReadAsStringAsync());
                    if (result != null)
                    {
                        TokenManager.SaveToken(userID, result.AccessToken);
                        TokenManager.SaveRefreshToken(userID, result.RefreshToken);

                        return result.AccessToken;
                    }
                }
                return null;
            }
        }
        public static async Task<string?> TokenValid(int userID, string token)
        {
            if (await VerifyToken(token)) { return token; }
            return await RefreshToken(userID);
        }
    }
}
