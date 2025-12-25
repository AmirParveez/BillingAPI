using Microsoft.AspNetCore.Mvc;
using api.BLL;

namespace api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserLoginBLL _userBLL;

        public AuthController(UserLoginBLL userBLL)
        {
            _userBLL = userBLL;
        }

        // GET api/auth/login?username=amir&password=123456
        [HttpGet("login")]
        public IActionResult Login([FromQuery] string username, [FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return BadRequest(new { message = "Username and password required" });

            var user = _userBLL.GetUser(username, password);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(new
            {
                status = "success",
                userId = user.UserId,
                username = user.Username,
                currentSession = user.Current_Session
            });
        }
    }
}
