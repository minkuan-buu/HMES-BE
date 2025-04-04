using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HMES.Data.Repositories.DeviceItemsRepositories;
using HMES.Data.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DeviceStatusChecker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMqttService _mqttService;

    public DeviceStatusChecker(IServiceScopeFactory serviceScopeFactory, IMqttService mqttService)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _mqttService = mqttService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _mqttService.SubscribeAsync("esp32/status", HandleIncomingMessage);
        await base.StartAsync(cancellationToken);
    }

    private async Task HandleIncomingMessage(string payload)
    {
        try
        {
            var data = JsonSerializer.Deserialize<DeviceStatusPayload>(payload);
            if (data != null)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var deviceItemsRepository = scope.ServiceProvider.GetRequiredService<IDeviceItemsRepositories>();
                    var device = await deviceItemsRepository.GetSingle(x => x.Id.Equals(Guid.Parse(data.DeviceId)));

                    if (device != null)
                    {
                        device.LastSeen = DateTime.Now;
                        device.IsOnline = data.Status == "online";
                        await deviceItemsRepository.Update(device);
                        Console.WriteLine($"✅ Cập nhật {device.DeviceId} thành {data.Status}!");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Không tìm thấy thiết bị {data.DeviceId}!");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Lỗi khi xử lý JSON: {ex.Message}");
        }
    }

    public class DeviceStatusPayload
    {
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var deviceItemsRepository = scope.ServiceProvider.GetRequiredService<IDeviceItemsRepositories>();
                var onlineDevices = await deviceItemsRepository.GetOnlineDevicesAsync();
                List<DeviceItem> offlineDevices = new List<DeviceItem>();

                foreach (var device in onlineDevices)
                {
                    if (device.LastSeen < DateTime.Now.AddMinutes(-1))
                    {
                        device.IsOnline = false;
                        offlineDevices.Add(device);
                    }
                }
                await deviceItemsRepository.UpdateRange(offlineDevices);
            }

            await Task.Delay(60000, stoppingToken);
        }
    }
}
