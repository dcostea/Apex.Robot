using Apex.Robot.Vision.API.DeepLearning;
using Apex.Robot.Vision.API.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace Apex.Robot.Vision.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ModelSettings _modelSettings;

        public ImagesController(ModelSettings modelSettings)
        {
            _modelSettings = modelSettings;
            Log.Debug(_modelSettings.TagsFilePath);
            Log.Debug(_modelSettings.ModelPath);
            Log.Debug(_modelSettings.ModelFilePath);
            Log.Debug(_modelSettings.RetrainedModelFilePath);
        }

        [HttpGet("train_inception")]
        public IActionResult ReTrainInception()
        {
            Inception.Model = Inception.LoadAndScoreModel(_modelSettings.TagsFilePath, _modelSettings.ModelPath, _modelSettings.ModelFilePath, _modelSettings.RetrainedModelFilePath);
            Log.Debug("inception re-trained");

            return Ok("inception re-trained");
        }

        [HttpPost("predict_image")]
        public async Task<IActionResult> ImagePredictAsync()
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            string body = await reader.ReadToEndAsync();
            byte[] imageBytes = Convert.FromBase64String(body);
            string result = "";

            try
            {
                using var image = Image.FromStream(new MemoryStream(imageBytes));
                image.Save(_modelSettings.TestImageFilePath, ImageFormat.Jpeg);

                var imageData = new ImageNetData()
                {
                    ImagePath = _modelSettings.TestImageFilePath,
                    Label = Path.GetFileNameWithoutExtension(_modelSettings.TestImageFilePath)
                };

                Inception.Model ??= Inception.LoadModel(_modelSettings.RetrainedModelFilePath);

                var prediction = Inception.Model.Predict(imageData);
                Log.Debug($"prediction: {prediction.PredictedLabelValue}");
                result = prediction.PredictedLabelValue;
            }
            catch (Exception ex)
            {
                Log.Debug(ex.Message);
            }

            return Ok(result);
        }
    }
}
