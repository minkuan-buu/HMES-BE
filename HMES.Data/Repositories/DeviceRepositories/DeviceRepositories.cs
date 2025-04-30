using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.DeviceRepositories
{
    public class DeviceRepositories : GenericRepositories<Device>, IDeviceRepositories
    {
        public DeviceRepositories(HmesContext context) : base(context)
        {
        }

        public async Task<List<Device>> GetListInRange(List<Guid?> ids)
        {
            return await Context.Devices.Where(p => ids.Contains(p.Id)).ToListAsync();
        }
    }
}
