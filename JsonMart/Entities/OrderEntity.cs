using System.ComponentModel.DataAnnotations;

namespace JsonMart.Entities;

public class OrderEntity
{
    [Key]
    public int Id { get; set; }

    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; }
    public OrderStatus Status { get; set; }
    public int UserId { get; set; }
    public virtual UserEntity User { get; set; }
    public virtual ICollection<OrderProduct> OrderProducts { get; set; }
}

public enum OrderStatus
{
    Pending,
    Paid
}