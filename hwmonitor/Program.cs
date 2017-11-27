using System;
using System.Threading;

class Program
{
    static void Init()
    {
        Log.Init();
        M4ATX.Init();
        Motherboard.Init();
        nVidiaGPU.Init();

        Log.WriteLine(
"--------------------+-------------+---------------------------------------+-------------+-------------------------------+-------+---------------------------------" + Environment.NewLine +
"     Timestamp      |  M4ATX PSU  |           CPU Temperature             |     GPU     |           CPU Power           |  GPU  |           M4ATX PSU Voltage         " + Environment.NewLine +
"  (UTC time zone)   | temperature |   PKG   Core0  Core1  Core02  Core03  | temperature |     PKG    Cores     DRAM     | power |   In       12V      3V        5V" + Environment.NewLine +
"--------------------+-------------+---------------------------------------+-------------+-------------------------------+-------+---------------------------------");

    }

    static void FetchAndLogRecord(Record record)
    {
        string line;

        M4ATX.Update(record);
        Motherboard.Update(record);
        nVidiaGPU.Update(record);

        Log.ToCloud(record);

        line = string.Format(
            "{0} |     {1}\x00B0     |    {2}\x00B0    {3}\x00B0    {4}\x00B0    {5}\x00B0    {6}\x00B0    |" +
            "     {7}\x00B0     |    {8,4:#0.0}W    {9,4:#0.0}W    {10,4:#0.0}W    | {11,4:#0.0}W |" +
            "   {12,4:#0.0}V    {13,4:#0.0}V    {14,4:#0.0}V    {15,4:#0.0}V",


            DateTime.UtcNow,
           
            record[DataPoint.M4ATXTemperature],
            record[DataPoint.CPUPackageTemperature],
            record[DataPoint.CPUCore0Temperature],
            record[DataPoint.CPUCore1Temperature],
            record[DataPoint.CPUCore2Temperature],
            record[DataPoint.CPUCore3Temperature],
            record[DataPoint.GPUCoreTemperature],
            record[DataPoint.CPUPackagePower],
            record[DataPoint.CPUCoresPower],
            record[DataPoint.CPUDRAMPower],
            record[DataPoint.GPUPower],
            record[DataPoint.M4ATXVoltageIn],
            record[DataPoint.M4ATXVoltageOn12V],
            record[DataPoint.M4ATXVoltageOn3V],
            record[DataPoint.M4ATXVoltageOn5V]
            );
      
        
        Log.WriteLine(line);
    }

    static void Main(string[] args)
    {
        Init();

        /* reuse same record object to save a bit on GC */
        Record record = new Record();
        while (true)
        {
            try
            {
                FetchAndLogRecord(record);
                Battery.CheckLevel((float)record[DataPoint.M4ATXVoltageIn]);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            Thread.Sleep(Log.ReportRate);
        }
    }
}
