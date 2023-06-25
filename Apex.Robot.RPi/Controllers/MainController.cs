using Apex.Robot.RPi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Apex.Robot.RPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly ApiSettings _settings;

        public MainController(ApiSettings apiSettings)
        {
            _settings = apiSettings;
        }

        [HttpGet("settings")]
        public IActionResult GetSettings()
        {
            return Ok(_settings);
        }
    }
}
