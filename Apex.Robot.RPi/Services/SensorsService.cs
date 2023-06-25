using System.Device.Gpio;
using System.Diagnostics;
using UnitsNet;
using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using Serilog;
using System.Device.I2c;
using Iot.Device.Sht3x;
using Iot.Device.Mlx90614;
using Iot.Device.Bh1750fvi;
using Iot.Device.Hcsr04;

namespace Apex.Robot.RPi.Services
{
    public class SensorsService : ISensorsService
    {
        private readonly ApiSettings _settings;
        private readonly GpioController _gpioController;
        private I2cDevice _illuminanceDevice;
        private I2cDevice _infraredDevice;
        private I2cDevice _temperatureDevice;
        private readonly I2cConnectionSettings _illuminanceConnectionSettings;
        private readonly I2cConnectionSettings _infraredConnectionSettings;
        private readonly I2cConnectionSettings _temperatureConnectionSettings;
        private Bh1750fvi _illuminanceSensor;
        private Mlx90614 _infraredSensor;
        private Sht3x _temperatureSensor;

        public SensorsService(ApiSettings settings)
        {
            _settings = settings;
            _illuminanceConnectionSettings = new(busId: 1, (int)Iot.Device.Bh1750fvi.I2cAddress.AddPinLow);
            _infraredConnectionSettings = new(busId: 3, Mlx90614.DefaultI2cAddress);
            _temperatureConnectionSettings = new(busId: 4, (byte)Iot.Device.Sht3x.I2cAddress.AddrLow);

            try
            {
                _gpioController = new GpioController(PinNumberingScheme.Logical);
                Log.Information($"GPIO controller has been initialized!");
            }
            catch (Exception ex)
            {
                Log.Error($"GPIO controller NOT initialized! {ex.Message}");
            }

            try
            {
                _illuminanceDevice = I2cDevice.Create(_illuminanceConnectionSettings);
                Log.Information($"I2C illuminance has been initialized!");
            }
            catch (Exception ex)
            {
                Log.Error($"I2C illuminance device NOT initialized! {ex.Message}");
            }

            try
            {
                _infraredDevice = I2cDevice.Create(_infraredConnectionSettings);
                Log.Information($"I2C infrared has been initialized!");
            }
            catch (Exception ex)
            {
                Log.Error($"I2C infrared device NOT initialized! {ex.Message}");
            }

            try
            {
                _temperatureDevice = I2cDevice.Create(_temperatureConnectionSettings);
                Log.Information($"I2C temperature has been initialized!");
            }
            catch (Exception ex)
            {
                Log.Error($"I2C temperature device NOT initialized! {ex.Message}");
            }

            try
            {
                _illuminanceSensor = new(_illuminanceDevice);
                Log.Information($"Illuminance sensor has been initialized!");
            }
            catch (Exception ex)
            {
                Log.Error($"Illuminance sensor NOT initialized! {ex.Message}");
            }

            try
            {
                _infraredSensor = new(_infraredDevice);
                Log.Information($"Infrared sensor has been initialized!");
            }
            catch (Exception ex)
            {
                Log.Error($"Infrared sensor NOT initialized! {ex.Message}");
            }

            try
            {
                _temperatureSensor = new(_temperatureDevice);
                Log.Information($"Temperature sensor has been initialized!");
            }
            catch (Exception ex)
            {
                Log.Error($"Teemperature sensor NOT initialized! {ex.Message}");
            }
        }

        public double ReadDistanceFromDevice()
        {
            using Hcsr04 sonar = new(_gpioController, _settings.ProximityTriggerPin, _settings.ProximityEchoPin);
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

        public double ReadLuminosity() 
        {
            if (_illuminanceDevice is null)
            {
                _illuminanceDevice = I2cDevice.Create(_illuminanceConnectionSettings);
                Log.Information($"Illuminance device has been reinitialized!");
            }

            if (_illuminanceSensor is null)
            {
                _illuminanceSensor = new(_illuminanceDevice);
                Log.Information($"Illuminance sensor has been reinitialized!");
            }

            var illuminance = _illuminanceSensor.Illuminance.Value;

            return Math.Round(illuminance, 2);
        }

        public double ReadTemperature()
        {
            if (_temperatureDevice is null)
            {
                _temperatureDevice = I2cDevice.Create(_temperatureConnectionSettings);
                Log.Information($"Temperature device has been reinitialized!");
            }

            if (_temperatureSensor is null)
            {
                _temperatureSensor = new(_temperatureDevice);
                Log.Information($"Temperature sensor has been reinitialized!");
            }

            var temperature = _temperatureSensor.Temperature.Value;

            return Math.Round(temperature, 2);
        }

        public double ReadHumidity()
        {
            if (_temperatureDevice is null)
            {
                _temperatureDevice = I2cDevice.Create(_temperatureConnectionSettings);
                Log.Information($"Temperature (humidity) device has been reinitialized!");
            }

            if (_temperatureSensor is null)
            {
                _temperatureSensor = new(_temperatureDevice);
                Log.Information($"Temperature (Humidity) sensor has been reinitialized!");
            }

            var humidity = _temperatureSensor.Humidity.Value;

            return Math.Round(humidity, 2);
        }

        public double ReadInfrared()
        {
            if (_infraredDevice is null)
            {
                _infraredDevice = I2cDevice.Create(_infraredConnectionSettings);
                Log.Information($"Infrared device has been reinitialized!");
            }

            if (_infraredSensor is null)
            {
                _infraredSensor = new(_infraredDevice);
                Log.Information($"Infrared sensor has been reinitialized!");
            }

            try
            {
                var infrared = _infraredSensor.ReadObjectTemperature().DegreesCelsius;
                return Math.Round(infrared, 2);
            }
            catch (Exception)
            {
                return 0D;
            }
        }

        public double ReadDistance()
        {
            double distance;
            Stopwatch timeWatcher = new();
            _gpioController.OpenPin(_settings.ProximityTriggerPin, PinMode.Output, PinValue.Low);

            var mre = new ManualResetEvent(false);
            mre.WaitOne(500);
            timeWatcher.Reset();

            //Send pulse
            _gpioController.Write(_settings.ProximityTriggerPin, PinValue.High);
            mre.WaitOne(TimeSpan.FromMilliseconds(0.01));
            _gpioController.Write(_settings.ProximityTriggerPin, PinValue.Low);

            _gpioController.OpenPin(_settings.ProximityEchoPin, PinMode.Input);
            var t = Task.Run(() =>
            {
                //Receive pulse
                while (_gpioController.Read(_settings.ProximityEchoPin) != PinValue.High) { }
                timeWatcher.Start();

                while (_gpioController.Read(_settings.ProximityEchoPin) == PinValue.High) { }
                timeWatcher.Stop();

                //Calculating distance
                distance = timeWatcher.Elapsed.TotalSeconds * 17000;
                return Math.Round(distance, 2);
            });

            bool didComplete = t.Wait(TimeSpan.FromMilliseconds(100));

            _gpioController.ClosePin(_settings.ProximityTriggerPin);
            _gpioController.ClosePin(_settings.ProximityEchoPin);

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
            if (_gpioController.Read(_settings.LinePin) == PinValue.Low)
            {
                return 0;
            }
            else 
            {
                return 1;
            }
        }

        //public double ReadInfrared()
        //{
        //    _gpioController.OpenPin(_settings.InfraredPin, PinMode.Input);
        //    var infrared = _gpioController.Read(_settings.InfraredPin);
        //    _gpioController.ClosePin(_settings.InfraredPin);

        //    return infrared == PinValue.High ? 0 : 1;
        //}
    }
}
