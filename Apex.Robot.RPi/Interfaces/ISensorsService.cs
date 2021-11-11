namespace Apex.Robot.RPi.Interfaces
{
    public interface ISensorsService
    {
        double ReadInfrared();
        double ReadDistance();
        double ReadTemperature();
        double ReadHumidity();
        double ReadLuminosity();
        double FollowLine();
    }
}
