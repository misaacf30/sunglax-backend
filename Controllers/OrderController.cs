using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using Stripe;
using webapi.Data;
using webapi.Models;
using webapi.Dtos.Order;
using Microsoft.AspNetCore.Authorization;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        public readonly DataContext _context;
        
        public OrderController(DataContext context)
        {
            _context = context;
        }
/*
        [HttpGet("get")]
        public async Task<ActionResult<List<OrderItem>>> GetAllOrders()
        {
            var orders = await _context.Orders.ToListAsync();
            if (orders == null)
            {
                return BadRequest("Orders not found");
            }
            return Ok(orders);
        }*/

        [HttpGet("get/{userId}"), Authorize(Roles = "User")]
        public async Task<ActionResult<List<GetOrderDto>>> GetOrdersByUserId(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var orders = await _context.Orders.Where(o => o.UserId == user.Id && o.IsCompleted == true).OrderByDescending(o => o.CreatedAt).ToListAsync();
            if (orders == null || orders.Count == 0)
            { 
                return BadRequest("Orders not found");
            }

            List<GetOrderDto> ordersDto = new List<GetOrderDto>();

            foreach (var order in orders)
            {
                var orderItems = await _context.OrdersItems.Where(i => i.OrderId == order.Id).ToListAsync();
                var products = new List<GetOrderProductDto>();

                foreach (var item in orderItems)
                {
                    var prod = await _context.Products.FindAsync(item.ProductId);
                    if (prod == null)
                    {
                        return BadRequest("Some item not found");
                    }
                    products.Add(new GetOrderProductDto(prod.Id, prod.Name, item.Quantity));
                }
                ordersDto.Add(new GetOrderDto(user.Id, order.Name, order.Total, order.CreatedAt, products));
            }

            return Ok(ordersDto);
        }

        [HttpPost("add")]
        public async Task<ActionResult> AddOrder(AddOrderDto request)
        {
            int? userId = null;
            
            if(request.userId != null)
            {
                userId = request.userId;
            }

            var order = new Order
            {
                UserId = userId,
                Total = request.Total,
                StripeSessionId = request.StripeSessionId,
                CreatedAt = DateTime.Now,
                IsCompleted = false
            };
            await _context.Orders.AddAsync(order);        
            await _context.SaveChangesAsync();

            foreach (var item in request.Items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.StripePriceId == item.Item1);
                if(product == null)
                {
                    return BadRequest("StripePriceId not found in database");
                }
 
                await _context.OrdersItems.AddAsync(
                    new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = item.Item2
                    }
                );
                await _context.SaveChangesAsync();
            }

            return Ok("Order added");
        }

        [HttpPut("confirm")]
        public async Task<ActionResult> ConfirmOrder(UpdateOrderDto request)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.StripeSessionId == request.StripeSessionId);
            if(order == null)
            {
                return BadRequest("Order not found");
            }
            order.CreatedAt = DateTime.Now;
            order.IsCompleted = true;
            order.Name = request.Name;
            await _context.SaveChangesAsync();

            // Add logic to change quantity of items after pruchase

            return Ok("Order confirmed");
        }
    }
}
