using HMES.Data.DTO.RequestModel;

namespace HMES.Data.DTO.ResponseModel;

public class OrderResModel
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;

    public Guid UserAddressId { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class OrderDetailsResModel
{
    public Guid OrderId { get; set; }
    public decimal Price { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = null!;
    public List<OrderDetailsItemResModel> OrderDetailsItems { get; set; } = null!;
    public OrderAddressResModel? UserAddress { get; set; }
    public List<OrderTransactionResModel> Transactions { get; set; } = null!;
    
}

public class OrderDetailsItemResModel
{
    public Guid OrderDetailsId { get; set; }

    public string ProductName { get; set; } = null!;
    
    public string ProductImage { get; set; } = null!;
    
    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }
}

public class OrderAddressResModel
{
    public Guid AddressId { get; set; }

    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal? Longitude { get; set; }

    public decimal? Latitude { get; set; }
}

public class OrderTransactionResModel
{
    public Guid TransactionId { get; set; }
    
    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}