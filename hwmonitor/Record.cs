using System;
using System.Linq;

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
}
