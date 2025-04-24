using MQTTnet;
using MQTTnet.Client;
using System.Text;
using System.Text.Json;
public class MqttService : IMqttService
{
    private readonly IMqttClient _mqttClient;

    public MqttService()
    {
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("14.225.210.123", 1883) // IP MQTT Broker
            .Build();

        _mqttClient.ConnectAsync(options, CancellationToken.None).Wait();
    }

    public async Task PublishAsync(string topic, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(json)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        if (_mqttClient.IsConnected)
        {
            await _mqttClient.PublishAsync(message);
        }
    }

    public async Task SubscribeAsync(string topic, Func<string, Task> onMessageReceived)
    {
        await _mqttClient.SubscribeAsync(topic);

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            await onMessageReceived(payload);
        };
    }
}