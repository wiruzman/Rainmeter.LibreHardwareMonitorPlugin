using System;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace ConsoleLibreHardwareMonitor
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Computer computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsStorageEnabled = true
            };

            computer.Open();
            computer.Accept(new UpdateVisitor());

            var sensors = computer.Hardware.SelectMany(h => h.Sensors.Concat(h.SubHardware.SelectMany(s => s.Sensors).ToArray()).ToArray()).ToArray();
            foreach (var sensor in sensors)
            {
                Console.WriteLine("Sensor: {0}, value: {1}, identifier: {2}", sensor.Name, sensor.Value, sensor.Identifier);
            }
    
            computer.Close();
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