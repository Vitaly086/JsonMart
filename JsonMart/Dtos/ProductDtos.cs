namespace JsonMart.Dtos;

public record ProductDto(int Id, string Name, decimal Price, string Description, int AvailableQuantity);
public record ProductCreateResponseDto(int Id, string Name, decimal Price, string Description);
public record ProductCreateDto(string Name, decimal Price, string Description, int Quantity);
public record ProductAvailabilityInfoDto(int ProductId, string? ProductName, int AvailableQuantity);

public record ProductUpdateDto(string? Name = null, decimal? Price = null, string? Description = null,
    int? AvailableQuantity = null)
{
    public bool AreAllPropertiesNull()
    {
        return Name == null && Description == null && AvailableQuantity == null && Price == null;
    }
};
