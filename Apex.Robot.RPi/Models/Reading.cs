namespace Apex.Robot.RPi.Models
{
    public class Reading
    {
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Distance { get; set; }
        public double Infrared { get; set; }

        public string CreatedAt { get; set; }
        public string IsAlarm { get; set; }
    }
}
