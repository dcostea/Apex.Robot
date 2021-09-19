using System.Threading.Tasks;

namespace Apex.Robot.RPi.Interfaces
{
    public interface ISensorsService
    {
        double ReadInfrared();
        double ReadDistance();
        double ReadTemperature();
        double ReadHumidity();
        double FollowLine();
    }
}
