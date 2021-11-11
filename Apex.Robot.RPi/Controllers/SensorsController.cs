using Apex.Robot.RPi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Apex.Robot.RPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorsController : ControllerBase
    {
        private readonly ISensorsService _sensorsService;

        public SensorsController(ISensorsService service)
        {
            _sensorsService = service;
        }

        [HttpGet("infrared")]
        public IActionResult Infrared()
        {
            var infrared = _sensorsService.ReadInfrared();

            return Ok(infrared);
        }

        [HttpGet("follow")]
        public IActionResult Follow()
        {
            _sensorsService.FollowLine();

            return Ok();
        }

        [HttpGet("distance")]
        public IActionResult Distance()
        {
            var distance = _sensorsService.ReadDistance();

            return Ok(distance);
        }

        [HttpGet("temperature")]
        public IActionResult Temperature()
        {
            var temperature = _sensorsService.ReadTemperature();

            return Ok(temperature);
        }

        [HttpGet("humidity")]
        public IActionResult Humidity()
        {
            var humidity = _sensorsService.ReadHumidity();

            return Ok(humidity);
        }

        [HttpGet("luminosity")]
        public IActionResult Luminosity()
        {
            var luminosity = _sensorsService.ReadLuminosity();

            return Ok(luminosity);
        }
    }
}
