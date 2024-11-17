using API.DTOs;
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
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet("users")]
        public ActionResult<List<User>> GetUsers()
        {
            var users = Program.context.Users
                .Include(u => u.IdRoleNavigation).ToList();
            return Ok(users);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpGet("{id_user}")]
        public async Task<ActionResult<User>> GetUser(int id_user)
        {
            var user = await Program.context.Users
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.IdUser == id_user);
            if (user == null) { return NotFound(); }
            return Ok(user);
        }

        [Authorize(Roles = "User, Admin")]
        [HttpPut("{id_user}")]
        public ActionResult<User> UpdateUser(int id_user, [FromBody] UserDTO userDTO)
        {
            var user = Program.context.Users.FirstOrDefault(u => u.IdUser == id_user);
            if (user != null)
            {
                if (!string.IsNullOrWhiteSpace(userDTO.NameUser) || !string.IsNullOrWhiteSpace(userDTO.EmailUser))
                {
                    if (user.EmailUser != userDTO.EmailUser)
                    {
                        var isEmailUnique = Program.context.Users.Any(u => u.EmailUser == userDTO.EmailUser);
                        if (isEmailUnique) { return Conflict("Почта занята."); }
                    }
                    if (!string.IsNullOrWhiteSpace(userDTO.PasswordHash))
                    {
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.PasswordHash);
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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id_user}")]
        public ActionResult<User> DeleteUser(int id_user)
        {
            return Ok();
        }
    }
}
