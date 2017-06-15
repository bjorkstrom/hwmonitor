using OpenHardwareMonitor.Hardware;
using System.Collections.Generic;

class Motherboard
{
    /*
     * Sensor IDs we want to log, and how they map to Record's DataPoint type
     */
    static Dictionary<Identifier, Record.DataPoint> IDMap = new Dictionary<Identifier, Record.DataPoint>
    {
        { new Identifier("intelcpu", "0", "temperature", "0"), Record.DataPoint.CPUCore0Temperature },
        { new Identifier("intelcpu", "0", "temperature", "1"), Record.DataPoint.CPUCore1Temperature },
        { new Identifier("intelcpu", "0", "temperature", "2"), Record.DataPoint.CPUCore2Temperature },
        { new Identifier("intelcpu", "0", "temperature", "3"), Record.DataPoint.CPUCore3Temperature },
        { new Identifier("intelcpu", "0", "temperature", "4"), Record.DataPoint.CPUPackageTemperature },
        { new Identifier("nvidiagpu", "0", "temperature", "0"), Record.DataPoint.GPUCoreTemperature },
        { new Identifier("intelcpu", "0", "power", "0"), Record.DataPoint.CPUPackagePower },
        { new Identifier("intelcpu", "0", "power", "1"), Record.DataPoint.CPUCoresPower },
        { new Identifier("intelcpu", "0", "power", "3"), Record.DataPoint.CPUDRAMPower },
    };

    public static Computer computer = new Computer();

    public static void Init()
    {
        computer.Open();
        computer.CPUEnabled = true;
        computer.GPUEnabled = true;
    }

    public static void Update(Record Record)
    {
        foreach (var hardware in computer.Hardware)
        {
            hardware.Update();

            foreach (var sensor in hardware.Sensors)
            {
                if (!IDMap.ContainsKey(sensor.Identifier))
                {
                    /* we are not interested in this sensor */
                    continue;
                }
                Record.Set(IDMap[sensor.Identifier], sensor.Value);
            }
        }
    }
}
