using Microsoft.AspNetCore.Http;


namespace HMES.Data.DTO.RequestModel
{
    public class TransactionReqModel
    {
    }


    public class OrderDetailCreateReqModel
    {
        public string Name { get; set; } = null!;
        public IFormFile? Attachment { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateOrderDetailReqModel
    {
        public List<OrderProductReqModel> Products { get; set; } = new();
        public int DeviceQuantity { get; set; }

    }
     
    public class OrderProductReqModel
    {
        public Guid Id { get; set; } 
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}