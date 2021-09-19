using Apex.Robot.Vision.DeepLearning;
using Apex.Robot.Vision.Helpers;
using Apex.Robot.Vision.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Apex.Robot.Vision.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ApiSettings _settings;

        //TODO pull assets path to appsettings
        static readonly string assetsRelativePath = @"../../../assets";
        static readonly string assetsPath = PathHelper.GetAbsolutePath(assetsRelativePath);

        static readonly string tagsTsv = Path.Combine(assetsPath, "inputs", "train", "tags.tsv");
        static readonly string inceptionTrainImagesFolder = Path.Combine(assetsPath, "inputs", "train");
        static readonly string inceptionPb = Path.Combine(assetsPath, "inputs", "inception", "tensorflow_inception_graph.pb");
        static readonly string imageClassifierZip = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");

        static readonly string dataRelativePath = @"../../../data";
        static readonly string dataPath = PathHelper.GetAbsolutePath(dataRelativePath);

        public ImagesController(ApiSettings apiSettings)
        {
            _settings = apiSettings;
        }

        [HttpGet("train_inception")]
        public IActionResult ReTrainInception()
        {
            Inception.Model = Inception.LoadAndScoreModel(tagsTsv, inceptionTrainImagesFolder, inceptionPb, imageClassifierZip);
            Console.WriteLine("inception re-trained");

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
                var testImage = Path.Combine(inceptionTrainImagesFolder, "capture.jpg");

                using var image = Image.FromStream(new MemoryStream(imageBytes));
                image.Save(testImage, ImageFormat.Jpeg);

                var imageData = new ImageNetData()
                {
                    ImagePath = testImage,
                    Label = Path.GetFileNameWithoutExtension(testImage)
                };

                if (Inception.Model is null)
                {
                    Inception.Model = Inception.LoadModel(tagsTsv, inceptionTrainImagesFolder, inceptionPb, imageClassifierZip);
                }

                var prediction = Inception.Model.Predict(imageData);
                result = prediction.PredictedLabelValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(result);
        }
    }
}
