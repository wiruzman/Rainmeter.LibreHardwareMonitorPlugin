using System;
using System.Linq;
using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;
using Rainmeter;

namespace PluginLibreHardwareMonitor
{
    internal class Measure
    {
        private API _api;
        private Computer _computer;
        private string _identifier = string.Empty;
        private ISensor _sensor;

        public static implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
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
        
        internal void Initialize(API api)
        {
            _api = api;
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true
            };

            _computer.Open();
            _computer.Accept(new UpdateVisitor());
            
            var type = api.ReadString("Identifier", string.Empty);
            _identifier = type;
            var sensors = _computer.Hardware.SelectMany(h => h.Sensors.Concat(h.SubHardware.SelectMany(s => s.Sensors)));
            _sensor = sensors.SingleOrDefault(s => s.Identifier.ToString() == _identifier);
        }
        
        internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            var type = api.ReadString("Identifier", string.Empty);
            _identifier = type;
        }
        
        internal double Update()
        {
            _sensor.Hardware.Update();
            if (_sensor?.Value is null)
            {
                return 0.0;
            }

            return (double)_sensor.Value;
        }

        internal void Close()
        {
            _computer.Close();
        }
    }
}

