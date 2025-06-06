﻿using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMES.Data.Repositories.DeviceRepositories
{
    public interface IDeviceRepositories : IGenericRepositories<Device>
    {
        

        Task<List<Device>> GetListInRange(List<Guid?> ids);
    }
}
