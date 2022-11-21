using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Rd.RabbitMQ.ExchangerManagers;
using Rd.RabbitMQ.Workers;

namespace Rd.RabbitMQ.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static void AddRabbitMQ(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IRabbitMQWorker>(_ =>
            {
                //Сделал чисто чтобы дернуть service.StartConsumers(). Вообще надо че-нить придумать
                var connection = _.GetService<IConnection>() ?? throw new Exception("Произошло чудо");
                var settings = _.GetService<IRabbitMQSettings>() ?? throw new Exception("Произошло чудо");
                var loggerFactory = _.GetService<ILoggerFactory>() ?? throw new Exception("Произошло чудо");
                var service = new RabbitMQWorker(connection, settings, loggerFactory);
                service.StartConsumers();
                return service;
            });
            serviceCollection.AddSingleton<IRabbitMQSettings, RabbitMQSettings>();
            serviceCollection.AddSingleton(_ =>
            {
                // Здесь сетпаится TCP коннекшен к базе. Тут можно настроить авторизацию и прочую лабуду для
                // удаленного подключения. Для локал хоста достаточно указать урл.
                var hostName = _.GetService<IConfiguration>()?.GetSection("RabbitMQ:UrlName").Value;
                var connectionFactory = new ConnectionFactory() { HostName = hostName };
                return connectionFactory.CreateConnection();
            });
        }
    }
}