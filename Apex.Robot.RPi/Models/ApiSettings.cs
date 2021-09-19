namespace Apex.Robot.RPi.Models
{
    public class ApiSettings
    {
        public string VisionUrl { get; set; }
        
        public string RaspberryPiUrl { get; set; }

        public string CameraHub { get; set; }
        
        public string SensorsHub { get; set; }



        public int InfraredPin { get; set; }

        public int TemperaturePin { get; set; }

        public int LinePin { get; set; }

        public int ProximityTriggerPin { get; set; }

        public int ProximityEchoPin { get; set; }

        public int ProximityMaxDistance { get; set; }

        public int ProximityDistance { get; set; }


        public int RightBackwardPin { get; set; }
        public int RightForwardPin { get; set; }
        public int LeftBackwardPin { get; set; }
        public int LeftForwardPin { get; set; }



        public int ReadingDelay { get; set; }

        public int CaptureDelay { get; set; }

        public uint ImageWidth { get; set; }

        public uint ImageHeight { get; set; }
    }
}
