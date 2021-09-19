using Microsoft.ML.Data;

namespace Apex.Robot.Vision.API.Models
{
    public class ImageNetData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }
}
