using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;
using Tours.API.DTOs;
using Tours.API.Models;
using Tours.API.Services;
using static Tours.API.Models.Enums;

namespace Tours.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ToursController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;

        public ToursController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        private async Task<bool> HasPurchasedTour(int touristId, string tourId)
        {
            var token = await _mongoDbService.PurchaseTokens
                .Find(pt => pt.TouristId == touristId && pt.TourId == tourId)
                .FirstOrDefaultAsync();

            return token != null;
        }

        // POST: api/tours
        [HttpPost]
        public async Task<ActionResult<TourDto>> CreateTour(CreateTourDto dto)
        {
            var authorId = GetCurrentUserId();

            var tour = new Tour
            {
                AuthorId = authorId,
                Name = dto.Name,
                Description = dto.Description,
                Difficulty = dto.Difficulty,
                Tags = dto.Tags,
                DurationHours = dto.DurationHours,
                Status = TourStatus.Draft,
                Price = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _mongoDbService.Tours.InsertOneAsync(tour);

            return CreatedAtAction(nameof(GetTourById), new { id = tour.Id }, MapToDto(tour));
        }

        // GET: api/tours/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<TourDto>> GetTourById(string id)
        {
            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == id)
                .FirstOrDefaultAsync();

            if (tour == null)
                return NotFound(new { message = "Tour not found" });

            // Proveri da li je korisnik ulogovan
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool isPurchased = false;

            if (int.TryParse(userIdClaim, out int userId))
            {
                isPurchased = await HasPurchasedTour(userId, id);
            }

            // Ako je autor ili je kupljena, vrati sve informacije
            if (tour.AuthorId == (int.TryParse(userIdClaim, out int authorCheck) ? authorCheck : 0) || isPurchased)
            {
                return Ok(MapToDto(tour));
            }

            // Inače vrati ograničene informacije (bez svih ključnih tačaka)
            return Ok(new TourDto
            {
                Id = tour.Id,
                AuthorId = tour.AuthorId,
                Name = tour.Name,
                Description = tour.Description,
                Difficulty = tour.Difficulty,
                Status = tour.Status,
                Price = tour.Price,
                Tags = tour.Tags,
                KeyPointsCount = tour.KeyPoints.Count,
                StartPoint = tour.StartPoint != null ? MapToKeyPointDto(tour.StartPoint) : null,
                EndPoint = null, // Ne prikazuj krajnju tačku
                DurationHours = tour.DurationHours,
                CreatedAt = tour.CreatedAt
            });
        }

        // GET: api/tours/my
        [HttpGet("my")]
        public async Task<ActionResult<List<TourDto>>> GetMyTours()
        {
            var authorId = GetCurrentUserId();

            var tours = await _mongoDbService.Tours
                .Find(t => t.AuthorId == authorId)
                .SortByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tours.Select(MapToDto).ToList());
        }

        // GET: api/tours/published
        [HttpGet("published")]
        [AllowAnonymous]
        public async Task<ActionResult<List<TourDto>>> GetPublishedTours()
        {
            var tours = await _mongoDbService.Tours
                .Find(t => t.Status == TourStatus.Published)
                .SortByDescending(t => t.PublishedAt)
                .ToListAsync();

            return Ok(tours.Select(MapToDto).ToList());
        }

        // PUT: api/tours/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<TourDto>> UpdateTour(string id, UpdateTourDto dto)
        {
            var authorId = GetCurrentUserId();

            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == id && t.AuthorId == authorId)
                .FirstOrDefaultAsync();

            if (tour == null)
                return NotFound(new { message = "Tour not found or you're not the author" });

            tour.Name = dto.Name;
            tour.Description = dto.Description;
            tour.Difficulty = dto.Difficulty;
            tour.Tags = dto.Tags;
            tour.DurationHours = dto.DurationHours;

            await _mongoDbService.Tours.ReplaceOneAsync(t => t.Id == id, tour);

            return Ok(MapToDto(tour));
        }

        // DELETE: api/tours/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTour(string id)
        {
            var authorId = GetCurrentUserId();

            var result = await _mongoDbService.Tours
                .DeleteOneAsync(t => t.Id == id && t.AuthorId == authorId);

            if (result.DeletedCount == 0)
                return NotFound(new { message = "Tour not found or you're not the author" });

            return Ok(new { message = "Tour deleted successfully" });
        }

        // POST: api/tours/{id}/keypoints
        [HttpPost("{id}/keypoints")]
        public async Task<ActionResult<KeyPointDto>> AddKeyPoint(string id, AddKeyPointDto dto)
        {
            var authorId = GetCurrentUserId();

            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == id && t.AuthorId == authorId)
                .FirstOrDefaultAsync();

            if (tour == null)
                return NotFound(new { message = "Tour not found or you're not the author" });

            var keyPoint = new KeyPoint
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Order = dto.Order
            };

            tour.KeyPoints.Add(keyPoint);

            await _mongoDbService.Tours.ReplaceOneAsync(t => t.Id == id, tour);

            return Ok(MapToKeyPointDto(keyPoint));
        }

        // GET: api/tours/{id}/keypoints
        [HttpGet("{id}/keypoints")]
        public async Task<ActionResult<List<KeyPointDto>>> GetKeyPoints(string id)
        {
            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == id)
                .FirstOrDefaultAsync();

            if (tour == null)
                return NotFound(new { message = "Tour not found" });

            var userId = GetCurrentUserId();

            // Proveri da li je autor ili je kupljena
            bool isPurchased = await HasPurchasedTour(userId, id);

            if (tour.AuthorId != userId && !isPurchased)
            {
                return Unauthorized(new { message = "You must purchase this tour to see all key points" });
            }

            return Ok(tour.KeyPoints.OrderBy(kp => kp.Order).Select(MapToKeyPointDto).ToList());
        }

        // PUT: api/tours/{id}/publish
        [HttpPut("{id}/publish")]
        public async Task<ActionResult<TourDto>> PublishTour(string id, PublishTourDto dto)
        {
            var authorId = GetCurrentUserId();

            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == id && t.AuthorId == authorId)
                .FirstOrDefaultAsync();

            if (tour == null)
                return NotFound(new { message = "Tour not found or you're not the author" });

            if (tour.KeyPoints.Count < 2)
                return BadRequest(new { message = "Tour must have at least 2 key points (start and end)" });

            tour.Status = TourStatus.Published;
            tour.Price = dto.Price;
            tour.PublishedAt = DateTime.UtcNow;

            await _mongoDbService.Tours.ReplaceOneAsync(t => t.Id == id, tour);

            return Ok(MapToDto(tour));
        }

        // PUT: api/tours/{id}/archive
        [HttpPut("{id}/archive")]
        public async Task<ActionResult<TourDto>> ArchiveTour(string id)
        {
            var authorId = GetCurrentUserId();

            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == id && t.AuthorId == authorId)
                .FirstOrDefaultAsync();

            if (tour == null)
                return NotFound(new { message = "Tour not found or you're not the author" });

            tour.Status = TourStatus.Archived;
            tour.ArchivedAt = DateTime.UtcNow;

            await _mongoDbService.Tours.ReplaceOneAsync(t => t.Id == id, tour);

            return Ok(MapToDto(tour));
        }

        // Helper methods
        private TourDto MapToDto(Tour tour)
        {
            return new TourDto
            {
                Id = tour.Id,
                AuthorId = tour.AuthorId,
                Name = tour.Name,
                Description = tour.Description,
                Difficulty = tour.Difficulty,
                Status = tour.Status,
                Price = tour.Price,
                Tags = tour.Tags,
                KeyPointsCount = tour.KeyPoints.Count,
                StartPoint = tour.StartPoint != null ? MapToKeyPointDto(tour.StartPoint) : null,
                EndPoint = tour.EndPoint != null ? MapToKeyPointDto(tour.EndPoint) : null,
                DurationHours = tour.DurationHours,
                CreatedAt = tour.CreatedAt
            };
        }

        private KeyPointDto MapToKeyPointDto(KeyPoint kp)
        {
            return new KeyPointDto
            {
                Name = kp.Name,
                Description = kp.Description,
                ImageUrl = kp.ImageUrl,
                Latitude = kp.Latitude,
                Longitude = kp.Longitude,
                Order = kp.Order
            };
        }
    }
}
