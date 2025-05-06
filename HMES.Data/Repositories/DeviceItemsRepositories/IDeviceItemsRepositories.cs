using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.DeviceItemsRepositories;

public interface IDeviceItemsRepositories : IGenericRepositories<DeviceItem>
{
    Task<List<DeviceItem>> GetOnlineDevicesAsync();
    Task<DeviceItem?> GetDeviceItemByDeviceItemIdAndUserId(Guid deviceItemId, Guid userId);
    Task<DeviceItem?> GetDeviceItemById(Guid id);
}