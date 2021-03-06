using System;
using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;
using System.Threading.Tasks;
using Serilog;

namespace Apex.Robot.RPi.Hubs
{
    public class CameraHub : Hub
    {
        private readonly ICameraService _cameraService;
        private static bool _isStreaming;
        private static uint _imageWidth;
        private static uint _imageHeight;
        private readonly ApiSettings _settings;

        public CameraHub(ICameraService cameraService, ApiSettings settings)
        {
            _imageWidth = settings.ImageWidth;
            _imageHeight = settings.ImageHeight;
            _cameraService = cameraService;
            _settings = settings;
        }

        public async Task StartCameraStreaming()
        {
            _isStreaming = true;
            await Clients.All.SendAsync("cameraStreamingStarted", "started...");
        }

        public async Task StopCameraStreaming()
        {
            _isStreaming = false;
            await Clients.All.SendAsync("cameraStreamingStopped");
        }

        public ChannelReader<Capture> CameraCaptureLoop()
        {
            var channel = Channel.CreateUnbounded<Capture>();
            _ = WriteToChannel(channel.Writer);
            return channel.Reader;

            async Task WriteToChannel(ChannelWriter<Capture> writer)
            {
                while (_isStreaming)
                {
                    byte[] image;
                    try
                    {
                        image = _cameraService.GetImage(_imageWidth, _imageHeight);
                        var capture = new Capture
                        {
                            Image = image,
                            CreatedAt = DateTime.Now.ToString("yyyyMMddhhmmssff")
                        };

                        await writer.WriteAsync(capture);
                        await Clients.All.SendAsync("cameraImageCaptured", capture.Image.Length);
                    }
                    catch (Exception)
                    {
                        await Clients.All.SendAsync("cameraImageNotCaptured");
                    }

                    await Task.Delay(_settings.CaptureDelay);
                }
            }
        }
    }
}
