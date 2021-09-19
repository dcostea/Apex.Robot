using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;

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

            //TODO use featureColumns or not
            var featureColumns = new[] { "Temperature", "Humidity", "Infrared", "Distance" };

            var trainingPipeline = mlContext.Transforms.ReplaceMissingValues(new[] { new InputOutputColumnPair(@"Temperature", @"Temperature"), new InputOutputColumnPair(@"Humidity", @"Humidity"), new InputOutputColumnPair(@"Infrared", @"Infrared"), new InputOutputColumnPair(@"Distance", @"Distance") })
                .Append(mlContext.Transforms.Text.FeaturizeText(@"CreatedAt", @"CreatedAt"))
                .Append(mlContext.Transforms.Concatenate(@"Features", new[] { @"Temperature", @"Humidity", @"Infrared", @"Distance", @"CreatedAt" }))
                .Append(mlContext.BinaryClassification.Trainers.FastTree(new FastTreeBinaryTrainer.Options() { NumberOfLeaves = 5, MinimumExampleCountPerLeaf = 21, NumberOfTrees = 4, MaximumBinCountPerFeature = 248, LearningRate = 0.0327903143558005F, FeatureFraction = 0.913244540244145F, LabelColumnName = @"IsAlarm", FeatureColumnName = @"Features" }));

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

            System.Console.WriteLine(predicted.Prediction);

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

            //TODO use featureColumns or not
            var featureColumns = new[] { "Temperature", "Humidity", "Infrared", "Distance" };

            var trainingPipeline = mlContext.Transforms.ReplaceMissingValues(new[] { new InputOutputColumnPair(@"Temperature", @"Temperature"), new InputOutputColumnPair(@"Humidity", @"Humidity"), new InputOutputColumnPair(@"Infrared", @"Infrared"), new InputOutputColumnPair(@"Distance", @"Distance") })
                .Append(mlContext.Transforms.Text.FeaturizeText(@"CreatedAt", @"CreatedAt"))
                .Append(mlContext.Transforms.Concatenate(@"Features", new[] { @"Temperature", @"Humidity", @"Infrared", @"Distance", @"CreatedAt" }))
                .Append(mlContext.BinaryClassification.Trainers.FastTree(new FastTreeBinaryTrainer.Options() { NumberOfLeaves = 5, MinimumExampleCountPerLeaf = 21, NumberOfTrees = 4, MaximumBinCountPerFeature = 248, LearningRate = 0.0327903143558005F, FeatureFraction = 0.913244540244145F, LabelColumnName = @"IsAlarm", FeatureColumnName = @"Features" }));

            var model = trainingPipeline.Fit(trainingData);

            mlContext.Model.Save(model, trainingData.Schema, "model.zip");
        }

        public ModelOutput Predict(ModelInput reading)
        {
            //TODO use Path.GetFullPath??
            var model = mlContext.Model.Load("model.zip", out var _);

            //TODO use predictionenginepool here!!!!
            var predictor = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            var predicted = predictor.Predict(reading);

            System.Console.WriteLine($"Prediction for {reading} is *** {predicted.Prediction} ***");

            return predicted;
        }
    }
}
