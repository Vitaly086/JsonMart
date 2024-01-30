using System.ComponentModel.DataAnnotations;

namespace JsonMart.Entities;

public class User
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Balance { get; set; }
    
    public virtual ICollection<Order> Orders { get; set; }
}