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
    public class TourExecutionController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;
        private const double PROXIMITY_THRESHOLD_METERS = 50; // 50 metara

        public TourExecutionController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // POST: api/tourexecution/start
        [HttpPost("start")]
        public async Task<ActionResult<TourExecutionDto>> StartTour(StartTourExecutionDto dto)
        {
            var touristId = GetCurrentUserId();

            // Proveri da li tura postoji
            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == dto.TourId)
                .FirstOrDefaultAsync();

            if (tour == null)
                return NotFound(new { message = "Tour not found" });

            // Proveri da li je tura Published ili Archived
            if (tour.Status == TourStatus.Draft)
                return BadRequest(new { message = "Cannot start a draft tour" });

            // Proveri da li je kupljena (ako je Published)
            if (tour.Status == TourStatus.Published)
            {
                var purchased = await _mongoDbService.PurchaseTokens
                    .Find(pt => pt.TouristId == touristId && pt.TourId == dto.TourId)
                    .FirstOrDefaultAsync();

                if (purchased == null)
                    return BadRequest(new { message = "You must purchase this tour before starting it" });
            }

            // Proveri da li ima aktivnu sesiju
            var activeExecution = await _mongoDbService.TourExecutions
                .Find(te => te.TouristId == touristId && te.Status == ExecutionStatus.Active)
                .FirstOrDefaultAsync();

            if (activeExecution != null)
                return BadRequest(new { message = "You already have an active tour session" });

            // Dobavi trenutnu poziciju turiste
            var position = await _mongoDbService.TouristPositions
                .Find(p => p.TouristId == touristId)
                .FirstOrDefaultAsync();

            if (position == null)
                return BadRequest(new { message = "Position not set. Please set your position first." });

            // Kreiraj sesiju
            var execution = new TourExecution
            {
                TouristId = touristId,
                TourId = tour.Id,
                TourName = tour.Name,
                Status = ExecutionStatus.Active,
                StartLatitude = position.Latitude,
                StartLongitude = position.Longitude,
                StartedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };

            await _mongoDbService.TourExecutions.InsertOneAsync(execution);

            return Ok(new TourExecutionDto
            {
                Id = execution.Id,
                TourId = execution.TourId,
                TourName = execution.TourName,
                Status = execution.Status,
                CompletedKeyPointsCount = 0,
                TotalKeyPointsCount = tour.KeyPoints.Count,
                StartedAt = execution.StartedAt,
                LastActivity = execution.LastActivity
            });
        }

        // POST: api/tourexecution/check-keypoint
        [HttpPost("check-keypoint")]
        public async Task<IActionResult> CheckKeyPoint(CheckKeyPointDto dto)
        {
            var touristId = GetCurrentUserId();

            // Pronađi aktivnu sesiju
            var execution = await _mongoDbService.TourExecutions
                .Find(te => te.TouristId == touristId && te.Status == ExecutionStatus.Active)
                .FirstOrDefaultAsync();

            if (execution == null)
                return NotFound(new { message = "No active tour session" });

            // Dobavi turu
            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == execution.TourId)
                .FirstOrDefaultAsync();

            if (tour == null)
                return NotFound(new { message = "Tour not found" });

            // Ažuriraj last activity
            execution.LastActivity = DateTime.UtcNow;

            // Proveri da li je blizu neke ključne tačke koja nije kompletirana
            foreach (var keyPoint in tour.KeyPoints.OrderBy(kp => kp.Order))
            {
                // Skip ako je već kompletirana
                if (execution.CompletedKeyPoints.Any(ck => ck.KeyPointOrder == keyPoint.Order))
                    continue;

                var distance = CalculateDistance(
                    dto.Latitude, dto.Longitude,
                    keyPoint.Latitude, keyPoint.Longitude);

                if (distance <= PROXIMITY_THRESHOLD_METERS)
                {
                    // Komplentiraj ključnu tačku
                    execution.CompletedKeyPoints.Add(new CompletedKeyPoint
                    {
                        KeyPointOrder = keyPoint.Order,
                        CompletedAt = DateTime.UtcNow
                    });

                    await _mongoDbService.TourExecutions.ReplaceOneAsync(
                        te => te.Id == execution.Id, execution);

                    return Ok(new
                    {
                        message = $"Key point '{keyPoint.Name}' completed!",
                        keyPointName = keyPoint.Name,
                        completedCount = execution.CompletedKeyPoints.Count,
                        totalCount = tour.KeyPoints.Count
                    });
                }
            }

            // Nije blizu nijedne ključne tačke
            await _mongoDbService.TourExecutions.ReplaceOneAsync(
                te => te.Id == execution.Id, execution);

            return Ok(new { message = "Not near any key point" });
        }

        // PUT: api/tourexecution/complete
        [HttpPut("complete")]
        public async Task<ActionResult<TourExecutionDto>> CompleteTour()
        {
            var touristId = GetCurrentUserId();

            var execution = await _mongoDbService.TourExecutions
                .Find(te => te.TouristId == touristId && te.Status == ExecutionStatus.Active)
                .FirstOrDefaultAsync();

            if (execution == null)
                return NotFound(new { message = "No active tour session" });

            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == execution.TourId)
                .FirstOrDefaultAsync();

            execution.Status = ExecutionStatus.Completed;
            execution.CompletedAt = DateTime.UtcNow;
            execution.LastActivity = DateTime.UtcNow;

            await _mongoDbService.TourExecutions.ReplaceOneAsync(
                te => te.Id == execution.Id, execution);

            return Ok(new TourExecutionDto
            {
                Id = execution.Id,
                TourId = execution.TourId,
                TourName = execution.TourName,
                Status = execution.Status,
                CompletedKeyPointsCount = execution.CompletedKeyPoints.Count,
                TotalKeyPointsCount = tour?.KeyPoints.Count ?? 0,
                StartedAt = execution.StartedAt,
                CompletedAt = execution.CompletedAt,
                LastActivity = execution.LastActivity
            });
        }

        // PUT: api/tourexecution/abandon
        [HttpPut("abandon")]
        public async Task<ActionResult<TourExecutionDto>> AbandonTour()
        {
            var touristId = GetCurrentUserId();

            var execution = await _mongoDbService.TourExecutions
                .Find(te => te.TouristId == touristId && te.Status == ExecutionStatus.Active)
                .FirstOrDefaultAsync();

            if (execution == null)
                return NotFound(new { message = "No active tour session" });

            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == execution.TourId)
                .FirstOrDefaultAsync();

            execution.Status = ExecutionStatus.Abandoned;
            execution.AbandonedAt = DateTime.UtcNow;
            execution.LastActivity = DateTime.UtcNow;

            await _mongoDbService.TourExecutions.ReplaceOneAsync(
                te => te.Id == execution.Id, execution);

            return Ok(new TourExecutionDto
            {
                Id = execution.Id,
                TourId = execution.TourId,
                TourName = execution.TourName,
                Status = execution.Status,
                CompletedKeyPointsCount = execution.CompletedKeyPoints.Count,
                TotalKeyPointsCount = tour?.KeyPoints.Count ?? 0,
                StartedAt = execution.StartedAt,
                LastActivity = execution.LastActivity
            });
        }

        // GET: api/tourexecution/active
        [HttpGet("active")]
        public async Task<ActionResult<TourExecutionDto>> GetActiveTour()
        {
            var touristId = GetCurrentUserId();

            var execution = await _mongoDbService.TourExecutions
                .Find(te => te.TouristId == touristId && te.Status == ExecutionStatus.Active)
                .FirstOrDefaultAsync();

            if (execution == null)
                return NotFound(new { message = "No active tour session" });

            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == execution.TourId)
                .FirstOrDefaultAsync();

            return Ok(new TourExecutionDto
            {
                Id = execution.Id,
                TourId = execution.TourId,
                TourName = execution.TourName,
                Status = execution.Status,
                CompletedKeyPointsCount = execution.CompletedKeyPoints.Count,
                TotalKeyPointsCount = tour?.KeyPoints.Count ?? 0,
                StartedAt = execution.StartedAt,
                LastActivity = execution.LastActivity
            });
        }

        // GET: api/tourexecution/history
        [HttpGet("history")]
        public async Task<ActionResult<List<TourExecutionDto>>> GetExecutionHistory()
        {
            var touristId = GetCurrentUserId();

            var executions = await _mongoDbService.TourExecutions
                .Find(te => te.TouristId == touristId)
                .SortByDescending(te => te.StartedAt)
                .ToListAsync();

            var result = new List<TourExecutionDto>();

            foreach (var execution in executions)
            {
                var tour = await _mongoDbService.Tours
                    .Find(t => t.Id == execution.TourId)
                    .FirstOrDefaultAsync();

                result.Add(new TourExecutionDto
                {
                    Id = execution.Id,
                    TourId = execution.TourId,
                    TourName = execution.TourName,
                    Status = execution.Status,
                    CompletedKeyPointsCount = execution.CompletedKeyPoints.Count,
                    TotalKeyPointsCount = tour?.KeyPoints.Count ?? 0,
                    StartedAt = execution.StartedAt,
                    CompletedAt = execution.CompletedAt,
                    LastActivity = execution.LastActivity
                });
            }

            return Ok(result);
        }

        // Helper method: Haversine formula za računanje distance između dve tačke
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth radius in meters

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Distance in meters
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
