using System;
using System.IO;
using System.Threading;


class Program
{
    const string ReportFile = "HardwareMonitorReport.txt";

    static StreamWriter ReportStream;

    /* time between we fetch new hardware reports, in miliseconds */
    const int ReportRate = 5000;

    static void OpenReportStream()
    {
        var DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string path = Path.Combine(DesktopPath, ReportFile);
        ReportStream = new StreamWriter(path, true);
    }

    static void Init()
    {
        OpenReportStream();
        M4ATX.Init();
        Motherboard.Init();
    }

    static void Main(string[] args)
    {
        Init();

        while (true)
        {
            var report = M4ATX.GetReport() + "\n" +
                         Motherboard.GetReport();

            /* write report to console for debugging purpuses */
            Console.WriteLine(report);

            /* write report to the file */
            ReportStream.WriteLine(report);
            ReportStream.Flush();

            Thread.Sleep(ReportRate);
        }
    }
}
