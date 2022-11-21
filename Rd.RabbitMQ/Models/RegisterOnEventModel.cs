namespace Rd.RabbitMQ.Models
{
    public class RegisterOnEventModel : RabbitMQMessageDto
    {
        public required string Email { get; set; }
    }
}
