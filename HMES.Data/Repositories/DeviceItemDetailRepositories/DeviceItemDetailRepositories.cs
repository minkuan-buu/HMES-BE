using System;
using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.DeviceItemDetailRepositories;

public class DeviceItemDetailRepositories : GenericRepositories<DeviceItemDetail>, IDeviceItemDetailRepositories
{
    public DeviceItemDetailRepositories(HmesContext context) : base(context)
    {

    }
}