using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHub.API.Data;
using ShopHub.API.DTOs;
using ShopHub.API.Models;
using System.Security.Claims;

namespace ShopHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return Ok(new { items = new List<object>(), total = 0 });

            var result = new
            {
                items = cart.CartItems.Select(ci => new
                {
                    ci.ProductId,
                    productName = ci.Product.Name,
                    ci.Product.ImageUrl,
                    ci.Product.Price,
                    ci.Quantity,
                    totalPrice = ci.Product.Price * ci.Quantity
                }),
                total = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity)
            };

            return Ok(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            var userId = GetUserId();

            var product = await _context.Products.FindAsync(dto.ProductId);

            if (product == null)
                return NotFound("Product not found");

            if (dto.Quantity <= 0)
                return BadRequest("Quantity must be greater than zero");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return Ok("Product added to cart");
        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity(AddToCartDto dto)
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found");

            var item = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == dto.ProductId);

            if (item == null)
                return NotFound("Item not found in cart");

            if (dto.Quantity <= 0)
                return BadRequest("Quantity must be greater than zero");

            item.Quantity = dto.Quantity;

            await _context.SaveChangesAsync();

            return Ok("Quantity updated");
        }


        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found");

            var item = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productId);

            if (item == null)
                return NotFound("Item not found");

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok("Item removed");
        }
    }
}