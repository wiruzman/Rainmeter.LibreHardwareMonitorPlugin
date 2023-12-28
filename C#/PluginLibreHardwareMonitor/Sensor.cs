using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using Rainmeter;

namespace PluginLibreHardwareMonitor
{
    public class Sensor
    {
        private readonly API _api;
        private readonly string _identifier;
        private const int UpdateInterval = 500;
        private bool _isRunningUpdate = true;
        private ISensor _hardwareSensor;
        public double Value { get; private set; }

        public Sensor(API api, string identifier)
        {
            _api = api;
            _identifier = identifier;
            Value = 0.0;
            Update();
        }

        private async Task Update()
        {
            while (_isRunningUpdate)
            {
                _api.Log(API.LogType.Debug, "Update sensor started");
                if (_hardwareSensor == null)
                {
                    _hardwareSensor = SensorLibraryManager.GetSensor(_identifier);
                }

                if (_hardwareSensor == null)
                {
                    _api.Log(API.LogType.Debug, "Sensor not ready");
                    Value = 0.0;
                    await Task.Delay(UpdateInterval);
                    continue;
                }
            
                _api.Log(API.LogType.Debug, "Update sensor");
                _hardwareSensor.Hardware.Update();
                _api.Log(API.LogType.Debug, "Sensor updated");
                Value = _hardwareSensor.Value ?? 0.0;
                await Task.Delay(UpdateInterval);
            }
        }

        public void Close()
        {
            _isRunningUpdate = false;
            _api.LogF(API.LogType.Notice, "Sensor closed: {0}", _identifier);
        }
    }
}