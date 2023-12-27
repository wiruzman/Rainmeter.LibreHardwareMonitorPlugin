using System;
using System.Runtime.InteropServices;

namespace PluginLibreHardwareMonitor
{
    internal class Measure
    {
        private readonly string _identifier;

        public Measure(string identifier)
        {
            _identifier = identifier;
        }

        public static implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }
        
        internal double Update()
        {
            return SensorLibraryManager.GetSensorValue(_identifier);

        }
    }
}

