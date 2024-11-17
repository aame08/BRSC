using Client.DTOs;
using Client.For_Token;
using Client.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows;

namespace Client.Data
{
    public class RegisterResponse
    {
        public User? User { get; set; }
        public string? Token { get; set; }
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
                    var response = JsonConvert.DeserializeObject<RegisterResponse>(await result.Content.ReadAsStringAsync());
                    string token = response.Token;
                    int id_user = response.User.IdUser;

                    TokenManager.SaveToken(id_user, token);
                    return token;
                }
                else
                {
                    var errorContent = await result.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error: {errorContent}");
                }
                return null;
            }
        }
        //public static async Task<bool> LoginWithToken(string email)
        //{
        //    var id_user = await GetUserIdByEmail(email);
        //    if (id_user == 0) { return false; }

        //    var token = TokenManager.GetToken(id_user);
        //    if (token == null) { return false; }

        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        //        var result = await client.GetAsync("https://localhost:7195/api/Auth/verify-token");
        //        if (result.StatusCode == System.Net.HttpStatusCode.OK) { return true; }
        //        else
        //        {
        //            TokenManager.DeleteToken(id_user);
        //            return false;
        //        }
        //    }
        //}
        public static async Task<int> GetUserIdByEmail(string email)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://localhost:7195/api/Auth/{email}");
                if (response.StatusCode != System.Net.HttpStatusCode.OK) { return 0; }

                var content = await response.Content.ReadAsStringAsync();
                var id_user = JsonConvert.DeserializeObject<int>(content);
                return id_user;
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

        public static async Task<List<User>> GetUsers(string token)
        {
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
    }
}
