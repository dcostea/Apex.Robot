using Apex.Robot.RPi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Apex.Robot.RPi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainController : ControllerBase
    {
        private readonly ILogger<MainController> _logger;
        private readonly ApiSettings _settings;

        public MainController(ILogger<MainController> logger, ApiSettings apiSettings)
        {
            _logger = logger;
            _settings = apiSettings;
        }

        [HttpGet("settings")]
        public IActionResult GetSettings()
        {
            return Ok(_settings);
        }
    }
}
