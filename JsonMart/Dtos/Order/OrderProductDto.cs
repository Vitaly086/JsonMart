namespace JsonMart.Dtos.Order;

public class OrderProductDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public string? Description { get; set; }
}