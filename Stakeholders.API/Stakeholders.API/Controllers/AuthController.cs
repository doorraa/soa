using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stakeholders.API.Data;
using Stakeholders.API.DTOs;
using Stakeholders.API.Models;
using Stakeholders.API.Services;
using BCrypt.Net;

namespace Stakeholders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly StakeholdersDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(StakeholdersDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDto>> Register(RegisterDto dto)
        {
            // Provera da li već postoji korisnik
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return BadRequest(new { message = "Username already exists" });
            }

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Kreiranje korisnika
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Kreiranje praznog profila
            var profile = new UserProfile
            {
                UserId = user.Id,
                FirstName = "",
                LastName = "",
                Bio = "",
                Motto = "",
                ProfilePictureUrl = ""
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            // Generisanje tokena
            var token = _jwtService.GenerateToken(user);

            return Ok(new LoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new LoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Role = user.Role
            });
        }
    }
}
