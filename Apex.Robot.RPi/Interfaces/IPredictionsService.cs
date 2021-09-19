using Apex.Robot.RPi.Models;

namespace Apex.Robot.RPi.Interfaces
{
    public interface IPredictionsService
    {
        ModelOutput Predict(ModelInput reading);
        void Train();
        ModelOutput TrainAndPredict();
    }
}