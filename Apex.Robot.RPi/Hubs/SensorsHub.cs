using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Apex.Robot.RPi.Hubs
{
    public class SensorsHub : Hub
    {
        private readonly ISensorsService _sensorsService;
        private static bool _isStreaming;
        private readonly ApiSettings _settings;
        public static string IsAlarm { get; set; }

        public SensorsHub(ISensorsService sensorsService, ApiSettings settings)
        {
            _sensorsService = sensorsService;
            _settings = settings;
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

        public ChannelReader<Reading> SensorsCaptureLoop()
        {
            var channel = Channel.CreateUnbounded<Reading>();
            _ = WriteToChannel(channel.Writer);
            return channel.Reader;

            async Task WriteToChannel(ChannelWriter<Reading> writer)
            {
                while (_isStreaming)
                {
                    try
                    {
                        var humidity = _sensorsService.ReadHumidity();
                        //await Task.Delay(_settings.ReadingDelay);
                        Console.WriteLine($"humidity {humidity}");

                        var temperature = _sensorsService.ReadTemperature();
                        //await Task.Delay(_settings.ReadingDelay);
                        Console.WriteLine($"temperature {temperature}");

                        ////////var infrared = _sensorsService.ReadInfrared();
                        //await Task.Delay(_settings.ReadingDelay);
                        var infrared = 1;
                        Console.WriteLine($"infrared {infrared}");

                        var distance = _sensorsService.ReadDistance();
                        //await Task.Delay(_settings.ReadingDelay);
                        Console.WriteLine($"distance {distance}");

                        var createdAt = DateTime.Now.ToString("yyyyMMddhhmmssff");

                        var reading = new Reading
                        {
                            Humidity = humidity,
                            Temperature = temperature,
                            Infrared = infrared,
                            Distance = distance,
                            CreatedAt = createdAt,
                            IsAlarm = IsAlarm ?? string.Empty
                        };

                        await writer.WriteAsync(reading);
                        await Clients.All.SendAsync("sensorsDataCaptured", $"{humidity}, {temperature}, {infrared}, {distance}, {createdAt}, {IsAlarm}");

                        await Task.Delay(_settings.ReadingDelay);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        await Clients.All.SendAsync("sensorsDataNotCaptured");
                    }

                    await Task.Delay(_settings.ReadingDelay);
                }
            }
        }

        public void ChangeSource(string isAlarm)
        {
            IsAlarm = isAlarm.Trim();
        }
    }
}
