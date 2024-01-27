using JsonMart.Dtos;
using JsonMart.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JsonMart.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrdersControllerV1 : ControllerBase
{
    private readonly IOrderService _orderService;


    public OrdersControllerV1(IOrderService orderService)
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

        if (createdOrderResponse == null)
        {
            return BadRequest("Order creation failed: Unable to process the order.");
        }
        
        if (!createdOrderResponse.IsOrderCreated)
        {
            return BadRequest(new 
            {
                Message = "Order creation failed: Some products are not available in the requested quantity.",
                createdOrderResponse.UnavailableProducts
            });
        }

        return Ok(createdOrderResponse);
    }

    [HttpPatch]
    [Route("{id}")]
    public async Task<ActionResult<OrderDto>> UpdateOrderAsync([FromRoute] int id,
        [FromBody] OrderUpdateDto orderUpdateDto, CancellationToken token)
    {
        var isUpdateSuccessful = await _orderService.TryUpdateOrderAsync(id, orderUpdateDto, token);

        return isUpdateSuccessful
            ? CreatedAtAction("GetOrderById", new { id })
            : BadRequest("Order update failed.");
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
}