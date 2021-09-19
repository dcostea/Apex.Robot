using Apex.Robot.RPi.Interfaces;
using Apex.Robot.RPi.Models;
using System;
using System.Device.Gpio;

namespace Apex.Robot.RPi.Services
{
    public class MotorsService : IMotorsService
    {
        private readonly GpioController _gpioController;
        private readonly ApiSettings _settings;

        public MotorsService(ApiSettings settings)
        {
            _settings = settings;

            _gpioController = new GpioController(PinNumberingScheme.Logical);
            _gpioController.OpenPin(_settings.LeftBackwardPin, PinMode.Output);
            _gpioController.OpenPin(_settings.LeftForwardPin, PinMode.Output);
            _gpioController.OpenPin(_settings.RightBackwardPin, PinMode.Output);
            _gpioController.OpenPin(_settings.RightForwardPin, PinMode.Output);
        }

        public void FullStop()
        {
            _gpioController.Write(_settings.LeftBackwardPin, PinValue.Low);
            _gpioController.Write(_settings.LeftForwardPin, PinValue.Low);
            _gpioController.Write(_settings.RightBackwardPin, PinValue.Low);
            _gpioController.Write(_settings.RightForwardPin, PinValue.Low);

            Console.WriteLine("full stop...");
        }

        public void TurnLeft(int milliseconds)
        {
            _gpioController.Write(_settings.RightBackwardPin, PinValue.Low);
            _gpioController.Write(_settings.RightForwardPin, PinValue.High);

            Console.WriteLine("turn left...");
            System.Threading.Thread.Sleep(milliseconds);
            FullStop();
        }

        public void TurnRight(int milliseconds)
        {
            _gpioController.Write(_settings.LeftBackwardPin, PinValue.Low);
            _gpioController.Write(_settings.LeftForwardPin, PinValue.High);

            Console.WriteLine("turn right...");
            System.Threading.Thread.Sleep(milliseconds);
            FullStop();
        }

        public void Forward(int milliseconds)
        {
            _gpioController.Write(_settings.RightBackwardPin, PinValue.Low);
            _gpioController.Write(_settings.RightForwardPin, PinValue.High);
            _gpioController.Write(_settings.LeftBackwardPin, PinValue.Low);
            _gpioController.Write(_settings.LeftForwardPin, PinValue.High);

            Console.WriteLine("forward...");
            System.Threading.Thread.Sleep(milliseconds);
            FullStop();
        }

        public void Backward(int milliseconds)
        {
            _gpioController.Write(_settings.RightBackwardPin, PinValue.High);
            _gpioController.Write(_settings.RightForwardPin, PinValue.Low);
            _gpioController.Write(_settings.LeftBackwardPin, PinValue.High);
            _gpioController.Write(_settings.LeftForwardPin, PinValue.Low);

            Console.WriteLine("backward...");
            System.Threading.Thread.Sleep(milliseconds);
            FullStop();
        }

        public void CheckMotors()
        {
            FullStop();

            Forward(500);

            TurnLeft(300);

            TurnRight(300);

            Backward(500);
        }
    }
}
