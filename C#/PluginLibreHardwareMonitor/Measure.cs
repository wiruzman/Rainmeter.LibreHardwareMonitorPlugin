using System;
using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;

namespace PluginLibreHardwareMonitor
{
    internal class Measure
    {
        private readonly string _identifier;
        private ISensor _sensor;

        public Measure(string identifier)
        {
            _identifier = identifier;
            _sensor = SensorLibraryManager.GetSensor(identifier);
        }

        public static implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }
        
        internal double Update()
        {
            if (_sensor == null)
            {
                _sensor = SensorLibraryManager.GetSensor(_identifier);
            }

            if (_sensor == null)
            {
                return 0.0;
            }
            
            _sensor.Hardware.Update();
            return _sensor.Value ?? 0.0;

        }
    }
}

