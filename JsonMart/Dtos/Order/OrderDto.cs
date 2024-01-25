namespace JsonMart.Dtos.Order;

public class OrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string? CustomerName { get; set; }
    public List<OrderProductDto> Products { get; set; }
    public decimal TotalPrice { get; set; }
}