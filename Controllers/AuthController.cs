using ApiBilling.Helpers;
using ApiBilling.Models;
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

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and Password are required." });

            string query = @"
                SELECT u.UserId, u.FullName, u.Email, r.RoleName
                FROM Users u
                INNER JOIN Roles r ON u.RoleId = r.RoleId
                WHERE u.Email=@Email AND u.PasswordHash=@Password AND u.IsActive=1";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", request.Email),
                new SqlParameter("@Password", request.Password) // Plain-text
            };

            var dt = _sqlHelper.ExecuteDataTable(query, parameters);
            if (dt.Rows.Count == 0)
                return Unauthorized(new { message = "Invalid Email or Password" });

            var row = dt.Rows[0];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", row["UserId"].ToString()!),
                    new Claim("Email", row["Email"].ToString()!),
                    new Claim("Role", row["RoleName"].ToString()!)
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

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                role = row["RoleName"],
                fullName = row["FullName"]
            });
        }
    }
}
