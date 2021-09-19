namespace Apex.Robot.RPi.Interfaces
{
    public interface ICameraService
    {
        public byte[] GetImage(uint width, uint height);
    }
}
