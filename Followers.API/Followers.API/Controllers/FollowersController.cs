using Followers.API.Models;
using Followers.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using System.Security.Claims;

namespace Followers.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FollowersController : ControllerBase
    {
        private readonly Neo4jService _neo4jService;

        public FollowersController(Neo4jService neo4jService)
        {
            _neo4jService = neo4jService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // POST: api/followers/follow/{userId}
        [HttpPost("follow/{userId}")]
        public async Task<IActionResult> FollowUser(int userId)
        {
            var followerId = GetCurrentUserId();

            if (followerId == userId)
                return BadRequest(new { message = "Cannot follow yourself" });

            await using var session = _neo4jService.GetSession();

            await session.ExecuteWriteAsync(async tx =>
            {
                var query = @"
                    MERGE (follower:User {id: $followerId})
                    MERGE (following:User {id: $followingId})
                    MERGE (follower)-[r:FOLLOWS {followedAt: datetime()}]->(following)
                    RETURN r";

                await tx.RunAsync(query, new { followerId, followingId = userId });
            });

            return Ok(new { message = "User followed successfully" });
        }

        // DELETE: api/followers/unfollow/{userId}
        [HttpDelete("unfollow/{userId}")]
        public async Task<IActionResult> UnfollowUser(int userId)
        {
            var followerId = GetCurrentUserId();

            await using var session = _neo4jService.GetSession();

            await session.ExecuteWriteAsync(async tx =>
            {
                var query = @"
                    MATCH (follower:User {id: $followerId})-[r:FOLLOWS]->(following:User {id: $followingId})
                    DELETE r";

                await tx.RunAsync(query, new { followerId, followingId = userId });
            });

            return Ok(new { message = "User unfollowed successfully" });
        }

        // GET: api/followers/following
        [HttpGet("following")]
        public async Task<ActionResult<List<User>>> GetFollowing()
        {
            var userId = GetCurrentUserId();

            await using var session = _neo4jService.GetSession();

            var users = await session.ExecuteReadAsync(async tx =>
            {
                var query = @"
                    MATCH (user:User {id: $userId})-[:FOLLOWS]->(following:User)
                    RETURN following.id AS id, following.username AS username";

                var result = await tx.RunAsync(query, new { userId });
                var records = new List<IRecord>();

                await foreach (var record in result)
                {
                    records.Add(record);
                }

                return records.Select(record => new User
                {
                    Id = record["id"].As<int>(),
                    Username = record["username"]?.As<string>() ?? ""
                }).ToList();
            });

            return Ok(users);
        }

        // GET: api/followers/followers
        [HttpGet("followers")]
        public async Task<ActionResult<List<User>>> GetFollowers()
        {
            var userId = GetCurrentUserId();

            await using var session = _neo4jService.GetSession();

            var users = await session.ExecuteReadAsync(async tx =>
            {
                var query = @"
                    MATCH (follower:User)-[:FOLLOWS]->(user:User {id: $userId})
                    RETURN follower.id AS id, follower.username AS username";

                var result = await tx.RunAsync(query, new { userId });
                var records = await result.ToListAsync();

                return records.Select(record => new User
                {
                    Id = record["id"].As<int>(),
                    Username = record["username"]?.As<string>() ?? ""
                }).ToList();
            });

            return Ok(users);
        }

        // GET: api/followers/recommendations
        [HttpGet("recommendations")]
        public async Task<ActionResult<List<User>>> GetRecommendations()
        {
            var userId = GetCurrentUserId();

            await using var session = _neo4jService.GetSession();

            var recommendations = await session.ExecuteReadAsync(async tx =>
            {
                var query = @"
                    MATCH (user:User {id: $userId})-[:FOLLOWS]->(friend:User)-[:FOLLOWS]->(recommendation:User)
                    WHERE NOT (user)-[:FOLLOWS]->(recommendation) AND recommendation.id <> $userId
                    RETURN DISTINCT recommendation.id AS id, recommendation.username AS username, COUNT(*) AS commonFriends
                    ORDER BY commonFriends DESC
                    LIMIT 10";

                var result = await tx.RunAsync(query, new { userId });
                var records = await result.ToListAsync();

                return records.Select(record => new User
                {
                    Id = record["id"].As<int>(),
                    Username = record["username"]?.As<string>() ?? ""
                }).ToList();
            });

            return Ok(recommendations);
        }

        // GET: api/followers/is-following/{userId}
        [HttpGet("is-following/{userId}")]
        public async Task<ActionResult<bool>> IsFollowing(int userId)
        {
            var followerId = GetCurrentUserId();

            await using var session = _neo4jService.GetSession();

            var isFollowing = await session.ExecuteReadAsync(async tx =>
            {
                var query = @"
            MATCH (follower:User {id: $followerId})-[:FOLLOWS]->(following:User {id: $followingId})
            RETURN COUNT(*) > 0 AS isFollowing";

                var result = await tx.RunAsync(query, new { followerId, followingId = userId });
                var record = await result.SingleAsync();
                return record["isFollowing"].As<bool>();
            });

            return Ok(new { isFollowing });
        }
    }
}
