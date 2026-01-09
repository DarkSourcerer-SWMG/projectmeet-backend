using Microsoft.AspNetCore.Mvc;
using ProjectMeet.Data;
using ProjectMeet.Models;
using ProjectMeet.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace ProjectMeet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private static string HashPassword(string password)
{
    using var sha = SHA256.Create();
    return Convert.ToBase64String(
        sha.ComputeHash(Encoding.UTF8.GetBytes(password))
    );
}

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
public async Task<IActionResult> Register(RegisterDto dto)
{
    if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        return BadRequest(new { message = "Email already registered" });

    var user = new User
    {
        Name = dto.Name,
        Email = dto.Email,
        PasswordHash = HashPassword(dto.Password),
        Role = "Student"
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    return Ok(new { message = "User created" });
}


        [HttpPost("login")]
public async Task<IActionResult> Login(LoginDto dto)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
    if (user == null) 
        return Unauthorized("Invalid email or password");

    using var hmac = new HMACSHA256();
    var computedHash = Convert.ToBase64String(
        hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password))
    );

    if (HashPassword(dto.Password) != user.PasswordHash)
    return Unauthorized(new { message = "Invalid email or password" });

    return Ok(new
    {
        token = CreateToken(user)
    });
}


        private string CreateToken(User user)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
    );

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(3),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

    }
}
