using System.ComponentModel.DataAnnotations;

namespace JsonMart.Entities;

public class Order
{
    [Key]
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    
    public int UserId { get; set; }
    public virtual User User { get; set; }
    public virtual ICollection<OrderProduct> OrderProducts { get; set; }
}