using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public enum DataPoint
{
    M4ATXTemperature = 0,
    M4ATXVoltageIn,
    M4ATXVoltageOn12V,
    M4ATXVoltageOn3V,
    M4ATXVoltageOn5V,
    CPUCore0Temperature,
    CPUCore1Temperature,
    CPUCore2Temperature,
    CPUCore3Temperature,
    CPUPackageTemperature,
    GPUPower,
    GPUCoreTemperature,
    CPUPackagePower,
    CPUCoresPower,
    CPUDRAMPower
}

public class Record
{
    object[] vals = new object[Enum.GetNames(typeof(DataPoint)).Length];
    public object this[DataPoint index]
    {
        get { return vals[(int)index]; }
        set { vals[(int)index] = value; }
    }

    public String ToJson()
    {
        var dict = new Dictionary<string, Object>();

        /*
         * Iterate over all data point types and add
         * all non-null values to a dictionary.
         *
         * Use the enum name as a key.
         */
        foreach (var key in Enum.GetValues(typeof(DataPoint)).Cast<DataPoint>())
        {
            var val = vals[(int)key];
            if (val != null)
            {
                dict[key.ToString()] = vals[(int)key];
            }
        }

        return JsonConvert.SerializeObject(dict);
    }
}
