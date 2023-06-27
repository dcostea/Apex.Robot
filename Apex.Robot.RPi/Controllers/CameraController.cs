using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Apex.Robot.RPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private readonly ApiSettings _settings;

        private readonly ICameraService _service;

        public CameraController(ApiSettings settings, ICameraService service)
        {
            _service = service;
            _settings = settings;
        }

        [HttpGet("capture")]
        public IActionResult Capture()
        {
            var image = _service.GetImage(_settings.ImageWidth, _settings.ImageHeight);
            
            return Ok(image);
        }
    }
}
