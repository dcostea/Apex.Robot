using Microsoft.AspNetCore.Mvc;
using Apex.Robot.RPi.Interfaces;

namespace Apex.Robot.RPi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MotorsController : ControllerBase
{
    private readonly IMotorsService _service;

    public MotorsController(IMotorsService service)
    {
        _service = service;
    }

    [HttpGet("check")]
    public IActionResult CheckMotors()
    {
        _service.CheckMotors();

        return Ok();
    }

    [HttpGet("forward")]
    public IActionResult Forward()
    {
        _service.Forward(500);

        return Ok();
    }

    [HttpGet("backward")]
    public IActionResult Backward()
    {
        _service.Backward(500);

        return Ok();
    }

    [HttpGet("right")]
    public IActionResult Right()
    {
        _service.TurnRight(300);

        return Ok();
    }

    [HttpGet("left")]
    public IActionResult Left()
    {
        _service.TurnLeft(300);

        return Ok();
    }
}
