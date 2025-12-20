using ApiBilling.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;

namespace ApiBilling.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SqlHelper _sqlHelper;
        private readonly string _jwtKey = "YourSuperSecretKey123!"; // Keep same as appsettings.json

        public AuthController(SqlHelper sqlHelper) => _sqlHelper = sqlHelper;

        // POST api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Email and Password are required.");

            string hashedPassword = HashPassword(request.Password);

            string query = @"SELECT u.UserId, u.FullName, u.Email, r.RoleName
                             FROM Users u
                             INNER JOIN Roles r ON u.RoleId = r.RoleId
                             WHERE u.Email=@Email AND u.PasswordHash=@Password AND u.IsActive=1";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", request.Email),
                new SqlParameter("@Password", hashedPassword)
            };

            var dt = _sqlHelper.ExecuteDataTable(query, parameters);

            if (dt.Rows.Count == 0)
                return Unauthorized(new { message = "Invalid Email or Password" });

            var row = dt.Rows[0];

            string token = GenerateJwtToken(
                row["UserId"].ToString()!,
                row["Email"].ToString()!,
                row["RoleName"].ToString()!
            );

            return Ok(new
            {
                token,
                role = row["RoleName"],
                fullName = row["FullName"]
            });
        }

        private string GenerateJwtToken(string userId, string email, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", userId),
                    new Claim("Email", email),
                    new Claim("Role", role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
