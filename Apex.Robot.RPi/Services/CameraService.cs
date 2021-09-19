using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Iot.Device.Media;
using Serilog;
using System.IO;

namespace Apex.Robot.RPi.Services
{
    public class CameraService : ICameraService
    {
        private readonly ApiSettings _settings;

        public CameraService(ApiSettings settings)
        {
            _settings = settings;
        }

        public byte[] GetImage(uint width, uint height)
        {
            //https://github.com/dotnet/iot/blob/main/src/devices/Media/README.md
            var settings = new VideoConnectionSettings(busId: 0, captureSize: (width, height), pixelFormat: PixelFormat.JPEG);

            var device = VideoDevice.Create(settings);
            var stream = device.Capture();

            Log.Debug("Image captured into capture.jpg");

            return StreamToByteArray(stream);
        }

        private byte[] StreamToByteArray(Stream input)
        {
            var ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
