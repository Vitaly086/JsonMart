using JsonMart.Dtos;
using JsonMart.Entities;

namespace JsonMart.Extensions;

public static class OrderExtensions
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto(
            order.Id,
            order.OrderDate,
            order.UserId,
            order.OrderProducts.ToOrderProductDtos(),
            order.Status,
            order.GetTotalOrderPrice()
        );
    }
    
    public static OrderCreateResponseDto ToCreateResponseDto(this Order order, OperationResult operationResult)
    {
        return new OrderCreateResponseDto(
            operationResult,
            order.Id,
            order.OrderDate,
            order.UserId,
            order.OrderProducts.ToOrderProductDtos(),
            order.Status,
            order.GetTotalOrderPrice()
        );
    }
    
    public static OrderUpdateResponseDto? ToUpdateResponseDto(this Order order, OperationResult operationResult)
    {
        return new OrderUpdateResponseDto(
            operationResult, new OrderDto(
            order.Id,
            order.OrderDate,
            order.UserId,
            order.OrderProducts.ToOrderProductDtos(),
            order.Status,
            order.GetTotalOrderPrice()
        ));
    }
    
    public static List<OrderProductDto> ToOrderProductDtos(this IEnumerable<OrderProduct> orderProducts)
    {
        return orderProducts.Select(op => new OrderProductDto(
            op.ProductId,
            op.Product.Name,
            op.Product.Price,
            op.Product.Description,
            op.ProductQuantity
        )).ToList();
    }

    public static decimal GetTotalOrderPrice(this Order order)
    {
        return order.OrderProducts.Sum(op => op.Product.Price * op.ProductQuantity);
    }
}