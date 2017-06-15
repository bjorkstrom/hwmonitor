using System;
using OpenHardwareMonitor.Hardware;
using System.IO;
using System.Globalization;
using System.Collections;

class Motherboard
{
    public static Computer computer = new Computer();
    public static void Init()
    {
        computer.Open();
        computer.CPUEnabled = true;
        computer.GPUEnabled = true;
    }

    public static string GetReport()
    {
        ArrayList list = new ArrayList()
        {
            "/intelcpu/0/temperature/0",
            "/intelcpu/0/temperature/1",
            "/intelcpu/0/temperature/2",
            "/intelcpu/0/temperature/3",
            "/intelcpu/0/temperature/4",
            "/intelcpu/0/power/0",
            "/intelcpu/0/power/1",
            "/intelcpu/0/power/2",
            "/intelcpu/0/power/3",
            "/nvidiagpu/0/temperature/0"
        };

        string s_identifier;

        using (StringWriter w = new StringWriter(CultureInfo.InvariantCulture))
        {
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();
                w.WriteLine();
                w.WriteLine("Hardware name : " + hardware.Name);

                foreach (var sensor in hardware.Sensors)
                {
                    s_identifier = Convert.ToString(sensor.Identifier);
                    if (list.Contains(s_identifier))
                    {
                        w.WriteLine("Sensor name : " + sensor.Name +
                                    " Sensor type : " + sensor.SensorType +
                                    " Sensor value : " + sensor.Value);
                    }
                }
            }

            return w.ToString();
        }
    }
}
