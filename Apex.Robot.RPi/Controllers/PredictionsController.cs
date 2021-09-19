using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Apex.Robot.RPi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Apex.Robot.RPi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PredictionsController : ControllerBase
{
    private readonly IPredictionsService _service;

    public PredictionsController(IPredictionsService service)
    {
        _service = service;
    }

    [HttpGet("train")]
    public IActionResult Train()
    {
        _service.Train();

        return Ok("model trained");
    }

    [HttpPost("predict")]
    public IActionResult Predict(ModelInput reading)
    {
        var prediction = _service.Predict(reading);

        return Ok(prediction);
    }

    [HttpGet("trainandpredict")]
    public IActionResult TrainAndPredict()
    {
        var prediction = _service.TrainAndPredict();

        return Ok(prediction);
    }
}
