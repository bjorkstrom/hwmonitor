using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class Record
{
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
        GPUCoreTemperature,
        CPUPackagePower,
        CPUCoresPower,
        CPUDRAMPower
    }

    /* this magic gives us number of enums defined */
    Object[] Values = new Object[Enum.GetValues(typeof(DataPoint)).Cast<int>().Max() + 1];

    public void Set(DataPoint Type, Object Value)
    {
        Values[(int)Type] = Value;
    }

    public Object Get(DataPoint Type)
    {
        return Values[(int)Type];
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
            var val = Values[(int)key];
            if (val != null)
            {
                dict[key.ToString()] = Values[(int)key];
            }
        }

        return JsonConvert.SerializeObject(dict);
    }
}
