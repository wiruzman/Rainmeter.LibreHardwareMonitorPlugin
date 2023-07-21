using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;
using Rainmeter;

namespace PluginLibreHardwareMonitor
{
    class Measure
    {
        private readonly Computer _computer;
        private string _identifier = string.Empty;
        
        internal Measure()
        {
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
        
        internal void Reload(Rainmeter.API api, ref double maxValue)
        {
            var type = api.ReadString("Identifier", string.Empty);
            _identifier = type;
        }
        
        internal double Update()
        {
            var sensors = _computer.Hardware.SelectMany(h => h.Sensors.Concat(h.SubHardware.SelectMany(s => s.Sensors)));
            var sensor = sensors.SingleOrDefault(s => s.Identifier.ToString() == _identifier);
            if (sensor?.Value is null)
            {
                return 0.0;
            }

            return (double)sensor.Value;
        }
    }

    public class Plugin
    {
        static IntPtr StringBuffer = IntPtr.Zero;

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new Rainmeter.API(rm), ref maxValue);
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            GCHandle.FromIntPtr(data).Free();

            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }
        }
    }
}

