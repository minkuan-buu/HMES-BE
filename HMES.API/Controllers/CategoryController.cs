using HMES.Business.Services.CategoryServices;
using HMES.Data.DTO.RequestModel;
using Microsoft.AspNetCore.Mvc;

namespace HMES.API.Controllers;


[Route("api/category")]
[ApiController]
public class CategoryController:ControllerBase
{
    private readonly ICategoryServices _categoryServices;

    public CategoryController(ICategoryServices categoryServices)
    {
        _categoryServices = categoryServices;
    }
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _categoryServices.GetCategories();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var result = await _categoryServices.GetCategoryById(id);
        return Ok(result);
    }
    [HttpGet("with-parents/{id}")]
    public async Task<IActionResult> GetCategoryWithParents(Guid id)
    {
        
        var result = await _categoryServices.GetCategoryWithParents(id);
        return Ok(result);
    }
    
    [HttpGet("roots")]
    public async Task<IActionResult> GetAllRootCategories()
    {
        var result = await _categoryServices.GetAllRootCategories();
        return Ok(result);
    }

    [HttpGet("{parentId}/children")]
    public async Task<IActionResult> GetChildCategories(Guid parentId)
    {
        var result = await _categoryServices.GetChildCategories(parentId);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromForm] CategoryCreateReqModel category)
    {
        var result = await _categoryServices.CreateCategory(category);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCategory([FromForm] CategoryUpdateReqModel category)
    {
        var result = await _categoryServices.UpdateCategory(category);
        return Ok(result);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _categoryServices.DeleteCategory(id);
        return Ok(result);
    }
    
    

}