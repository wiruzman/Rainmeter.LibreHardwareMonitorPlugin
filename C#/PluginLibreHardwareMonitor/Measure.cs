using System;
using System.Runtime.InteropServices;

namespace PluginLibreHardwareMonitor
{
    internal class Measure
    {
        public Measure(string identifier)
        {
            Identifier = identifier;
        }

        public static implicit operator Measure(IntPtr data)
        {
            return (Measure)GCHandle.FromIntPtr(data).Target;
        }
        
        internal double Update()
        {
            return SensorLibraryManager.GetSensorValue(Identifier);
        }

        public string Identifier { get; }
    }
}

