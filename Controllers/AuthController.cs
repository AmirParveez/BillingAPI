using ApiBilling.Helpers;
using ApiBilling.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Data.SqlClient;

namespace ApiBilling.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SqlHelper _sqlHelper;
        private readonly IConfiguration _configuration;

        public AuthController(SqlHelper sqlHelper, IConfiguration configuration)
        {
            _sqlHelper = sqlHelper;
            _configuration = configuration;
        }

        // ===================== LOGIN (POST) =====================
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Email and Password are required.");

            string email = model.Email.Trim();
            string password = model.Password.Trim();

            string hashedPassword = PasswordHelper.HashPassword(password);

            string query = @"
                SELECT u.UserId, u.FullName, u.Email, r.RoleName
                FROM Users u
                INNER JOIN Roles r ON u.RoleId = r.RoleId
                WHERE u.Email=@Email AND u.PasswordHash=@Password AND u.IsActive=1";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email),
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

        // ===================== AUTH TEST =====================
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                userId = User.FindFirst("UserId")?.Value,
                email = User.FindFirst("Email")?.Value,
                role = User.FindFirst("Role")?.Value
            });
        }

        // ===================== JWT =====================
        private string GenerateJwtToken(string userId, string email, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", userId),
                    new Claim("Email", email),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
