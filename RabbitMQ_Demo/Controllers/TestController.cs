using Microsoft.AspNetCore.Mvc;
using Rd.RabbitMQ.Models;
using Rd.RabbitMQ.Workers;

namespace RabbitMQ_Demo.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly IRabbitMQWorker _rabbitMQWorker;
        public TestController(IRabbitMQWorker rabbitMQProducer)
        {
            _rabbitMQWorker = rabbitMQProducer;
        }

        [HttpPost("send-event")]
        public object Send1(RegisterOnEventModel message)
        {
            try
            {
                _rabbitMQWorker.SendMessage(message);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost("send-training")]
        public object Send2(RegisterOnTrainingModel message)
        {
            try
            {
                _rabbitMQWorker.SendMessage(message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}