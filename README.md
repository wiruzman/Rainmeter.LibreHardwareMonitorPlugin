# Rainmeter.LibreHardwareMonitorPlugin

Plugin for Rainmeter to use sensors from LibreHardwareMonitor.

# Build

- Build project `PluginLibreHardwareMonitor`
- Copy `LibreHardwareMonitor.dll` from `PluginLibreHardwareMonitor\<platform>\<configuration>` i.e., `PluginLibreHardwareMonitor\x64\Release`
- Paste into `C:\Users\<username>\AppData\Roaming\Rainmeter\Plugins`

# Usage

In Rainmeter skin file (.ini), the plugin can be used as following,

```
[Temperature1]
Measure=Plugin
Plugin=LibreHardwareMonitor.dll
Identifier=/lpc/ec/temperature/1

[Temperature1Meter]
Meter=String
MeasureName=Temperature1
FontSize=8
Text=%1
X=0r
Y=14r
```

To determine `Identifier`, run `ConsoleLibreHardwareMonitor` to find the identifier the sensor you want to use.