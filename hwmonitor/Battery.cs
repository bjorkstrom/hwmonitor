using System.Diagnostics;
using System.Linq;

public static class Battery
{
    static float minVoltage = 10.9F;
    static int numtimesForAverageVolt = 3;
    static int NumtimesCheckedVolt { get; set; }

    static float[] voltages = new float[numtimesForAverageVolt];
    static float[] Voltages { get { return voltages; } }

    ///
    /// Log that we are shutting down and issue shutdown command
    ///
    static void Shutdown(float voltageIn)
    {
        var msg = string.Format(
            "Battery voltage {0} below minimal threshold {1}, " +
            "issuing a shutdown command", voltageIn, minVoltage);
        Log.WriteLine(msg);
        Process.Start("shutdown", "-s -t 30");
    }

    public static void CheckLevel(float voltageIn)
    {
        if (NumtimesCheckedVolt != numtimesForAverageVolt)
        {
            Voltages[NumtimesCheckedVolt] = voltageIn;
            NumtimesCheckedVolt++;
            return;
        }

        var AverageVolt = Voltages.Sum() / numtimesForAverageVolt;
        if (AverageVolt < minVoltage)
        {
            Shutdown(voltageIn);
        }
        NumtimesCheckedVolt = 0;
    }
}
