using System;

namespace Hw2
{
    public enum CarType { Tesla, Bmw, Lada }
    public enum DriveMode { Park, Reverse, Neutral, Drive, Sport }

    public interface IComponentInfo
    {
        string GetComponentDescription();
    }

    public interface ICar
    {
        string Brand { get; }
        int Seats { get; }
        string GetDescription();
        void Start();
        void Stop();
    }

    public interface IDrivable
    {
        double CurrentSpeed { get; }
        void SetDriveMode(DriveMode mode);
        void Accelerate(double deltaSpeed);
        void Brake(double deltaSpeed);
    }

    public interface IEngine : IComponentInfo
    {
        string EngineType { get; }
        bool IsRunning { get; }
        double CurrentRPM { get; }
        void Start();
        void Stop();
        void SetRPM(double rpm);
    }

    public interface IElectricEngine : IEngine
    {
        double BatteryLevelKwh { get; }
        void Charge(double kwhAmount);
    }

    public interface ICombustionEngine : IEngine
    {
        double FuelLevelLiters { get; }
        void Refuel(double litersAmount);
    }

    public interface ITransmission : IComponentInfo
    {
        string TransmissionType { get; }
        DriveMode CurrentMode { get; }
        void SetMode(DriveMode mode);
        double GetRatio();
    }

    public interface IMultiGearTransmission : ITransmission
    {
        int CurrentGear { get; }
        void ShiftUp();
        void ShiftDown();
    }

    public interface IAutomaticTransmission : IMultiGearTransmission
    {
        void AdaptiveShift(double speed, double acceleration);
    }

    public interface IManualTransmission : IMultiGearTransmission
    {
        bool IsClutchPressed { get; }
        void PressClutch();
        void ReleaseClutch();
    }

    public interface ISmartCar { string OsVersion { get; } }

    public static class GearSystem
    {
        private static readonly double[] _ratios = [0.0, 3.5, 2.0, 1.4, 1.0, 0.8, 0.6, 0.5, 0.4];
        public const int MaxAutoGear = 8;
        public const int MaxManualGear = 5;

        public static double GetRatio(int gear)
        {
            if (gear < 0 || gear >= _ratios.Length)
                throw new ArgumentOutOfRangeException(nameof(gear));
            return _ratios[gear];
        }
    }

    public class ElectricEngine : IElectricEngine
    {
        private const double IdleRpm = 0.0;
        private const double MaxRpm = 16000.0;
        public string EngineType => "Electric";
        public bool IsRunning { get; private set; }
        public double CurrentRPM { get; private set; }
        public double BatteryLevelKwh { get; private set; } = 100.0;

        public void Start() { IsRunning = true; CurrentRPM = IdleRpm; }
        public void Stop() { IsRunning = false; CurrentRPM = 0; }
        public void SetRPM(double rpm)
        {
            if (IsRunning)
                CurrentRPM = Math.Min(rpm, MaxRpm);
        }
        public void Charge(double kwhAmount) => BatteryLevelKwh = Math.Min(100.0, BatteryLevelKwh + kwhAmount);
        public string GetComponentDescription() => $"{EngineType} (Battery: {BatteryLevelKwh} kWh)";
    }

    public class CombustionEngine : ICombustionEngine
    {
        private const double IdleRpm = 1000.0;
        private const double MaxRpm = 7000.0;
        public string EngineType => "Combustion";
        public bool IsRunning { get; private set; }
        public double CurrentRPM { get; private set; }
        public double FuelLevelLiters { get; private set; } = 60.0;

        public void Start()
        {
            IsRunning = true;
            CurrentRPM = IdleRpm;
        }

        public void Stop()
        {
            IsRunning = false;
            CurrentRPM = 0;
        }

        public void SetRPM(double rpm) { if (IsRunning) CurrentRPM = Math.Min(rpm, MaxRpm); }
        public void Refuel(double litersAmount) => FuelLevelLiters = Math.Min(60.0, FuelLevelLiters + litersAmount);
        public string GetComponentDescription() => $"{EngineType} (Fuel: {FuelLevelLiters} L)";
    }

    public class SingleSpeedTransmission : ITransmission
    {
        private const double FixedReductionRatio = 1.0;

        public DriveMode CurrentMode { get; private set; } = DriveMode.Park;
        public string TransmissionType => "Single-Speed Reduction Gear";

        public void SetMode(DriveMode mode) => CurrentMode = mode;

        public double GetRatio() => CurrentMode == DriveMode.Drive ? FixedReductionRatio : 0;

        public string GetComponentDescription() => $"{TransmissionType} (Mode: {CurrentMode})";
    }

    public class AutomaticTransmission : IAutomaticTransmission
    {
        public int CurrentGear { get; private set; } = 0;
        public DriveMode CurrentMode { get; private set; } = DriveMode.Park;
        public string TransmissionType => "Auto";

        public void ShiftUp()
        {
            if (CurrentGear < GearSystem.MaxAutoGear)
                CurrentGear++;
        }

        public void ShiftDown()
        {
            if (CurrentGear > 0)
                CurrentGear--;
        }

        public double GetRatio() => GearSystem.GetRatio(CurrentGear);

        public void SetMode(DriveMode mode)
        {
            if (mode == DriveMode.Drive && CurrentMode != DriveMode.Drive)
                CurrentGear = 1;
            else if (mode == DriveMode.Park || mode == DriveMode.Neutral)
                CurrentGear = 0;
            CurrentMode = mode;
        }

        public void AdaptiveShift(double speed, double acceleration)
        {
            if (CurrentMode == DriveMode.Drive)
            {
                if (acceleration > 15.0 && CurrentGear < GearSystem.MaxAutoGear)
                    ShiftUp();
                else if (speed < 30.0 && CurrentGear > 1)
                    ShiftDown();
            }
        }

        public string GetComponentDescription() => $"{TransmissionType} (Mode: {CurrentMode}, Gear: {CurrentGear})";
    }

    public class ManualTransmission(int maxGear) : IManualTransmission
    {
        private readonly int _maxGear = maxGear;
        public int CurrentGear { get; private set; } = 0;
        public DriveMode CurrentMode { get; private set; } = DriveMode.Neutral;
        public bool IsClutchPressed { get; private set; }
        public string TransmissionType => "Manual";

        public void PressClutch() => IsClutchPressed = true;
        public void ReleaseClutch() => IsClutchPressed = false;
        public void ShiftUp()
        {
            if (IsClutchPressed && CurrentGear < _maxGear)
                CurrentGear++;
        }

        public void ShiftDown()
        {
            if (IsClutchPressed && CurrentGear > 0)
                CurrentGear--;
        }

        public double GetRatio() => GearSystem.GetRatio(CurrentGear);

        public void SetMode(DriveMode mode)
        {
            CurrentMode = mode;
            if (mode == DriveMode.Drive)
            {
                PressClutch();
                if (CurrentGear == 0) CurrentGear = 1;
                ReleaseClutch();
            }
            else if (mode == DriveMode.Neutral || mode == DriveMode.Park)
            {
                PressClutch();
                CurrentGear = 0;
                ReleaseClutch();
            }
        }

        public string GetComponentDescription() => $"{TransmissionType} (Gear: {CurrentGear})";
    }

    public abstract class ACar<TEngine, TTransmission>(string brand, int seats, TEngine engine, TTransmission transmission) : ICar, IDrivable
        where TEngine : IEngine
        where TTransmission : ITransmission
    {
        private const double MaxSpeedLimit = 250.0;
        private const double SpeedToRpmFactor = 30.0;

        protected readonly TEngine _engine = engine;
        protected readonly TTransmission _transmission = transmission;

        public string Brand { get; } = brand;
        public int Seats { get; } = seats;
        public double CurrentSpeed { get; private set; }

        public virtual void Start()
        {
            _engine.Start();
            Console.WriteLine($"[{Brand}] Engine started.");
        }

        public virtual void Stop()
        {
            _engine.Stop();
            CurrentSpeed = 0;
            Console.WriteLine($"[{Brand}] Engine stopped.");
        }

        public void SetDriveMode(DriveMode mode)
        {
            _transmission.SetMode(mode);
            Console.WriteLine($"[{Brand}] Switched to {mode} mode.");
        }

        public virtual void Accelerate(double deltaSpeed)
        {
            if (_engine.IsRunning && _transmission.CurrentMode == DriveMode.Drive)
            {
                CurrentSpeed = Math.Min(MaxSpeedLimit, CurrentSpeed + deltaSpeed);
                _engine.SetRPM(CurrentSpeed * SpeedToRpmFactor * _transmission.GetRatio());

                if (_transmission is IAutomaticTransmission autoTrans)
                {
                    autoTrans.AdaptiveShift(CurrentSpeed, deltaSpeed);
                }

                Console.WriteLine($"-> {Brand} speeding up: {CurrentSpeed} km/h (RPM: {_engine.CurrentRPM})");
            }
            else
            {
                Console.WriteLine($"[{Brand}] Cannot accelerate. Engine off or not in Drive mode.");
            }
        }

        public virtual void Brake(double deltaSpeed)
        {
            CurrentSpeed = Math.Max(0, CurrentSpeed - deltaSpeed);
            _engine.SetRPM(CurrentSpeed * SpeedToRpmFactor * _transmission.GetRatio());
            Console.WriteLine($"-> {Brand} slowing down: {CurrentSpeed} km/h");
        }

        public virtual string GetDescription()
        {
            string osInfo = this is ISmartCar smartCar ? $"{smartCar.OsVersion}" : "No OS";
            return $"[{Brand}] {_engine.GetComponentDescription()} | {_transmission.GetComponentDescription()} | {Seats} seats | {osInfo}";
        }
    }

    public class TeslaCar : ACar<IElectricEngine, SingleSpeedTransmission>, ISmartCar
    {
        public string OsVersion => "Android OS";
        public TeslaCar() : base("Tesla", 5, new ElectricEngine(), new SingleSpeedTransmission()) { }
    }

    public class BmwCar : ACar<ICombustionEngine, IAutomaticTransmission>, ISmartCar
    {
        public string OsVersion => "BMW OS 8";
        public BmwCar() : base("BMW", 4, new CombustionEngine(), new AutomaticTransmission()) { }
    }

    public class LadaCar : ACar<ICombustionEngine, IManualTransmission>
    {
        public LadaCar() : base("Lada", 5, new CombustionEngine(), new ManualTransmission(GearSystem.MaxManualGear)) { }
    }

    public static class CarFactory
    {
        public static ICar GetCar(CarType carType) => carType switch
        {
            CarType.Tesla => new TeslaCar(),
            CarType.Bmw => new BmwCar(),
            CarType.Lada => new LadaCar(),
            _ => throw new ArgumentException("Unknown car type")
        };
    }

    class Program
    {
        static void Main()
        {
            while (true)
            {
                Console.Write("\nPick a car (Tesla, Bmw, Lada) or type 'done': ");
                string input = Console.ReadLine()?.Trim() ?? string.Empty;

                if (string.Equals(input, "done", StringComparison.OrdinalIgnoreCase)) break;

                if (Enum.TryParse(input, true, out CarType selectedCarType) &&
                    Enum.IsDefined(selectedCarType))
                {
                    ICar car = CarFactory.GetCar(selectedCarType);
                    Console.WriteLine(car.GetDescription());

                    if (car is IDrivable drivableCar)
                    {
                        Console.WriteLine("--- Test Drive ---");
                        car.Start();

                        drivableCar.SetDriveMode(DriveMode.Drive);
                        drivableCar.Accelerate(30);
                        drivableCar.Accelerate(20);
                        drivableCar.Brake(25);

                        car.Stop();
                        Console.WriteLine("------------------");
                    }
                }
                else
                {
                    Console.WriteLine("Car not found. Try again.");
                }
            }
        }
    }
}
