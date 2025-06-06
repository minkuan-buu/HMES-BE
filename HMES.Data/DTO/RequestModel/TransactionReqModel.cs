using HMES.Data.Enums;
using Microsoft.AspNetCore.Http;


namespace HMES.Data.DTO.RequestModel
{
    public class TransactionReqModel
    {
    }

    public class OrderUpdateAddress
    {
        public Guid OrderId { get; set; }
        public Guid UserAddressId { get; set; }
    }

    public class CreateOrderDetailReqModel
    {
        public List<OrderProductReqModel> Products { get; set; } = new();
        public List<OrderDeviceReqModel> Devices { get; set; } = new();
    }

    public class OrderProductReqModel
    {
        public Guid Id { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderDeviceReqModel
    {
        public Guid Id { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderConfirmReqModel
    {
        public Guid OrderId { get; set; }
        public OrderEnums Status { get; set; }
    }

    public class OrderDeliveryConfirmReqModel
    {
        public Guid OrderId { get; set; }
        public OrderEnums Status { get; set; }
    }
}