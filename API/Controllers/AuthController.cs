using API.DTOs;
using API.JwtTokens;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(JwtProvider jwtProvider, BrscContext context) : ControllerBase
    {
        private readonly JwtProvider _jwtProvider = jwtProvider;
        private readonly BrscContext _context = context;

        /// <summary>
        /// Регистрация нового пользователя.
        /// </summary>
        /// <remarks>
        /// Этот метод создает нового пользователя. Для создания пользователя необходимо передать все необходимые данные.
        /// В случае успешной регистрации возвращается объект пользователя и JWT токен доступа.
        /// Пример запроса:
        /// 
        ///     POST
        ///     {
        ///         "idUser": 1,
        ///         "nameUser": "amee",
        ///         "emailUser": "ame@gmail.com",
        ///         "passwordHash": "WERTydfno389",
        ///         "idRole": 3
        ///     } 
        /// </remarks>
        /// <param name="userDTO">Модель данных для регистрации.</param>
        /// <response code="201">Пользователь успешно зарегистрирован.</response>
        /// <response code="409">Если почта уже используется.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost("register")]
        public ActionResult<User> Register([FromBody] UserDTO userDTO)
        {
            try
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.EmailUser == userDTO.EmailUser);
                if (existingUser != null) { return Conflict("Данная почта уже зарегистрирована."); }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.OldPassword);

                var newUser = new User
                {
                    IdUser = _context.Users.Any() ? _context.Users.Max(u => u.IdUser) + 1 : 1,
                    NameUser = userDTO.NameUser,
                    EmailUser = userDTO.EmailUser,
                    PasswordHash = passwordHash,
                    IdRole = 3,
                    IdRoleNavigation = _context.Roles.FirstOrDefault(r => r.IdRole == 3)
                };

                if (newUser.IdRoleNavigation == null) { return StatusCode(500, "Роль не найдена."); }

                var token = _jwtProvider.GenerateToken(newUser);

                _context.Users.Add(newUser);
                _context.SaveChanges();

                return StatusCode(201, new { User = newUser, AccessToken = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренная ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Логин пользователя с использованием электронной почты и пароля.
        /// </summary>
        /// <param name="email">Электронная почта пользователя.</param>
        /// <param name="password">Пароль пользователя.</param>
        /// <remarks>
        /// Этот метод выполняет вход пользователя в систему и возвращает JWT токен, если данные корректны.
        /// </remarks>
        /// <response code="200">Возвращает токен доступа и refresh токен.</response>
        /// <response code="401">Если пользователь не найден или пароль неверный.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost("login")]
        public ActionResult Login(string email, string password)
        {
            try
            {
                var user = _context.Users
                    .Include(u => u.IdRoleNavigation)
                    .FirstOrDefault(u => u.EmailUser == email);

                if (user == null) { return Unauthorized("Пользователь не найден."); }
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) { return Unauthorized("Неверный пароль."); }

                var token = _jwtProvider.GenerateToken(user);
                var refreshToken = _jwtProvider.GenerateRefreshToken(user);

                return Ok(new { User = user, AccessToken = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренная ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Получение ID пользователя по электронной почте.
        /// </summary>
        /// <remarks>
        /// Этот метод ищет пользователя по электронной почте и возвращает его ID.
        /// Если пользователя с такой почтой нет, возвращает 0.
        /// </remarks>
        /// <param name="email">Электронная почта пользователя, для которого нужно получить ID.</param>
        /// <response code="200">Возвращает ID пользователя, если он найден.</response>
        /// <response code="404">Если пользователь с указанной почтой не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [Authorize]
        [HttpGet("{email}")]
        public ActionResult<int> GetIdUserByEmail(string email)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.EmailUser == email);
                if (user == null) return NotFound("Пользователь не найден.");
                return Ok(user.IdUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренная ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверка валидности JWT токена.
        /// </summary>
        /// <remarks>
        /// Этот метод проверяет, действителен ли текущий токен, и возвращает статус о его валидности.
        /// </remarks>
        /// <response code="200">Токен действителен.</response>
        /// <response code="401">Токен недействителен или не был передан.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [Authorize]
        [HttpGet("verify-token")]
        public ActionResult VerifyToken()
        {
            return Ok("Токен валидный.");
        }

        /// <summary>
        /// Обновление токенов (генерация нового Access Token и Refresh Token).
        /// </summary>
        /// <remarks>
        /// Этот метод позволяет обновить JWT токены с использованием refresh token.
        /// </remarks>
        /// <param name="token">Токен, который необходимо обновить (Refresh Token).</param>
        /// <response code="200">Возвращает новый Access Token и Refresh Token.</response>
        /// <response code="401">Если токен недействителен или не был передан.</response>
        /// <response code="404">Если пользователь, связанный с токеном, не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost("refresh-token")]
        public ActionResult RefreshToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userIDClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "id_user");
            if (userIDClaim == null) { return Unauthorized("Неверный токен."); }

            var userID = int.Parse(userIDClaim.Value);
            var user = _context.Users
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefault(u => u.IdUser == userID);
            if (user == null) { return Unauthorized("Пользователь не найден."); }

            var newAccessToken = _jwtProvider.GenerateToken(user);
            var newRefreshToken = _jwtProvider.GenerateRefreshToken(user);
            return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
        }
    }
}
