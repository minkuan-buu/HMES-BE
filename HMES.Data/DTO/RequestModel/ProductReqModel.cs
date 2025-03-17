using HMES.Data.Enums;
using Microsoft.AspNetCore.Http;

namespace HMES.Data.DTO.RequestModel;

public class ProductReqModel
{
    
}

public class ProductCreateDto
{
    public string Name { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string Description { get; set; } = null!;
    public IFormFile MainImage { get; set; } = null!;
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public ProductStatusEnums Status { get; set; } = ProductStatusEnums.Active;
    public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    
}

public class ProductUpdateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public IFormFile? MainImage { get; set; }
    public string Description { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public ProductStatusEnums Status { get; set; } = ProductStatusEnums.Active;
    public List<string> OldImages { get; set; } = new List<string>();
    public List<IFormFile> NewImages { get; set; } = new List<IFormFile>();
    
}