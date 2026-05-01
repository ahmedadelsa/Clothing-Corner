using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopHub.API.Data;
using ShopHub.API.DTOs;
using ShopHub.API.Helpers;
using ShopHub.API.Models;
using System.Security.Claims;

namespace ShopHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CheckoutDto dto)
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest(new ApiResponse(false, "Cart is empty"));

            var order = new Order
            {
                UserId = userId,
                ShippingAddress = dto.ShippingAddress,
                PhoneNumber = dto.PhoneNumber,
                TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity),
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderItems = cart.CartItems.Select(ci => new OrderItem
            {
                OrderId = order.Id,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                Price = ci.Product.Price
            }).ToList();

            _context.OrderItems.AddRange(orderItems);
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse(true, "Order created successfully", new
            {
                orderId = order.Id
            }));
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Select(o => new
                {
                    o.Id,
                    o.TotalAmount,
                    o.Status,
                    o.ShippingAddress,
                    o.PhoneNumber,
                    o.CreatedAt,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.ProductId,
                        ProductName = oi.Product.Name,
                        oi.Quantity,
                        oi.Price,
                        TotalPrice = oi.Price * oi.Quantity
                    })
                })
                .ToListAsync();

            return Ok(new ApiResponse(true, "Orders retrieved successfully", orders));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = GetUserId();

            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Select(o => new
                {
                    o.Id,
                    o.TotalAmount,
                    o.Status,
                    o.ShippingAddress,
                    o.PhoneNumber,
                    o.CreatedAt,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.ProductId,
                        ProductName = oi.Product.Name,
                        oi.Quantity,
                        oi.Price,
                        TotalPrice = oi.Price * oi.Quantity
                    })
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound(new ApiResponse(false, "Order not found"));

            return Ok(new ApiResponse(true, "Order retrieved successfully", order));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .Select(o => new
                {
                    o.Id,
                    Customer = o.User.Email,
                    o.TotalAmount,
                    o.Status,
                    o.CreatedAt,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.ProductId,
                        ProductName = oi.Product.Name,
                        oi.Quantity,
                        oi.Price
                    })
                })
                .ToListAsync();

            return Ok(new ApiResponse(true, "All orders retrieved successfully", orders));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var allowedStatuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

            if (!allowedStatuses.Contains(status))
                return BadRequest(new ApiResponse(false, "Invalid order status"));

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound(new ApiResponse(false, "Order not found"));

            order.Status = status;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse(true, "Order status updated"));
        }
    }
}