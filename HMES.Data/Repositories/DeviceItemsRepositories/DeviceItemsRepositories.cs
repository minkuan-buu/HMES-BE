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
        return await Context.DeviceItems
            .Include(d => d.Order)
            .FirstOrDefaultAsync(x => x.Id == deviceItemId && x.UserId == userId);
    }

    public async Task<DeviceItem?> GetDeviceItemById(Guid id)
    {
        return await Context.DeviceItems.Include(d => d.DeviceItemDetails).FirstOrDefaultAsync(x => x.Id == id); 
    }

    public async Task<bool> CheckDeviceItemByPlantIdAndPhaseId(Guid plantId, Guid phaseId)
    {
        return await Context.DeviceItems
            .AnyAsync(x => x.PlantId == plantId && x.PhaseId == phaseId);
    }

    public async Task<bool> CheckDeviceItemByPhaseId(Guid phaseId)
    {
        return await Context.DeviceItems
            .AnyAsync(x => x.PhaseId == phaseId);
    }

    public async Task<List<DeviceItem>> GetOnlineDevicesAsync()
    {
        return await Context.DeviceItems.Where(x => x.IsOnline).ToListAsync();
    }
}
