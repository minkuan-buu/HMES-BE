using HMES.Business.Services.ProductServices;
using HMES.Data.DTO.RequestModel;
using HMES.Data.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers;

[Route("api/product")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductServices _productServices;

    public ProductController(IProductServices productServices)
    {
        _productServices = productServices;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts([FromQuery] ProductStatusEnums status, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _productServices.GetAllProducts(pageIndex, pageSize,status);
        return StatusCode(result.StatusCodes, result.Response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        var result = await _productServices.GetProductById(id);
        return StatusCode(result.StatusCodes, result.Response);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string? keyword,
        [FromQuery] Guid? categoryId,
        [FromQuery] int? minAmount,
        [FromQuery] int? maxAmount,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] ProductStatusEnums status,
        [FromQuery] DateTime? createdAfter,
        [FromQuery] DateTime? createdBefore,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _productServices.SearchProducts(keyword, categoryId, minAmount, maxAmount, minPrice, maxPrice, status, createdAfter, createdBefore, pageIndex, pageSize);
        return StatusCode(result.StatusCodes, result.Response);
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct([FromForm] ProductCreateDto productDto)
    {
        var result = await _productServices.AddProduct(productDto);
        return StatusCode(result.StatusCodes, result.Response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProduct([FromForm] ProductUpdateDto productDto)
    {
        var result = await _productServices.UpdateProduct(productDto);
        return StatusCode(result.StatusCodes, result.Response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var result = await _productServices.DeleteProduct(id);
        return StatusCode(result.StatusCodes, result.Response);
    }
}