using System.ComponentModel.DataAnnotations;

namespace JsonMart.Entities;

public class UserEntity
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Balance { get; set; }
    public virtual ICollection<OrderEntity> Orders { get; set; }
}