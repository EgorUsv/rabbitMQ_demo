using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Rd.RabbitMQ.ExchangerManagers;
using Rd.RabbitMQ.Models;

namespace Rd.RabbitMQ.Workers
{
    public class RabbitMQWorker : IRabbitMQWorker
    {
        private readonly IConnection _connection;
        private Dictionary<Type, IModel> _channels;
        private readonly IRabbitMQSettings _rabbitMQSettings;
        private readonly ILogger _logger;

        public RabbitMQWorker(
            IConnection connection, 
            IRabbitMQSettings rabbitMQSettings,
            ILoggerFactory loggerFactory)
        {
            _rabbitMQSettings = rabbitMQSettings;
            _connection = connection;
            _channels = new Dictionary<Type, IModel>();
            _logger = loggerFactory.CreateLogger<RabbitMQWorker>();
        }

        public void SendMessage<T>(T data)
            where T : RabbitMQMessageDto
        {
            if (!_rabbitMQSettings.IsEnabled())
            {
                return;
            }

            if (!_channels.Any())
            {
                StartConsumers();
            }

            var settings = _rabbitMQSettings.GetExchangeInfo<T>();

            if (!_channels.TryGetValue(typeof(T), out IModel channel))
            {
                throw new Exception("Что-то с конфигом. Пора чинить");
            }

            channel?.BasicPublish(
                exchange: settings.ExchangeName,
                routingKey: settings.QueueName,
                basicProperties: null,
                body: GetBody(data));
        }

        public void StartConsumers()
        {
            var settings = _rabbitMQSettings.GetAll();

            //Тоже такой рофлан, но я уверен что можно что-то придумать без рефлексии
            foreach (var item in settings)
            {
                var type = Assembly.GetCallingAssembly().GetTypes().First(_ => _.Name == item.ModelName);
                var genericMethod = typeof(RabbitMQWorker)
                    .GetMethod(nameof(RabbitMQWorker.SetUpModel), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(type);
                var channel = genericMethod
                    .Invoke(this, new object[] { item.ExchangeName, item.QueueName }) as IModel;

                _channels.Add(type, channel);
            }
        }

        private IModel SetUpModel<T>(string exchangeName, string queueName)
            where T : RabbitMQMessageDto
        {
            var channel = _connection.CreateModel();
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += ReceiveMessage<T>;

            channel.ExchangeDeclare(exchangeName, type: ExchangeType.Direct);

            channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            channel.QueueBind(queueName, exchangeName, queueName);

            channel.BasicConsume(queueName, autoAck: false, consumer);

            return channel;
        }

        private void ReceiveMessage<T>(object? model, BasicDeliverEventArgs ea)
            where T : RabbitMQMessageDto
        {
            var body = ea.Body.ToArray();
            var stringMessage = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<T>(stringMessage);

            if (ea.Redelivered)
            {
                // здесь можно указать логику при повторном чтении. Вообще можно сделать отдельную очередь,
                // в которой будут сторится все сообщения, а все отсальные сделать временными, чтоб
                // после чтения чистить (Но это на подумать, хз как лучше). DeliveryTag - грубо говоря это
                // offset из кафки. Вообще это тема для channels для того, чтобы сохранять порядок отправки сообщений.
                _logger.LogWarning($"Redelivered message. Payload: {stringMessage}, Tag: {ea.DeliveryTag}");
                return;
            }

            /* Рекомендую вручную посылать подтверждения отпрвки сообщения, 
             * после чего они удаляются из очереди. Так мы гарантируем, что они точно отправились.
             * Для автоматического подтверждения нужно засетать параметр в BasicConsume autoAck = true.
             * 
                var consumer = model as EventingBasicConsumer;
                consumer.Model.BasicAck(ea.DeliveryTag, multiple: false);
            */

            /* Для того, чтобы пометить сообщение как непрочитанное, можно отправить на сервер отрицательный запрос.
             * При этом сообщение будет опмечено как не доставленное. Параметр requeue указывает, нужно ли при этом
             * класть сообщение обратно в очередь.
             * 
                var consumer = model as EventingBasicConsumer;
                consumer.Model.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            */
            
            _logger.LogInformation($"Received message. Payload: {stringMessage}, Tag: {ea.DeliveryTag}");
        }

        private static byte[]? GetBody<T>(T data)
        {
            var stringObject = JsonSerializer.Serialize(data);
            var body = Encoding.UTF8.GetBytes(stringObject);

            return body;
        }

        ~RabbitMQWorker()
        {
            foreach (var channel in _channels.Values)
            {
                channel?.Close();
            }

            _connection?.Close();
        }
    }
}
