namespace Apex.Robot.RPi.Interfaces
{
    public interface IMotorsService
    {
        void Backward(int milliseconds);
        void CheckMotors();
        void Forward(int milliseconds);
        void FullStop();
        void TurnLeft(int milliseconds);
        void TurnRight(int milliseconds);
        void RunAway(DateTime sleepTo);
    }
}