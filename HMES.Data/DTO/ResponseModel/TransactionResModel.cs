using Microsoft.AspNetCore.Http;


namespace HMES.Data.DTO.RequestModel
{
    class TransactionResModel
    {
    }


    public class OrderCreateResModel
    {
        public Guid Id { get; set; }
    }

    public class OrderPaymentResModel
    {
        public Guid Id { get; set; }
        public List<OrderDetailResModel> OrderProductItem { get; set; } = null!;
        public OrderUserAddress? UserAddress { get; set; } = null!;
        public decimal OrderPrice { get; set; }
        public decimal ShippingPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string StatusPayment { get; set; } = null!;
    }

    public class OrderDetailResModel
    {
        public Guid Id { get; set; }
        public string Attachment { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string ProductItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderUserAddress
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;
        public bool IsDefault { get; set; }
    }
}