namespace HMES.Data.DTO.ResponseModel;

public class CartResModel
{
    
}


public class CartItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImage { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
}
public class CartResponseDto
{
    public Guid Id { get; set; }
    public int TotalItems { get; set; }
}