using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Tours.API.DTOs;
using Tours.API.Models;
using Tours.API.Services;
using System.Security.Claims;


namespace Tours.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PositionController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;

        public PositionController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // PUT: api/position
        [HttpPut]
        public async Task<ActionResult<PositionDto>> UpdatePosition(UpdatePositionDto dto)
        {
            var touristId = GetCurrentUserId();

            var position = await _mongoDbService.TouristPositions
                .Find(p => p.TouristId == touristId)
                .FirstOrDefaultAsync();

            if (position == null)
            {
                // Kreiraj novu poziciju
                position = new TouristPosition
                {
                    TouristId = touristId,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    UpdatedAt = DateTime.UtcNow
                };
                await _mongoDbService.TouristPositions.InsertOneAsync(position);
            }
            else
            {
                // Ažuriraj postojeću
                position.Latitude = dto.Latitude;
                position.Longitude = dto.Longitude;
                position.UpdatedAt = DateTime.UtcNow;

                await _mongoDbService.TouristPositions.ReplaceOneAsync(
                    p => p.TouristId == touristId, position);
            }

            return Ok(new PositionDto
            {
                Latitude = position.Latitude,
                Longitude = position.Longitude,
                UpdatedAt = position.UpdatedAt
            });
        }

        // GET: api/position
        [HttpGet]
        public async Task<ActionResult<PositionDto>> GetMyPosition()
        {
            var touristId = GetCurrentUserId();

            var position = await _mongoDbService.TouristPositions
                .Find(p => p.TouristId == touristId)
                .FirstOrDefaultAsync();

            if (position == null)
                return NotFound(new { message = "Position not set" });

            return Ok(new PositionDto
            {
                Latitude = position.Latitude,
                Longitude = position.Longitude,
                UpdatedAt = position.UpdatedAt
            });
        }
    }
}
