using System;
using System.Runtime.InteropServices;
using Rainmeter;

namespace PluginLibreHardwareMonitor
{
    public class Plugin
    {
        private const string IdentifierSettingString = "Identifier";
        
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            var api = new API(rm);
            var identifier = api.ReadString(IdentifierSettingString, string.Empty);
            SensorLibraryManager.Initialize(api, identifier);
            
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure(identifier)));
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = data;
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            GCHandle.FromIntPtr(data).Free();
            SensorLibraryManager.Close(measure.Identifier);
        }
    }
}