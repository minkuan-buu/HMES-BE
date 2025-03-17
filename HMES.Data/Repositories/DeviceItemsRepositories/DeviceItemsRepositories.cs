using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;
using Microsoft.EntityFrameworkCore;

namespace HMES.Data.Repositories.DeviceItemsRepositories;

public class DeviceItemsRepositories : GenericRepositories<DeviceItem>, IDeviceItemsRepositories
{
    public DeviceItemsRepositories(HmesContext context) : base(context)
    {
        
    }

    public async Task<DeviceItem?> GetDeviceItemByDeviceItemIdAndUserId(Guid deviceItemId, Guid userId)
    {
        return await Context.DeviceItems.FirstOrDefaultAsync(x => x.Id == deviceItemId && x.UserId == userId);
    }
}
