using JsonMart.Dtos;
using JsonMart.Entities;

namespace JsonMart.Extensions;

public static class OrderEntityExtensions
{
    public static OrderDto ToDto(this OrderEntity orderEntity)
    {
        return new OrderDto(
            orderEntity.Id,
            orderEntity.OrderDate,
            orderEntity.CustomerName,
            orderEntity.OrderProducts.Select(op => new OrderProductDto(
                op.ProductId,
                op.Product.Name,
                op.Product.Price,
                op.Product.Description,
                op.ProductQuantity
            )).ToList(),
            orderEntity.Status,
            orderEntity.GetTotalOrderPrice()
        );
    }

    public static decimal GetTotalOrderPrice(this OrderEntity orderEntity)
    {
        return orderEntity.OrderProducts.Sum(op => op.Product.Price * op.ProductQuantity);
    }
}