using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stakeholders.API.Data;
using Stakeholders.API.DTOs;
using Stakeholders.API.Models;
using Stakeholders.API.Services;
using System.Security.Claims;

namespace Stakeholders.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BlogController : ControllerBase
    {
        private readonly StakeholdersDbContext _context;
        private readonly FollowersService _followersService;

        public BlogController(StakeholdersDbContext context, FollowersService followersService)
        {
            _context = context;
            _followersService = followersService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // POST: api/blog
        [HttpPost]
        public async Task<ActionResult<BlogDto>> CreateBlog(CreateBlogDto dto)
        {
            var userId = GetCurrentUserId();

            var blog = new Blog
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                ImageUrls = dto.ImageUrls,
                CreatedAt = DateTime.UtcNow
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return CreatedAtAction(nameof(GetBlogById), new { id = blog.Id }, new BlogDto
            {
                Id = blog.Id,
                UserId = blog.UserId,
                Username = user?.Username ?? "",
                Title = blog.Title,
                Description = blog.Description,
                CreatedAt = blog.CreatedAt,
                ImageUrls = blog.ImageUrls,
                CommentCount = 0
            });
        }

        // GET: api/blog/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogDto>> GetBlogById(int id)
        {
            var blog = await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
                return NotFound(new { message = "Blog not found" });

            var currentUserId = GetCurrentUserId();

            // Provera: Korisnik može videti svoj blog ili blogove koje prati
            if (blog.UserId != currentUserId)
            {
                var token = GetBearerToken();
                var isFollowing = await _followersService.IsFollowingAsync(currentUserId, blog.UserId, token);

                if (!isFollowing)
                {
                    return Forbid(); // Ili: return Unauthorized(new { message = "You must follow this user to read their blog" });
                }
            }

            return Ok(new BlogDto
            {
                Id = blog.Id,
                UserId = blog.UserId,
                Username = blog.User.Username,
                Title = blog.Title,
                Description = blog.Description,
                CreatedAt = blog.CreatedAt,
                ImageUrls = blog.ImageUrls,
                CommentCount = blog.Comments.Count
            });
        }

        // GET: api/blog/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<BlogDto>>> GetBlogsByUser(int userId)
        {
            var currentUserId = GetCurrentUserId();

            // Provera: Korisnik može videti svoje blogove ili blogove koje prati
            if (userId != currentUserId)
            {
                var token = GetBearerToken();
                var isFollowing = await _followersService.IsFollowingAsync(currentUserId, userId, token);

                if (!isFollowing)
                {
                    return Forbid(); // Ili: return Unauthorized(new { message = "You must follow this user to read their blogs" });
                }
            }

            var blogs = await _context.Blogs
                .Where(b => b.UserId == userId)
                .Include(b => b.User)
                .Include(b => b.Comments)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BlogDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    Username = b.User.Username,
                    Title = b.Title,
                    Description = b.Description,
                    CreatedAt = b.CreatedAt,
                    ImageUrls = b.ImageUrls,
                    CommentCount = b.Comments.Count
                })
                .ToListAsync();

            return Ok(blogs);
        }

        // GET: api/blog/my
        [HttpGet("my")]
        public async Task<ActionResult<List<BlogDto>>> GetMyBlogs()
        {
            var userId = GetCurrentUserId();
            return await GetBlogsByUser(userId);
        }

        // PUT: api/blog/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<BlogDto>> UpdateBlog(int id, UpdateBlogDto dto)
        {
            var userId = GetCurrentUserId();

            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
                return NotFound(new { message = "Blog not found" });

            if (blog.UserId != userId)
                return Forbid();

            blog.Title = dto.Title;
            blog.Description = dto.Description;
            blog.ImageUrls = dto.ImageUrls;

            await _context.SaveChangesAsync();

            return Ok(new BlogDto
            {
                Id = blog.Id,
                UserId = blog.UserId,
                Username = blog.User.Username,
                Title = blog.Title,
                Description = blog.Description,
                CreatedAt = blog.CreatedAt,
                ImageUrls = blog.ImageUrls,
                CommentCount = blog.Comments.Count
            });
        }

        // DELETE: api/blog/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var userId = GetCurrentUserId();

            var blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);

            if (blog == null)
                return NotFound(new { message = "Blog not found" });

            if (blog.UserId != userId)
                return Forbid();

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Blog deleted successfully" });
        }

        // GET: api/blog/{id}/comments
        [HttpGet("{id}/comments")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CommentDto>>> GetBlogComments(int id)
        {
            var comments = await _context.BlogComments
                .Where(c => c.BlogId == id)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Username = c.User.Username,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/blog/{id}/comments
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<CommentDto>> AddComment(int id, CreateCommentDto dto)
        {
            var userId = GetCurrentUserId();

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
                return NotFound(new { message = "Blog not found" });

            // Provera: Korisnik može komentarisati svoj blog ili blogove koje prati
            if (blog.UserId != userId)
            {
                var token = GetBearerToken();
                var isFollowing = await _followersService.IsFollowingAsync(userId, blog.UserId, token);

                if (!isFollowing)
                {
                    return Forbid(); // Ili: return Unauthorized(new { message = "You must follow this user to comment on their blog" });
                }
            }

            var comment = new BlogComment
            {
                BlogId = id,
                UserId = userId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.BlogComments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return CreatedAtAction(nameof(GetBlogComments), new { id = id }, new CommentDto
            {
                Id = comment.Id,
                UserId = comment.UserId,
                Username = user?.Username ?? "",
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            });
        }

        // DELETE: api/blog/comments/{commentId}
        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = GetCurrentUserId();

            var comment = await _context.BlogComments.FindAsync(commentId);

            if (comment == null)
                return NotFound(new { message = "Comment not found" });

            if (comment.UserId != userId)
                return Forbid();

            _context.BlogComments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment deleted successfully" });
        }

        private string GetBearerToken()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return string.Empty;
        }

        // GET: api/blog/feed
        [HttpGet("feed")]
        public async Task<ActionResult<List<BlogDto>>> GetFeed()
        {
            var userId = GetCurrentUserId();

            // Za sada vraća sve blogove - možeš dodati logiku za following kasnije
            var blogs = await _context.Blogs
                .Include(b => b.User)
                .Include(b => b.Comments)
                .OrderByDescending(b => b.CreatedAt)
                .Take(50)
                .Select(b => new BlogDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    Username = b.User.Username,
                    Title = b.Title,
                    Description = b.Description,
                    CreatedAt = b.CreatedAt,
                    ImageUrls = b.ImageUrls,
                    CommentCount = b.Comments.Count
                })
                .ToListAsync();

            return Ok(blogs);
        }
    }
}
