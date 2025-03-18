namespace HMES.Data.DTO.ResponseModel;

public class ProductResModel
{
    
}


public class ProductBriefResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? MainImage { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public decimal Price { get; set; }
    public string Status { get; set; } = null!;
    
}

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? MainImage { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public List<string> Images { get; set; } = [];
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
}
