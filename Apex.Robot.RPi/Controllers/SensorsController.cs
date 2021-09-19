using Apex.Robot.RPi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Apex.Robot.RPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsController : ControllerBase
    {
        private readonly ISensorsService _service;

        public SensorsController(ISensorsService service)
        {
            _service = service;
        }

        [HttpGet("infrared")]
        public IActionResult Infrared()
        {
            var infrared = _service.ReadInfrared();

            return Ok(infrared);
        }

        [HttpGet("follow")]
        public IActionResult Follow()
        {
            _service.FollowLine();

            return Ok();
        }

        [HttpGet("distance")]
        public IActionResult Distance()
        {
            var distance = _service.ReadDistance();

            return Ok(distance);
        }

        [HttpGet("temperature")]
        public IActionResult Temperature()
        {
            var temperature = _service.ReadTemperature();

            return Ok(temperature);
        }

        [HttpGet("humidity")]
        public IActionResult Humidity()
        {
            var humidity = _service.ReadHumidity();

            return Ok(humidity);
        }
    }
}
