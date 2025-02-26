namespace HMES.Data.DTO.ResponseModel;

public class ProductResModel
{
    
}

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
