using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;
using Serilog;

namespace Apex.Robot.RPi.Services
{
    public class PredictionsService : IPredictionsService
    {
        private readonly MLContext mlContext;
        private readonly ApiSettings _settings;
        private readonly PredictionEnginePool<ModelInput, ModelOutput> _predictionEngine;

        public PredictionsService(PredictionEnginePool<ModelInput, ModelOutput> predictionEngine, ApiSettings settings)
        {
            _predictionEngine = predictionEngine;
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

            var featureColumns = new[] { "Luminosity", "Temperature", "Infrared" };

            var trainingPipeline = mlContext.Transforms.ReplaceMissingValues(new[]
{
                new InputOutputColumnPair(@"Luminosity", @"Luminosity"),
                new InputOutputColumnPair(@"Temperature", @"Temperature"),
                new InputOutputColumnPair(@"Infrared", @"Infrared"),
            })
                .Append(mlContext.Transforms.Concatenate(@"Features", featureColumns))
                .Append(mlContext.BinaryClassification.Trainers.FastTree(new FastTreeBinaryTrainer.Options() { LabelColumnName = @"IsAlarm", FeatureColumnName = @"Features" }));

            var model = trainingPipeline.Fit(trainingData);

            var sampleData = new ModelInput
            {
                Luminosity = 10F,
                Temperature = 32F,
                Infrared = 1F
            };

            var predictor = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
            var predicted = predictor.Predict(sampleData);

            Log.Debug($"[PREDICT] IsAlarm: {predicted.Prediction} ({predicted.Probability:P2})");

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

            var featureColumns = new[] { "Luminosity", "Temperature", "Infrared" };

            var trainingPipeline = mlContext.Transforms.ReplaceMissingValues(new[] 
            {
                new InputOutputColumnPair(@"Luminosity", @"Luminosity"),
                new InputOutputColumnPair(@"Temperature", @"Temperature"), 
                new InputOutputColumnPair(@"Infrared", @"Infrared"), 
            })
                .Append(mlContext.Transforms.Concatenate(@"Features", featureColumns))
                .Append(mlContext.BinaryClassification.Trainers.FastTree(new FastTreeBinaryTrainer.Options() { LabelColumnName = @"IsAlarm", FeatureColumnName = @"Features" }));

            var model = trainingPipeline.Fit(trainingData);

            mlContext.Model.Save(model, trainingData.Schema, _settings.ModelFilePath);

            Log.Debug($"[TRAIN] model {_settings.ModelFilePath} trained and saved");
        }

        public ModelOutput Predict(ModelInput reading)
        {
            var predictionEngine = _predictionEngine.GetPredictionEngine("sensorsModel");
            var predicted = predictionEngine.Predict(reading);

            Log.Debug($"[SENSORS] {reading}");
            Log.Debug($"[PREDICT] IsAlarm: {predicted.Prediction} ({predicted.Probability:P2})");

            return predicted;
        }
    }
}
