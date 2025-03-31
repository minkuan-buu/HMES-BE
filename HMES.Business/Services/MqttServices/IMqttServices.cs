public interface IMqttService
{
    Task PublishAsync(string topic, object payload);
    Task SubscribeAsync(string topic, Func<string, Task> onMessageReceived);
}