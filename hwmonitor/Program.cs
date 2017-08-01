using System;
using System.Diagnostics;
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
            record.Get(Record.DataPoint.M4ATXTemperature),

            record.Get(Record.DataPoint.CPUPackageTemperature),
            record.Get(Record.DataPoint.CPUCore0Temperature),
            record.Get(Record.DataPoint.CPUCore1Temperature),
            record.Get(Record.DataPoint.CPUCore2Temperature),
            record.Get(Record.DataPoint.CPUCore3Temperature),

            record.Get(Record.DataPoint.GPUCoreTemperature),

            record.Get(Record.DataPoint.CPUPackagePower),
            record.Get(Record.DataPoint.CPUCoresPower),
            record.Get(Record.DataPoint.CPUDRAMPower),

            record.Get(Record.DataPoint.GPUPower),

            record.Get(Record.DataPoint.M4ATXVoltageIn),
            record.Get(Record.DataPoint.M4ATXVoltageOn12V),
            record.Get(Record.DataPoint.M4ATXVoltageOn3V),
            record.Get(Record.DataPoint.M4ATXVoltageOn5V)
            );

        Log.WriteLine(line);
    }

    static void StartMSIAfterburner()
    {
        try
        {
            var psi = new ProcessStartInfo(@"C:\Program Files (x86)\MSI Afterburner\MSIAfterburner.exe");
            psi.UseShellExecute = false;
            Process.Start(psi);

        }
        catch (Exception e)
        {
            Log.Exception(e);
        }
    }


    static void Main(string[] args)
    {
        Init();

        /*
         * a hack to start MSI afterburner becouse we can't figure out how to do it
         * with windows task scheduler
         */
        StartMSIAfterburner();

        /* reuse same record object to save a bit on GC */
        Record record = new Record();
        while (true)
        {
            try
            {
                FetchAndLogRecord(record);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            Thread.Sleep(Log.ReportRate);
        }
    }
}
