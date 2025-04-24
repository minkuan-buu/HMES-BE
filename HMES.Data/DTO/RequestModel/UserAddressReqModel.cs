using System;

namespace HMES.Data.DTO.RequestModel;

public class UserAddressReqModel
{
}
public class UserAddressCreateReqModel
{
    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Ward { get; set; } = null!;

    public string Province { get; set; } = null!;

    public string District { get; set; } = null!;

    public Guid? orderId { get; set; }

    public bool IsDefault { get; set; } = false;
}

public class UserAddressUpdateReqModel
{
    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Ward { get; set; } = null!;

    public string District { get; set; } = null!;

    public string Province { get; set; } = null!;
}
