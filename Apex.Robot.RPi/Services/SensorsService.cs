using Iot.Device.Hcsr04;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnitsNet;
using Iot.Device.DHTxx;
using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;

namespace Apex.Robot.RPi.Services
{
    public class SensorsService : ISensorsService
    {
        private readonly GpioController gpioController;

        private readonly ApiSettings _settings;

        public SensorsService(ApiSettings settings)
        {
            _settings = settings;

            try
            {
                gpioController = new GpioController(PinNumberingScheme.Logical);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public double ReadDistanceFromDevice()
        {
            using Hcsr04 sonar = new(gpioController, _settings.ProximityTriggerPin, _settings.ProximityEchoPin);
            do
            {
                var hasDistance = sonar.TryGetDistance(out Length distance);

                if (hasDistance)
                {
                    return distance.Centimeters;
                }

                Thread.Sleep(100);
            }
            while (true);
        }

        public double ReadHumidity()
        {
            using Dht11 dht11 = new(_settings.TemperaturePin);
            double humidity;
            do
            {
                humidity = dht11.Humidity.Percent;
            }
            while (!dht11.IsLastReadSuccessful);

            return humidity;
        }

        public double ReadTemperature()
        {
            using Dht11 dht11 = new(_settings.TemperaturePin);
            double temperature;
            do
            {
                temperature = dht11.Temperature.DegreesCelsius;
            } 
            while (!dht11.IsLastReadSuccessful);

            return temperature;
        }

        public double ReadDistance()
        {
            double distance;
            Stopwatch timeWatcher = new Stopwatch();
            gpioController.OpenPin(_settings.ProximityTriggerPin, PinMode.Output, PinValue.Low);

            var mre = new ManualResetEvent(false);
            mre.WaitOne(500);
            timeWatcher.Reset();

            //Send pulse
            gpioController.Write(_settings.ProximityTriggerPin, PinValue.High);
            mre.WaitOne(TimeSpan.FromMilliseconds(0.01));
            gpioController.Write(_settings.ProximityTriggerPin, PinValue.Low);

            gpioController.OpenPin(_settings.ProximityEchoPin, PinMode.Input);
            var t = Task.Run(() =>
            {
                //Receive pulse
                while (gpioController.Read(_settings.ProximityEchoPin) != PinValue.High) { }
                timeWatcher.Start();

                while (gpioController.Read(_settings.ProximityEchoPin) == PinValue.High) { }
                timeWatcher.Stop();

                //Calculating distance
                distance = timeWatcher.Elapsed.TotalSeconds * 17000;
                return Math.Round(distance, 2);
            });

            bool didComplete = t.Wait(TimeSpan.FromMilliseconds(100));

            gpioController.ClosePin(_settings.ProximityTriggerPin);
            gpioController.ClosePin(_settings.ProximityEchoPin);

            if (didComplete)
            {
                return Math.Round(t.Result, 2);
            }
            else
            {
                return 400; // if no response, assumes the distance is MAX_DISTANCE             
            }
        }

        //TODO review
        public double FollowLine()
        {
            if (gpioController.Read(_settings.LinePin) == PinValue.Low)
            {
                return 0;
            }
            else 
            {
                return 1;
            }
        }

        public double ReadInfrared()
        {
            gpioController.OpenPin(_settings.InfraredPin, PinMode.Input);

            var infrared = gpioController.Read(_settings.InfraredPin);

            return infrared == PinValue.High ? 1 : 0;
        }
    }
}
