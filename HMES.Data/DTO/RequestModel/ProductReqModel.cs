namespace HMES.Data.DTO.RequestModel;

public class ProductReqModel
{
    
}

public class ProductCreateDto
{
    public string Name { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = "Active";
}

public class ProductUpdateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = null!;
}