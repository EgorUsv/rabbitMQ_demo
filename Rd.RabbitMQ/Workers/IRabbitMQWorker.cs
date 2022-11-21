using Rd.RabbitMQ.Models;

namespace Rd.RabbitMQ.Workers
{
    public interface IRabbitMQWorker
    {
        void SendMessage<T>(T data) where T : RabbitMQMessageDto;

        void StartConsumers();
    }
}
