using Microsoft.AspNetCore.Http;


namespace HMES.Data.DTO.RequestModel
{
    class TransactionReqModel
    {
    }


    public class OrderDetailCreateReqModel
    {
        public string Name { get; set; } = null!;
        public IFormFile? Attachment { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}