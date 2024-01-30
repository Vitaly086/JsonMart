using JsonMart.Dtos;
using JsonMart.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JsonMart.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderProductDto>>> GetAllProductsAsync(CancellationToken token)
    {
        var products = await _productService.GetAllProductsAsync(token);

        if (products == null || !products.Any())
        {
            return NoContent();
        }

        return Ok(products);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<OrderProductDto>> GetProductByIdAsync([FromRoute] int id, CancellationToken token)
    {
        var product = await _productService.GetProductByIdAsync(id, token);

        if (product == null)
        {
            return NotFound($"Product with id {id} not found.");
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateProductAsync([FromBody] ProductCreateDto productCreateDto,
        CancellationToken token)
    {
        var productDto = await _productService.CreateProductAsync(productCreateDto, token);

        if (productDto == null)
        {
            return BadRequest("Product creation failed.");
        }

        return Ok(productDto);
    }

    [HttpPatch]
    [Route("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProductByIdAsync([FromRoute] int id,
        [FromBody] ProductUpdateDto productUpdateDto, CancellationToken token)
    {
        if (productUpdateDto.AreAllPropertiesNull())
        {
            return BadRequest("No fields to update.");
        }

        var updatedProduct = await _productService.UpdateProductAsync(id, productUpdateDto, token);

        if (updatedProduct == null)
        {
            return NotFound($"Product with id {id} not found.");
        }

        return Ok(updatedProduct); 
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> DeleteProductAsync([FromRoute] int id, CancellationToken token)
    {
        var isDeletionSuccessful = await _productService.TryDeleteProductAsync(id, token);

        return isDeletionSuccessful
            ? NoContent()
            : NotFound($"Product with id {id} not found.");
    }

    [HttpGet]
    [Route("available")]
    public async Task<ActionResult<IEnumerable<OrderProductDto>>> GetAvailableProductsAsync(CancellationToken token)
    {
        var availableProducts = await _productService.GetAvailableProductsAsync(token);

        if (availableProducts == null)
        {
            return NoContent();
        }

        return Ok(availableProducts);
    }
}