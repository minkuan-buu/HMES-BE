using HMES.Data.Entities;
using HMES.Data.Repositories.GenericRepositories;

namespace HMES.Data.Repositories.DeviceItemsRepositories;

public interface IDeviceItemsRepositories : IGenericRepositories<DeviceItem>
{
    Task<DeviceItem?> GetDeviceItemByDeviceItemIdAndUserId(Guid deviceItemId, Guid userId);
}