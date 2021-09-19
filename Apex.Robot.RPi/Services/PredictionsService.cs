using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Microsoft.ML;

namespace Apex.Robot.RPi.Services
{
    public class PredictionsService : IPredictionsService
    {
        private readonly MLContext mlContext;
        private readonly ApiSettings _settings;

        public PredictionsService(ApiSettings settings)
        {
            _settings = settings;

            mlContext = new MLContext(seed: 1);
        }

        public ModelOutput TrainAndPredict()
        {
            const string DATASET_PATH = @"sensors_data.csv";
            IDataView data = mlContext.Data.LoadFromTextFile<ModelInput>(
                path: DATASET_PATH,
                hasHeader: true,
                separatorChar: ',');

            var shuffledData = mlContext.Data.ShuffleRows(data, seed: 1);
            var split = mlContext.Data.TrainTestSplit(shuffledData, testFraction: 0.3);
            var trainingData = split.TrainSet;
            var testingData = split.TestSet;

            var featureColumns = new[] { "Temperature", "Humidity", "Infrared", "Distance" };

            var trainingPipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.Transforms.Concatenate("Features", featureColumns))
                .Append(mlContext.MulticlassClassification.Trainers.OneVersusAll(mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: @"Label", featureColumnName: "Features"), labelColumnName: @"Label"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = trainingPipeline.Fit(trainingData);

            var sampleData = new ModelInput
            {
                Temperature = 32F,
                Humidity = 22F,
                Infrared = 10F,
                Distance = 20F,
                CreatedAt = "01/03/2020 10:22:08"
            };

            var predictor = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            var predicted = predictor.Predict(sampleData);

            System.Console.WriteLine(predicted.PredictedLabel);

            return predicted;
        }

        public void Train()
        {
            const string DATASET_PATH = @"sensors_data.csv";
            IDataView data = mlContext.Data.LoadFromTextFile<ModelInput>(
                path: DATASET_PATH,
                hasHeader: true,
                separatorChar: ',');

            var shuffledData = mlContext.Data.ShuffleRows(data, seed: 1);
            var split = mlContext.Data.TrainTestSplit(shuffledData, testFraction: 0.3);
            var trainingData = split.TrainSet;
            var testingData = split.TestSet;

            var featureColumns = new[] { "Temperature", "Humidity", "Infrared", "Distance" };

            var trainingPipeline = mlContext.Transforms.Conversion.MapValueToKey("Label")
                .Append(mlContext.Transforms.Concatenate("Features", featureColumns))
                .Append(mlContext.MulticlassClassification.Trainers.OneVersusAll(mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: @"Label", featureColumnName: "Features"), labelColumnName: @"Label"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = trainingPipeline.Fit(trainingData);

            mlContext.Model.Save(model, trainingData.Schema, "model.zip");
        }

        public ModelOutput Predict()
        {
            var sampleData = new ModelInput
            {
                Temperature = 32F,
                Humidity = 22F,
                Infrared = 10F,
                Distance = 20F,
                CreatedAt = "01/03/2020 10:22:08"
            };

            var model = mlContext.Model.Load("model.zip", out var modelSchema);

            var predictor = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            var predicted = predictor.Predict(sampleData);

            System.Console.WriteLine(predicted.PredictedLabel);

            return predicted;
        }
    }
}
