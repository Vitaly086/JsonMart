using JsonMart.Dtos;

namespace JsonMart.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>?> GetAllProductsAsync(CancellationToken token);
    Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken token);
    Task<ProductCreateResponseDto?> CreateProductAsync(ProductCreateDto productCreateDto, CancellationToken token);
    Task<ProductDto?> UpdateProductAsync(int id, ProductUpdateDto productUpdateDto, CancellationToken token);
    Task<bool> TryDeleteProductAsync(int id, CancellationToken token);
    Task<IEnumerable<ProductAvailabilityInfoDto>?> GetAvailableProductsAsync(CancellationToken token);
}