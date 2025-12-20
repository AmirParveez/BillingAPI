using ApiBilling.Helpers;
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

        // GET api/auth/login?email=xxx&password=yyy
        [HttpGet("login")]
        public IActionResult Login([FromQuery] string email, [FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return BadRequest("Email and Password are required.");

            email = email.Trim();
            password = password.Trim();

            string hashedPassword = PasswordHelper.HashPassword(password);

            // 🔹 DEBUG: log hashed password to console
            Console.WriteLine($"Email: {email}, Hashed Password: {hashedPassword}");

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
                    new Claim("Role", role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}