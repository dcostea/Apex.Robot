using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Apex.Robot.RPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private static uint _imageWidth;
        private static uint _imageHeight;

        private readonly ICameraService _service;

        public CameraController(ApiSettings settings, ICameraService service)
        {
            _service = service;
            _imageWidth = settings.ImageWidth;
            _imageHeight = settings.ImageHeight;
        }

        [HttpGet("capture")]
        public IActionResult Capture()
        {
            var image = _service.GetImage(_imageWidth, _imageHeight);

            return Ok(image);
        }
    }
}
