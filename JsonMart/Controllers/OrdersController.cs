using JsonMart.Dtos;
using JsonMart.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JsonMart.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;


    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrdersAsync(CancellationToken token)
    {
        var orders = await _orderService.GetAllOrdersAsync(token);

        if (orders == null || !orders.Any())
        {
            return NoContent();
        }

        return Ok(orders);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderByIdAsync([FromRoute] int id, CancellationToken token)
    {
        var order = await _orderService.GetOrderAsync(id, token);

        if (order == null)
        {
            return NotFound($"Order id {id} not found.");
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderCreateResponseDto>> CreateOrderAsync([FromBody] OrderCreateDto? orderCreateDto,
        CancellationToken token)
    {
        if (orderCreateDto == null || !orderCreateDto.ProductIds.Any())
        {
            return BadRequest("Invalid order data: Order details are missing or product list is empty.");
        }

        var createdOrderResponse = await _orderService.CreateOrderAsync(orderCreateDto, token);

        if (!createdOrderResponse.Result.Success)
        {
            var message = createdOrderResponse.Result.Message ?? "Order creation failed: Unable to process the order.";
            return BadRequest(new
            {
                Message = message, 
                createdOrderResponse.UnavailableProducts
            });
        }

        return Ok(createdOrderResponse);
    }

    [HttpPatch]
    [Route("{id}")]
    public async Task<ActionResult<OrderUpdateResponseDto>> UpdateOrderAsync([FromRoute] int id,
        [FromBody] OrderUpdateDto orderUpdateDto, CancellationToken token)
    {
        var updateResponse = await _orderService.UpdateOrderAsync(id, orderUpdateDto, token);

        if (updateResponse == null)
        {
            return NotFound($"Order with ID {id} not found.");
        }

        if (!updateResponse.Result.Success)
        {
            return BadRequest(updateResponse.Result.Message);
        }

        return Ok(updateResponse);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> DeleteOrderAsync(int id, CancellationToken token)
    {
        var isDeletionSuccessful = await _orderService.DeleteOrderAsync(id, token);

        return isDeletionSuccessful
            ? NoContent()
            : NotFound($"Order id {id} not found.");
    }
    
    
    [HttpPost("{id}/pay")]
    public async Task<ActionResult> PayOrder(int userId, int id, CancellationToken token)
    {
        var paymentResult = await _orderService.TryPayOrder(userId, id, token);

        if (!paymentResult.Success)
        {
            return BadRequest(paymentResult.Message);
        }

        return Ok("Order paid successfully.");
    }
}