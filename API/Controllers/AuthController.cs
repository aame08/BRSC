using API.DTOs;
using API.JwtTokens;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(JwtProvider jwtProvider) : ControllerBase
    {
        private readonly JwtProvider _jwtProvider = jwtProvider;

        [HttpPost("register")]
        public ActionResult<User> Register([FromBody] UserDTO userDTO)
        {
            var existingUser = Program.context.Users.FirstOrDefault(u => u.EmailUser == userDTO.EmailUser);
            if (existingUser != null) { return Conflict("Данная почта уже зарегистрирована."); }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.PasswordHash);

            var newUser = new User
            {
                IdUser = Program.context.Users.Any() ? Program.context.Users.Max(u => u.IdUser) + 1 : 1,
                NameUser = userDTO.NameUser,
                EmailUser = userDTO.EmailUser,
                PasswordHash = passwordHash,
                IdRole = 3,
                IdRoleNavigation = Program.context.Roles.FirstOrDefault(r => r.IdRole == 3)
            };

            if (newUser.IdRoleNavigation == null) { return StatusCode(500, "Роль не найдена."); }

            var token = _jwtProvider.GenerateToken(newUser);

            Program.context.Users.Add(newUser);
            Program.context.SaveChanges();

            return StatusCode(201, new {User = newUser, Token = token});
        }
    }
}
