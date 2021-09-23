using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Apex.Robot.RPi.Hubs
{
    public class SensorsHub : Hub
    {
        private readonly ISensorsService _sensorsService;
        private readonly IPredictionsService _predictionsService;
        private readonly IMotorsService _motorsService;
        private readonly ICameraService _cameraService;
        private DateTime _sleepTo;
        private static bool _isStreaming;
        private readonly ApiSettings _settings;
        public static string IsAlarm { get; set; }

        public SensorsHub(
            ISensorsService sensorsService, 
            IPredictionsService predictionsService, 
            IMotorsService motorsService, 
            ICameraService cameraService,
            ApiSettings settings)
        {
            _motorsService = motorsService;
            _sensorsService = sensorsService;
            _predictionsService = predictionsService;
            _cameraService = cameraService;
            _settings = settings;
            _sleepTo = DateTime.Now;
        }

        public async Task StartSensorsStreaming()
        {
            _isStreaming = true;
            await Clients.All.SendAsync("sensorsStreamingStarted", "started...");
        }

        public async Task StopSensorsStreaming()
        {
            _isStreaming = false;
            await Clients.All.SendAsync("sensorsStreamingStopped");
        }

        public ChannelReader<ModelInput> SensorsCaptureLoop()
        {
            var channel = Channel.CreateUnbounded<ModelInput>();
            _ = WriteToChannel(channel.Writer);
            return channel.Reader;

            async Task WriteToChannel(ChannelWriter<ModelInput> writer)
            {
                while (_isStreaming)
                {
                    try
                    {
                        var humidity = _sensorsService.ReadHumidity();
                        var temperature = _sensorsService.ReadTemperature();
                        var infrared = _sensorsService.ReadInfrared();
                        var distance = _sensorsService.ReadDistance();
                        var createdAt = DateTime.Now.ToString("yyyyMMddhhmmssff");

                        var reading = new ModelInput
                        {
                            Humidity = (float)humidity,
                            Temperature = (float)temperature,
                            Infrared = (float)infrared,
                            Distance = (float)distance,
                            CreatedAt = createdAt
                        };

                        var prediction = _predictionsService.Predict(reading);
                        reading.IsAlarm = prediction.Prediction;

                        var inceptionPrediction = await _cameraService.GetInceptionPrediction("capture.jpg");

                        if (reading.IsAlarm && inceptionPrediction.Equals("Lighter")) 
                        {
                            _motorsService.RunAway(_sleepTo);
                            _sleepTo = DateTime.Now.AddSeconds(5);
                        }

                        await writer.WriteAsync(reading);
                        await Clients.All.SendAsync("sensorsDataCaptured", $"{humidity}, {temperature}, {infrared}, {distance}, {createdAt}, {IsAlarm}");

                        await Task.Delay(_settings.ReadingDelay);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.Message);
                        await Clients.All.SendAsync("sensorsDataNotCaptured");
                    }

                    await Task.Delay(_settings.ReadingDelay);
                }
            }
        }
    }
}
