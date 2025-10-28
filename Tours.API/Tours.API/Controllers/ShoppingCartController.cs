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
    public class ShoppingCartController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;

        public ShoppingCartController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        // GET: api/shoppingcart
        [HttpGet]
        public async Task<ActionResult<ShoppingCartDto>> GetCart()
        {
            var touristId = GetCurrentUserId();

            var cart = await _mongoDbService.ShoppingCarts
                .Find(c => c.TouristId == touristId)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                return Ok(new ShoppingCartDto
                {
                    Items = new List<OrderItemDto>(),
                    TotalPrice = 0
                });
            }

            return Ok(new ShoppingCartDto
            {
                Items = cart.Items.Select(i => new OrderItemDto
                {
                    TourId = i.TourId,
                    TourName = i.TourName,
                    Price = i.Price
                }).ToList(),
                TotalPrice = cart.TotalPrice
            });
        }

        // POST: api/shoppingcart/add
        [HttpPost("add")]
        public async Task<ActionResult<ShoppingCartDto>> AddToCart(AddToCartDto dto)
        {
            var touristId = GetCurrentUserId();

            // Proveri da li tura postoji i da li je Published
            var tour = await _mongoDbService.Tours
                .Find(t => t.Id == dto.TourId && t.Status == TourStatus.Published)
                .FirstOrDefaultAsync();

            if (tour == null)
                return NotFound(new { message = "Tour not found or not available for purchase" });

            // Proveri da li je već kupljena
            var alreadyPurchased = await _mongoDbService.PurchaseTokens
                .Find(pt => pt.TouristId == touristId && pt.TourId == dto.TourId)
                .FirstOrDefaultAsync();

            if (alreadyPurchased != null)
                return BadRequest(new { message = "You already own this tour" });

            var cart = await _mongoDbService.ShoppingCarts
                .Find(c => c.TouristId == touristId)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                // Kreiraj novu korpu
                cart = new ShoppingCart
                {
                    TouristId = touristId,
                    Items = new List<OrderItem>(),
                    TotalPrice = 0
                };
                await _mongoDbService.ShoppingCarts.InsertOneAsync(cart);
            }

            // Proveri da li je tura već u korpi
            if (cart.Items.Any(i => i.TourId == dto.TourId))
                return BadRequest(new { message = "Tour already in cart" });

            // Dodaj turu u korpu
            cart.Items.Add(new OrderItem
            {
                TourId = tour.Id,
                TourName = tour.Name,
                Price = tour.Price
            });

            // Preračunaj ukupnu cenu
            cart.TotalPrice = cart.Items.Sum(i => i.Price);
            cart.UpdatedAt = DateTime.UtcNow;

            await _mongoDbService.ShoppingCarts.ReplaceOneAsync(
                c => c.TouristId == touristId, cart);

            return Ok(new ShoppingCartDto
            {
                Items = cart.Items.Select(i => new OrderItemDto
                {
                    TourId = i.TourId,
                    TourName = i.TourName,
                    Price = i.Price
                }).ToList(),
                TotalPrice = cart.TotalPrice
            });
        }

        // DELETE: api/shoppingcart/remove/{tourId}
        [HttpDelete("remove/{tourId}")]
        public async Task<ActionResult<ShoppingCartDto>> RemoveFromCart(string tourId)
        {
            var touristId = GetCurrentUserId();

            var cart = await _mongoDbService.ShoppingCarts
                .Find(c => c.TouristId == touristId)
                .FirstOrDefaultAsync();

            if (cart == null)
                return NotFound(new { message = "Cart not found" });

            cart.Items = cart.Items.Where(i => i.TourId != tourId).ToList();
            cart.TotalPrice = cart.Items.Sum(i => i.Price);
            cart.UpdatedAt = DateTime.UtcNow;

            await _mongoDbService.ShoppingCarts.ReplaceOneAsync(
                c => c.TouristId == touristId, cart);

            return Ok(new ShoppingCartDto
            {
                Items = cart.Items.Select(i => new OrderItemDto
                {
                    TourId = i.TourId,
                    TourName = i.TourName,
                    Price = i.Price
                }).ToList(),
                TotalPrice = cart.TotalPrice
            });
        }

        // POST: api/shoppingcart/checkout
        [HttpPost("checkout")]
        public async Task<ActionResult<List<PurchaseTokenDto>>> Checkout()
        {
            var touristId = GetCurrentUserId();

            var cart = await _mongoDbService.ShoppingCarts
                .Find(c => c.TouristId == touristId)
                .FirstOrDefaultAsync();

            if (cart == null || cart.Items.Count == 0)
                return BadRequest(new { message = "Cart is empty" });

            // Kreiraj purchase token za svaku stavku
            var purchaseTokens = new List<TourPurchaseToken>();

            foreach (var item in cart.Items)
            {
                var token = new TourPurchaseToken
                {
                    TouristId = touristId,
                    TourId = item.TourId,
                    TourName = item.TourName,
                    PricePaid = item.Price,
                    PurchasedAt = DateTime.UtcNow
                };
                purchaseTokens.Add(token);
            }

            await _mongoDbService.PurchaseTokens.InsertManyAsync(purchaseTokens);

            // Isprazni korpu
            cart.Items.Clear();
            cart.TotalPrice = 0;
            await _mongoDbService.ShoppingCarts.ReplaceOneAsync(
                c => c.TouristId == touristId, cart);

            return Ok(purchaseTokens.Select(pt => new PurchaseTokenDto
            {
                Id = pt.Id,
                TourId = pt.TourId,
                TourName = pt.TourName,
                PricePaid = pt.PricePaid,
                PurchasedAt = pt.PurchasedAt
            }).ToList());
        }

        // GET: api/shoppingcart/purchased
        [HttpGet("purchased")]
        public async Task<ActionResult<List<PurchaseTokenDto>>> GetPurchasedTours()
        {
            var touristId = GetCurrentUserId();

            var tokens = await _mongoDbService.PurchaseTokens
                .Find(pt => pt.TouristId == touristId)
                .SortByDescending(pt => pt.PurchasedAt)
                .ToListAsync();

            return Ok(tokens.Select(pt => new PurchaseTokenDto
            {
                Id = pt.Id,
                TourId = pt.TourId,
                TourName = pt.TourName,
                PricePaid = pt.PricePaid,
                PurchasedAt = pt.PurchasedAt
            }).ToList());
        }
    }
}
