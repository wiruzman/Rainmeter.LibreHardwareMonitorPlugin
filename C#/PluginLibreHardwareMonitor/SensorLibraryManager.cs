using System.Linq;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;

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
        private static ISensor[] _sensors;

        public static void Initialize()
        {
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

        public static ISensor GetSensor(string identifier)
        {
            return _initialized ? _sensors.FirstOrDefault(s => s.Identifier.ToString() == identifier) : null;
        }

        public static void Close()
        {
            _initializationTask?.Wait();
            if (Computer == null) return;
            Computer.Close();
            _initialized = false;
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