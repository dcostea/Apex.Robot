namespace Apex.Robot.RPi.Interfaces
{
    public interface ICameraService
    {
        byte[] GetImage(uint width, uint height);

        Task<string> GetInceptionPrediction(string fileName);
    }
}
