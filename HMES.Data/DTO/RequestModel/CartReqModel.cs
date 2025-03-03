namespace HMES.Data.DTO.RequestModel;

public class CartReqModel
{
    
}

public class CartItemCreateDto
{
    public Guid ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
}

public class CartItemUpdateDto
{
    public Guid ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
}
