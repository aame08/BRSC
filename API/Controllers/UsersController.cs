﻿using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Получает список всех пользователей.
        /// </summary>
        /// <remarks>
        /// Этот метод возвращает список пользователей с их ролями.
        /// Только пользователи с ролью "Admin" или "Manager" могут вызвать этот метод.
        /// </remarks>
        /// <response code="200">Возвращает список пользователей.</response>
        /// <response code="401">Если пользователь не авторизован.</response>
        /// <response code="403">Если пользователь не имеет подходящей роли.</response>
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet("users")]
        public ActionResult<List<User>> GetUsers()
        {
            var users = Program.context.Users
                .Include(u => u.IdRoleNavigation)
                .OrderBy(u => u.IdUser).ToList();
            return Ok(users);
        }

        /// <summary>
        /// Получает информацию о конкретном пользователе.
        /// </summary>
        /// <param name="userID">ID пользователя, информацию о котором нужно получить.</param>
        /// <remarks>
        /// Этот метод позволяет получить информацию о пользователе, если он существует.
        /// Для использования этого метода, пользователь должен иметь роль "Admin" или "User".
        /// </remarks>
        /// <response code="200">Возвращает данные пользователя.</response>
        /// <response code="401">Если пользователь не авторизован.</response>
        /// <response code="403">Если пользователь не имеет подходящей роли.</response>
        /// <response code="404">Если пользователь с данным ID не найден.</response>
        [Authorize(Roles = "User, Admin")]
        [HttpGet("{userID}")]
        public async Task<ActionResult<User>> GetUser(int userID)
        {
            var user = await Program.context.Users
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.IdUser == userID);
            if (user == null) { return NotFound(); }
            return Ok(user);
        }

        /// <summary>
        /// Обновляет информацию о пользователе.
        /// </summary>
        /// <param name="userID">ID пользователя, информацию которого нужно обновить.</param>
        /// <param name="userDTO">Обновляемые данные пользователя.</param>
        /// <remarks>
        /// Этот метод позволяет обновить информацию о пользователе.
        /// Только пользователи с ролью "Admin" или сам пользователь могут изменять свои данные.
        /// Для пользователей с ролью "Admin" доступны изменения роли другого пользователя.
        /// </remarks>
        /// <response code="200">Если пользователь был успешно обновлён.</response>
        /// <response code="401">Если пользователь не авторизован.</response>
        /// <response code="403">Если пользователь не имеет подходящей роли.</response>
        /// <response code="404">Если пользователь с данным ID не найден.</response>
        /// <response code="409">Если почта пользователя уже занята или данные не заполнены.</response>
        [Authorize(Roles = "User, Admin")]
        [HttpPut("{userID}")]
        public ActionResult<User> UpdateUser(int userID, [FromBody] UserDTO userDTO)
        {
            var user = Program.context.Users.FirstOrDefault(u => u.IdUser == userID);
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(userDTO.NameUser) || !string.IsNullOrWhiteSpace(userDTO.EmailUser))
                {
                    if (user.EmailUser != userDTO.EmailUser)
                    {
                        var isEmailUnique = Program.context.Users.Any(u => u.EmailUser == userDTO.EmailUser);
                        if (isEmailUnique) { return Conflict("Почта занята."); }
                    }
                    if (!string.IsNullOrWhiteSpace(userDTO.NewPassword))
                    {
                        if (!BCrypt.Net.BCrypt.Verify(userDTO.OldPassword, user.PasswordHash)) { return Conflict("Пароль неверный."); }
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.NewPassword);
                    }
                    user.NameUser = userDTO.NameUser;
                    if (User.IsInRole("Admin")) { user.IdRole = userDTO.IdRole; }

                    Program.context.SaveChanges();
                    return Ok(user);
                }
                else { return Conflict("Данные не заполнены."); }
            }
            else { return NotFound("Пользователь не найден."); }
        }

        /// <summary>
        /// Удаляет пользователя.
        /// </summary>
        /// <param name="userID">ID пользователя, которого нужно удалить.</param>
        /// <remarks>
        /// Этот метод позволяет удалить пользователя. Только пользователи с ролью "Admin" могут выполнить это действие.
        /// </remarks>
        /// <response code="204">Если пользователь был успешно удалён.</response>
        /// <response code="401">Если пользователь не авторизован.</response>
        /// <response code="403">Если пользователь не имеет подходящей роли.</response>
        /// <response code="404">Если пользователь с данным ID не найден.</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{userID}")]
        public ActionResult<User> DeleteUser(int userID)
        {
            var currentUserIdClaim = User.FindFirst("id_user");
            if (!int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                return Unauthorized("Некорректный идентификатор текущего пользователя.");
            }
            if (currentUserId == userID)
            {
                return Conflict("Администратор не может удалить самого себя.");
            }

            var user = Program.context.Users.FirstOrDefault(u => u.IdUser == userID);
            if (user != null)
            {
                Program.context.Users.Remove(user);
                Program.context.SaveChanges();
                return NoContent();
            }
            else { return NotFound(); }
        }

        /// <summary>
        /// Получает список всех ролей.
        /// </summary>
        /// <remarks>
        /// Этот метод позволяет получить список всех ролей, доступных в системе.
        /// Только пользователи с ролью "Admin" могут вызвать этот метод.
        /// </remarks>
        /// <response code="200">Возвращает список ролей.</response>
        /// <response code="401">Если пользователь не авторизован.</response>
        /// <response code="403">Если пользователь не имеет подходящей роли.</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("roles")]
        public ActionResult<List<Role>> GetRoles()
        {
            var roles = Program.context.Roles.ToList();
            return Ok(roles);
        }
    }
}
