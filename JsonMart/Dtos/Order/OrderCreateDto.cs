namespace JsonMart.Dtos.Order;

public class OrderCreateDto
{
    public int UserId { get; set;}
    public List<int> ProductIds { get; set; }
}