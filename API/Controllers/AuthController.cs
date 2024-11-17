using API.DTOs;
using API.JwtTokens;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                IdRole = userDTO.IdRole,
                IdRoleNavigation = Program.context.Roles.FirstOrDefault(r => r.IdRole == userDTO.IdRole)
            };

            if (newUser.IdRoleNavigation == null) { return StatusCode(500, "Роль не найдена."); }

            var token = _jwtProvider.GenerateToken(newUser);

            Program.context.Users.Add(newUser);
            Program.context.SaveChanges();

            return StatusCode(201, new { User = newUser, Token = token });
        }

        [HttpPost("login")]
        public ActionResult Login(string email, string password)
        {
            var user = Program.context.Users
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefault(u => u.EmailUser == email);

            if (user == null) { return Unauthorized("Пользователь не найден."); }
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) { return Unauthorized("Неверный пароль."); }

            var token = _jwtProvider.GenerateToken(user);

            return Ok(new { User = user, Token = token });
        }

        [HttpGet("{email}")]
        public ActionResult<int> GetIdUserByEmail(string email)
        {
            var user = Program.context.Users.FirstOrDefault(u => u.EmailUser == email);
            return user != null ? user.IdUser : 0;
        }
        [Authorize]
        [HttpGet("verify-token")]
        public ActionResult VerifyToken()
        {
            return Ok("Токен валидный.");
        }
    }
}
