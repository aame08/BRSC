using Client.DTOs;
using Client.For_Token;
using Client.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

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
    }
}
