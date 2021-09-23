using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Iot.Device.Media;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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
            var stream = device.Capture();

            HttpContent fileStreamContent = new StreamContent(stream);
            fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "file", FileName = fileName };
            fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            using (var client = new HttpClient())
            using (var content = new StringContent(Convert.ToBase64String(StreamToByteArray(stream))))
            {
                var response = await client.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                Log.Debug($"[PREDICT] Image: {result}");
                return result;
            }
        }

        public byte[] GetImage(uint width, uint height)
        {
            //https://github.com/dotnet/iot/blob/main/src/devices/Media/README.md
            var settings = new VideoConnectionSettings(busId: 0, captureSize: (width, height), pixelFormat: PixelFormat.JPEG);
            var device = VideoDevice.Create(settings);
            var stream = device.Capture();
            var byteArray = StreamToByteArray(stream);

            return byteArray;
        }

        private byte[] StreamToByteArray(Stream input)
        {
            var ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
