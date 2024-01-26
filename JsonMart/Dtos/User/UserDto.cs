using JsonMart.Entities;

namespace JsonMart.Dtos.User;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<OrderEntity> Orders { get; set; }
}