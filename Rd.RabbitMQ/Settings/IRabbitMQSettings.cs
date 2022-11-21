namespace Rd.RabbitMQ.ExchangerManagers
{
    public interface IRabbitMQSettings
    {
        (string ExchangeName, string QueueName) GetExchangeInfo<T>();

        (string ExchangeName, string QueueName, string ModelName)[] GetAll();

        bool IsEnabled();
    }
}
