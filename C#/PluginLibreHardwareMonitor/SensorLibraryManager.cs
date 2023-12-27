using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using Rainmeter;

namespace PluginLibreHardwareMonitor
{
    public static class SensorLibraryManager
    {
        private static readonly Computer Computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsStorageEnabled = true
        };

        private static bool _initializationStarted;
        private static bool _initialized;
        private static Task _initializationTask;
        private static ISensor[] _sensors = {};
        private static readonly ConcurrentDictionary<string, double> SensorValues = new ConcurrentDictionary<string, double>();
        private static readonly List<Timer> Timers = new List<Timer>();

        public static void Initialize(API api, string identifier)
        {
            api.LogF(API.LogType.Notice, "Register sensor: {0}", identifier);
            Timers.Add(new Timer(state => UpdateSensorValue(identifier), null, 0, 1000));
            if (_initializationStarted) return;
            _initializationStarted = true;
            if (_initialized && _initializationTask != null) return;
            _initializationTask = Task.Run(() =>
            {
                Computer.Open();
                Computer.Accept(new UpdateVisitor());
                _sensors = Computer.Hardware
                    .SelectMany(h => h.Sensors.Concat(h.SubHardware.SelectMany(sh => sh.Sensors))).ToArray();
                _initialized = true;
            });
        }

        private static void UpdateSensorValue(string identifier)
        {
            var sensor = _sensors.FirstOrDefault(s => s.Identifier.ToString() == identifier);
            if (sensor == null) return;
            sensor.Hardware.Update();
            SensorValues[identifier] = sensor?.Value ?? 0.0;
        }

        public static double GetSensorValue(string identifier)
        {
            return SensorValues.TryGetValue(identifier, out var sensorValue) ? sensorValue : 0.0;
        }

        public static void Close()
        {
            _initializationTask?.Wait();
            foreach (var timer in Timers)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
            }
            Timers.Clear();
            if (Computer == null) return;
            Computer.Close();
            _initialized = false;
            _initializationTask?.Dispose();
            _initializationTask = null;
        }
        
        private class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
    }
}