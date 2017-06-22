using System;
using System.IO;
using System.Threading;


class Program
{
    const string PrevReportFile = "HardwareMonitorReportPrev.txt";
    const string ReportFile = "HardwareMonitorReport.txt";
    /* time between we fetch new hardware reports, in miliseconds */
    const int ReportRate = 5000;


    /*
     * Max length, in bytes, of the report file, before it is trimmed.
     *
     * We assume that we log around 256 bytes for each report line,
     * the formula below caclulates approx number of bytes needed to
     * store 3 hours of data.
     */
    const int MaxReportLength = 256 * (60/(ReportRate/1000)) * 60 * 3;


    static StreamWriter ReportStream;

    /*
     * Make sure we don't generate infinite large report file
     * by trimming away the oldest data when we hit a max length.
     *
     * We perform trimming by keeping around two report files.
     *
     * Current and Previous report. If Current goes over the size
     * limit, move Current to Previous, possibly overwriting the
     * old Previous.
     */
    static void TrimReportFile(string path)
    {
        if (!File.Exists(path))
        {
            /* report file does not exist, nothing to trim */
            return;
        }

        var len = new FileInfo(path).Length;
        if (len < MaxReportLength)
        {
            /* no trimming nessesary, do nothing */
            return;
        }

        /*
         * do the trimming operation
         */
        var DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var prev = Path.Combine(DesktopPath, PrevReportFile);

        /* delete old previous */
        if (File.Exists(prev))
        {
            File.Delete(prev);
        }
        File.Move(path, prev);
    }

    static void OpenReportStream()
    {
        var DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string path = Path.Combine(DesktopPath, ReportFile);
        TrimReportFile(path);
        ReportStream = new StreamWriter(path, true);

        WriteLine(
        "--------------------+-------------+---------------------------------------+-------------+-------------------------------+------------" + Environment.NewLine +
        "     Timestamp      |  M4ATX PSU  |           CPU Temperature             |     GPU     |           CPU Power           |  M4ATX PSU" + Environment.NewLine +
        "  (UTC time zone)   | temperature |   PKG   Core0  Core1  Core02  Core03  | temperature |     PKG    Cores     DRAM     | Voltage IN" + Environment.NewLine +
        "--------------------+-------------+---------------------------------------+-------------+-------------------------------+------------"
                  );
    }

    static void WriteLine(string line)
    {
        /* write report to console for debugging purpuses */
        Console.WriteLine(line);

        /* write report to the file */
        ReportStream.WriteLine(line);
        ReportStream.Flush();
    }

    static void LogException(Exception e)
    {
        WriteLine(e.ToString());
    }

    static void Init()
    {
        OpenReportStream();
        try
        {
            M4ATX.Init();
        }
        catch (Exception e)
        {
            /*
             * the M4ATX USB interface is a bit unstable,
             * handle the case we can't connect to it
             * by logging an error and continuing without it
             */
            LogException(e);
        }
        Motherboard.Init();
    }

    static void FetchAndLogRecord(Record record)
    {
        string line;
        try
        {
            M4ATX.Update(record);
        }
        catch (Exception e)
        {
            LogException(e);
        }

        Motherboard.Update(record);

        line = string.Format(
"{0} |     {1}\x00B0     |    {2}\x00B0    {3}\x00B0    {4}\x00B0    {5}\x00B0    {6}\x00B0    |     {7}\x00B0     |    {8,4:#0.0}W    {9,4:#0.0}W    {10,4:#0.0}W    |   {11:#0.0}V",

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
            record.Get(Record.DataPoint.M4ATXVoltageIn)
            );

        WriteLine(line);

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
            }
            catch (Exception e)
            {
                LogException(e);
            }
            Thread.Sleep(ReportRate);
        }
    }
}
