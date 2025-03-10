using System;

namespace HMES.Data.DTO.RequestModel;

public class UserAddressReqModel
{
}
public class  UserAddressCreateReqModel
{
    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;
}
