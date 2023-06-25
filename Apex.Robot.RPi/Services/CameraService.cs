using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Iot.Device.Media;
using Serilog;

namespace Apex.Robot.RPi.Services
{
    public class CameraService : ICameraService
    {
        private readonly ApiSettings _settings;

        public CameraService(ApiSettings settings)
        {
            _settings = settings;
        }
        
        public async Task<string> GetInceptionPrediction(string fileName)
        {
            var url = $"{_settings.VisionUrl}/api/images/predict_image";
            var settings = new VideoConnectionSettings(busId: 0, captureSize: (_settings.ImageWidth, _settings.ImageHeight), pixelFormat: PixelFormat.JPEG);
            var device = VideoDevice.Create(settings);
            var byteArray = device.Capture();

            ////HttpContent fileStreamContent = new StreamContent(stream);
            ////fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "file", FileName = fileName };
            ////fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            using var client = new HttpClient();
            using var content = new StringContent(Convert.ToBase64String(byteArray));
            var response = await client.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Log.Debug($"[PREDICT] Image: {result}");
            }

            return string.Empty;
        }

        public byte[] GetImage(uint width, uint height)
        {
            //https://github.com/dotnet/iot/blob/main/src/devices/Media/README.md
            var settings = new VideoConnectionSettings(busId: 0, captureSize: (width, height), pixelFormat: PixelFormat.JPEG);
            var device = VideoDevice.Create(settings);
            var byteArray = device.Capture();

            return byteArray;
        }
    }
}
