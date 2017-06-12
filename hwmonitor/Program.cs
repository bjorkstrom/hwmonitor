using System;
using System.Threading;


class Program
{
    /* time between we fetch new hardware reports, in miliseconds */
    const int ReportRate = 5000;

    static void Init()
    {
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
            Console.WriteLine(report);
            Thread.Sleep(ReportRate);
        }
    }
}
