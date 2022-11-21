using Microsoft.Extensions.Configuration;

namespace Rd.RabbitMQ.ExchangerManagers
{
    public record RabbitMQSetting(string ModelName, string QueueName);

    public class RabbitMQSettings : IRabbitMQSettings
    {
        private readonly IConfiguration _configuration;

        private const string RabbitMQ = "RabbitMQ";
        private const string Sections = "Sections";
        private const string IsEnabled = "IsEnabled";
        private const string Mail = "rd.mail";

        public RabbitMQSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (string ExchangeName, string QueueName) GetExchangeInfo<T>()
        {
            var mailSections = _configuration
                .GetSection($"{RabbitMQ}:{Sections}:{Mail}")
                .Get<List<RabbitMQSetting>>() ?? new List<RabbitMQSetting>();

            var typeName = typeof(T).Name;

            foreach (var item in mailSections)
            {
                if (item.ModelName == typeName)
                {
                    return (Mail, item.QueueName);
                }
            }

            return (Mail, string.Empty);
        }


        public (string ExchangeName, string QueueName, string ModelName)[] GetAll()
        {
            var mailSections = _configuration
                .GetSection($"{RabbitMQ}:{Sections}:{Mail}")
                .Get<List<RabbitMQSetting>>() ?? new List<RabbitMQSetting>();

            return mailSections.Select(_ => (Mail, _.QueueName, _.ModelName)).ToArray();
        }

        bool IRabbitMQSettings.IsEnabled()
        {
            var section = _configuration.GetSection($"{RabbitMQ}:{IsEnabled}");

            if (bool.TryParse(section.Value, out bool result))
            {
                return result;
            }

            return result;
        }
    }
}
