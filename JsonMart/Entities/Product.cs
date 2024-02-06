using System.ComponentModel.DataAnnotations;

namespace JsonMart.Entities;

public class Product
{
    [Key]
    public int Id { get; set; }
    [Required(ErrorMessage = "Product name is requerd")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Price is requied")]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    public string Description { get; set; }
    [Range(0,Int32.MaxValue)]
    public int AvailableQuantity { get; set; }
}