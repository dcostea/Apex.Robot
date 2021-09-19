using Apex.Robot.RPi.Models;

namespace Apex.Robot.RPi.Interfaces
{
    public interface IPredictionsService
    {
        ModelOutput Predict();
        void Train();
        ModelOutput TrainAndPredict();
    }
}