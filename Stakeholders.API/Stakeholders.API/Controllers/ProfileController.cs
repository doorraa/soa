using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stakeholders.API.Data;
using Stakeholders.API.DTOs;
using System.Security.Claims;

namespace Stakeholders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly StakeholdersDbContext _context;

        public ProfileController(StakeholdersDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userId = GetCurrentUserId();

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Profile == null)
            {
                return NotFound(new { message = "Profile not found" });
            }

            return Ok(new UserProfileDto
            {
                UserId = user.Id,
                Username = user.Username,
                FirstName = user.Profile.FirstName,
                LastName = user.Profile.LastName,
                Bio = user.Profile.Bio,
                Motto = user.Profile.Motto,
                ProfilePictureUrl = user.Profile.ProfilePictureUrl
            });
        }

        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<ActionResult<UserProfileDto>> GetUserProfile(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Profile == null)
            {
                return NotFound(new { message = "Profile not found" });
            }

            return Ok(new UserProfileDto
            {
                UserId = user.Id,
                Username = user.Username,
                FirstName = user.Profile.FirstName,
                LastName = user.Profile.LastName,
                Bio = user.Profile.Bio,
                Motto = user.Profile.Motto,
                ProfilePictureUrl = user.Profile.ProfilePictureUrl
            });
        }

        [HttpPut("me")]
        public async Task<ActionResult<UserProfileDto>> UpdateMyProfile(UpdateProfileDto dto)
        {
            var userId = GetCurrentUserId();

            var profile = await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                return NotFound(new { message = "Profile not found" });
            }

            profile.FirstName = dto.FirstName;
            profile.LastName = dto.LastName;
            profile.Bio = dto.Bio;
            profile.Motto = dto.Motto;
            profile.ProfilePictureUrl = dto.ProfilePictureUrl;

            await _context.SaveChangesAsync();

            return Ok(new UserProfileDto
            {
                UserId = profile.UserId,
                Username = profile.User.Username,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Bio = profile.Bio,
                Motto = profile.Motto,
                ProfilePictureUrl = profile.ProfilePictureUrl
            });
        }

        // GET: api/profile/search?username=test
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<List<UserProfileDto>>> SearchUsers(string username)
        {
            if (string.IsNullOrEmpty(username) || username.Length < 2)
            {
                return BadRequest(new { message = "Username must be at least 2 characters" });
            }

            var users = await _context.Users
                .Include(u => u.Profile)
                .Where(u => u.Username.ToLower().Contains(username.ToLower()))
                .Take(10) // Limit to 10 results
                .Select(u => new UserProfileDto
                {
                    UserId = u.Id,
                    Username = u.Username,
                    FirstName = u.Profile != null ? u.Profile.FirstName : "",
                    LastName = u.Profile != null ? u.Profile.LastName : "",
                    Bio = u.Profile != null ? u.Profile.Bio : "",
                    Motto = u.Profile != null ? u.Profile.Motto : "",
                    ProfilePictureUrl = u.Profile != null ? u.Profile.ProfilePictureUrl : ""
                })
                .ToListAsync();

            return Ok(users);
        }

    }
}
