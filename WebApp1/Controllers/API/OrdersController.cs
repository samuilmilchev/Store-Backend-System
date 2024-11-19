using Business.Intefraces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using System.Security.Claims;

namespace WebApp1.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _orderService;

        public OrdersController(IOrdersService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Retrieves the details of an order by its unique ID.
        /// </summary>
        /// <param name="id">The ID of the order to search for.</param>
        /// <returns>Returns the order details matching the provided ID.</returns>
        /// <response code="200">Returns the order details if the order is found.</response>
        /// <response code="404">Not found if no order exists with the specified ID.</response>
        [HttpGet("getById")]
        public async Task<IActionResult> GetOrderById([FromQuery] int id)
        {
            var order = await _orderService.GetOrderById(id);

            return Ok(order);
        }

        /// <summary>
        /// Creates a new order for the authenticated user.
        /// </summary>
        /// <param name="orderData">The details of the order to be created.</param>
        /// <returns>Returns the created order along with its details.</returns>
        /// <response code="201">Returns the created order with its details.</response>
        /// <response code="400">Bad request if the provided order data is invalid.</response>
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderData)
        {
            var userIdClaim = User.Claims
           .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier && Guid.TryParse(claim.Value, out _));

            var userId = Guid.Parse(userIdClaim.Value);

            var order = await _orderService.CreateOrder(userId, orderData);

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }

        /// <summary>
        /// Retrieves the orders for the authenticated user, or a specific order if an order ID is provided.
        /// </summary>
        /// <param name="orderId">The ID of the specific order to retrieve (optional).</param>
        /// <returns>Returns a list of orders for the authenticated user, or a specific order if the order ID is provided.</returns>
        /// <response code="200">Returns the orders or the specific order if found.</response>
        /// <response code="404">Not found if no orders or the specified order exists for the user.</response>
        [HttpGet("getOrder")]
        public async Task<IActionResult> GetOrders([FromQuery] int orderId)
        {
            var userIdClaim = User.Claims
           .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier && Guid.TryParse(claim.Value, out _));

            var userId = Guid.Parse(userIdClaim.Value);

            var order = await _orderService.GetOrders(userId, orderId);

            return Ok(order);
        }

        /// <summary>
        /// Updates the items in an order for the authenticated user.
        /// </summary>
        /// <param name="updateDto">The data required to update the order items, including product IDs and the new quantities.</param>
        /// <returns>Returns the updated order items after the changes are applied.</returns>
        /// <response code="200">Returns the updated order items.</response>
        /// <response code="400">Bad request if the update data is invalid or if the order cannot be updated.</response>
        /// <response code="404">Not found if no order exists for the authenticated user or the specified order ID is incorrect.</response>
        [HttpPut("updateOrder")]
        public async Task<IActionResult> UpdateOrderItems([FromBody] UpdateOrderItemsDto updateDto)
        {
            var userIdClaim = User.Claims
          .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier && Guid.TryParse(claim.Value, out _));

            var userId = Guid.Parse(userIdClaim.Value);

            var updatedOrderItems = await _orderService.UpdateOrder(userId, updateDto);

            return Ok(updatedOrderItems);
        }

        /// <summary>
        /// Removes items from an order for the authenticated user.
        /// </summary>
        /// <param name="updateDto">The data specifying the items to be removed from the order.</param>
        /// <returns>Returns no content after successfully removing the items.</returns>
        /// <response code="204">No content if the order items are successfully removed.</response>
        /// <response code="400">Bad request if the removal data is invalid or if the items cannot be removed.</response>
        /// <response code="404">Not found if no order exists for the authenticated user or the specified order ID is incorrect.</response>
        [HttpDelete("deleteOrder")]
        public async Task<IActionResult> DeleteOrderItems([FromBody] RemoveOrderItemsDto updateDto)
        {
            var userIdClaim = User.Claims
          .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier && Guid.TryParse(claim.Value, out _));

            var userId = Guid.Parse(userIdClaim.Value);

            await _orderService.DeleteOrderItems(userId, updateDto);

            return NoContent();
        }

        /// <summary>
        /// Processes the purchase of items in the specified order for the authenticated user.
        /// </summary>
        /// <param name="orderId">The ID of the order to process for purchase.</param>
        /// <returns>Returns no content after successfully processing the purchase.</returns>
        /// <response code="204">No content if the purchase is successfully processed.</response>
        /// <response code="400">Bad request if the order ID is invalid or the user cannot purchase the items.</response>
        /// <response code="404">Not found if the order does not exist or the specified order ID is incorrect.</response>
        /// <response code="403">Forbidden if the user attempts to purchase items in a paid order or unauthorized access is detected.</response>
        [HttpPost("buy")]
        public async Task<IActionResult> BuyItems([FromQuery] int orderId)
        {
            var userIdClaim = User.Claims
          .FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier && Guid.TryParse(claim.Value, out _));

            var userId = Guid.Parse(userIdClaim.Value);

            await _orderService.BuyItems(userId, orderId);

            return NoContent();
        }
    }
}
