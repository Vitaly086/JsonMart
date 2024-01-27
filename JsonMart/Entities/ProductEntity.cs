using System.ComponentModel.DataAnnotations;

namespace JsonMart.Entities;

public class ProductEntity
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public int AvailableQuantity { get; set; }
    public virtual ICollection<OrderProduct> OrderProducts { get; set; }
}