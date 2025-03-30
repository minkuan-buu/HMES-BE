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

public class DeviceStatusChecker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IMqttClient _mqttClient;

    public DeviceStatusChecker(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await ConnectMqtt();
        await base.StartAsync(cancellationToken);
    }

    private async Task ConnectMqtt()
    {
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883) // Hoặc IP MQTT Broker của bạn
            .Build();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            string topic = e.ApplicationMessage.Topic;
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            Console.WriteLine($"📩 Nhận từ {topic}: {payload}");

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
        };

        await _mqttClient.ConnectAsync(options, CancellationToken.None);
        await _mqttClient.SubscribeAsync("esp32/status");
        Console.WriteLine("✅ Đã kết nối MQTT và đang lắng nghe 'esp32/status'");
    }

    // Định nghĩa model để parse JSON
    public class DeviceStatusPayload
    {
        public string DeviceId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient != null)
        {
            await _mqttClient.DisconnectAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}
