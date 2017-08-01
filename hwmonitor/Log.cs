using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Log
{
    const string PrevReportFile = "HardwareMonitorReportPrev.txt";
    const string ReportFile = "HardwareMonitorReport.txt";

    /* time between we fetch new hardware reports, in miliseconds */
    public const int ReportRate = 5000;


    /*
     * Max length, in bytes, of the report file, before it is trimmed.
     *
     * We assume that we log around 256 bytes for each report line,
     * the formula below caclulates approx number of bytes needed to
     * store 3 hours of data.
     */
    const int MaxReportLength = 256 * (60 / (ReportRate / 1000)) * 60 * 3;


    static StreamWriter ReportStream;
    static KinesisLog.Send SendToKinesis;

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
    }

    public static void Init()
    {
        OpenReportStream();
        SendToKinesis = KinesisLog.Create();
    }

    public static void ToCloud(Record record)
    {
        SendToKinesis(record);
    }

    public static void WriteLine(string line)
    {
        /* write report to console for debugging purpuses */
        Console.WriteLine(line);

        /* write report to the file */
        ReportStream.WriteLine(line);
        ReportStream.Flush();
    }

    public static void Exception(Exception e)
    {
        WriteLine(e.ToString());
    }

}
