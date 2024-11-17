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
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public ActionResult<List<User>> GetUsers()
        {
            var users = Program.context.Users
                .Include(u => u.IdRoleNavigation).ToList();
            return Ok(users);
        }
    }
}
