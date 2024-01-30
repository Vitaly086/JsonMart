using JsonMart.Context;
using JsonMart.Dtos;
using JsonMart.Entities;
using JsonMart.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JsonMart.Services;

public class ProductService : IProductService
{
    private readonly JsonMartDbContext _dbContext;

    public ProductService(JsonMartDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<IEnumerable<ProductDto>?> GetAllProductsAsync(CancellationToken token)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Select(p => MapProductDto(p))
            .ToListAsync(token);

        return products;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken token)
    {
        var product = await _dbContext.Products.FindAsync(id, token);

        if (product == null)
        {
            return null;
        }

        return MapProductDto(product);
    }


    public async Task<ProductCreateResponseDto?> CreateProductAsync(ProductCreateDto productCreateDto,
        CancellationToken token)
    {
        var newProduct = new Product
        {
            Name = productCreateDto.Name,
            Price = productCreateDto.Price,
            Description = productCreateDto.Description,
            AvailableQuantity = productCreateDto.Quantity
        };


        await _dbContext.Products.AddAsync(newProduct, token);
        await _dbContext.SaveChangesAsync(token);

        return new ProductCreateResponseDto(newProduct.Id, newProduct.Name, newProduct.Price,
            newProduct.Description);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, ProductUpdateDto productUpdateDto,
        CancellationToken token)
    {
        var product = await _dbContext.Products.FindAsync(id, token);

        if (product == null)
        {
            return null;
        }

        product.Name = productUpdateDto.Name ?? product.Name;
        product.Description = productUpdateDto.Description ?? product.Description;
        product.AvailableQuantity = productUpdateDto.AvailableQuantity ?? product.AvailableQuantity;
        product.Price = productUpdateDto.Price ?? product.Price;

        await _dbContext.SaveChangesAsync(token);
        return MapProductDto(product);
    }

    public async Task<bool> TryDeleteProductAsync(int id, CancellationToken token)
    {
        var product = await _dbContext.Products.FindAsync(id, token);

        if (product == null)
        {
            return false;
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(token);
        return true;
    }

    public async Task<IEnumerable<ProductAvailabilityInfoDto>?> GetAvailableProductsAsync(CancellationToken token)
    {
        var availableProducts = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.AvailableQuantity > 0)
            .Select(p => new ProductAvailabilityInfoDto(p.Id, p.Name, p.AvailableQuantity))
            .ToListAsync(token);

        return availableProducts.Any() ? availableProducts : null;
    }

    private static ProductDto MapProductDto(Product product)
    {
        return new ProductDto(product.Id, product.Name, product.Price, product.Description,
            product.AvailableQuantity);
    }
}