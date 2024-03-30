using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using todolistApiv2.Data;
using todolistApiv2.Models;

namespace todolistApiv2.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly DataContext _context;

        public AuthenticationController(IConfiguration config, DataContext context)
        {
            _configuration = config;
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            // Check if the username already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists");
            }

            // Hash the password before saving it to the database
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] hashedBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(user.Password));

                // Convert the byte array to a string representation
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    builder.Append(hashedBytes[i].ToString("x2"));
                }
                user.Password = builder.ToString();
            }


            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Clear the password before returning the user object
            user.Password = null;

            // For demo purposes, returning the user object (you might want to return a DTO)
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(User _userData)
        {
            IActionResult response = Unauthorized();
            if (_userData != null && _userData.Username != null && _userData.Password != null)
            {
                var user = await GetUser(_userData.Username);

                if (user != null)
                {
                    // Hash the password provided during login
                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        byte[] hashedBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(_userData.Password));

                        // Convert the byte array to a string representation
                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < hashedBytes.Length; i++)
                        {
                            builder.Append(hashedBytes[i].ToString("x2"));
                        }
                        string hashedPassword = builder.ToString();

                        // Compare the hashed password with the one stored in the database
                        if (user.Password == hashedPassword)
                        {
                            var token = GenerateToken(user);
                            response = Ok(new { token = token });
                        }
                        return response;

                    }
                }
                else
                {
                    return BadRequest("Invalid credentials");
                }
            }
            else
            {
                return BadRequest();
            }
        }
        private string GenerateToken(User users)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                 new Claim(ClaimTypes.NameIdentifier, users.UserId.ToString()), // Include user ID as a claim
            };
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims,
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private async Task<User> GetUser(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
