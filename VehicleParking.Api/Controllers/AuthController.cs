using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VehicleParking.Api.Data;
using VehicleParking.Api.DTOs;
using VehicleParking.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace VehicleParking.Api.Controllers
{
    [Route("api/[controller]")] // api/auth
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. Admin-Only User Creation Endpoint (POST: api/auth/create-user)
        [HttpPost("create-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] User model)
        {
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email is already registered!" });
            }

            // Password Hashing
            var passwordHasher = new PasswordHasher<User>();
            model.Password = passwordHasher.HashPassword(model, model.Password);

            model.CreatedAt = DateTime.Now;

            // If role does not define, so "User" by default selected
            if (string.IsNullOrEmpty(model.Role))
            {
                model.Role = "User";
            }

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User account created successfully by Admin with secure password!" });
        }

        // 2. Login Endpoint (POST: api/auth/login)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password!" });
            }

            var passwordHasher = new PasswordHasher<User>();
            bool isPasswordValid = false;

            try
            {
                // 1. Try secure hash verification
                var verificationResult = passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
                if (verificationResult == PasswordVerificationResult.Success)
                {
                    isPasswordValid = true;
                }
            }
            catch (FormatException)
            {
                // 2. Fallback: Agar database mein purana plain text password parha ho
                if (user.Password == model.Password)
                {
                    isPasswordValid = true;
                    user.Password = passwordHasher.HashPassword(user, model.Password);
                    await _context.SaveChangesAsync();
                }
            }

            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Invalid email or password!" });
            }

            // JWT Token generate 
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        }),
                Expires = DateTime.UtcNow.AddHours(3),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Token = tokenString,
                Message = "Login successful!",
                User = new { user.Id, user.FullName, user.Email, user.Role }
            });
        }
    }
}