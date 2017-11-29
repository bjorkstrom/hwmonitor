using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace hwmonitor
{
    public static class Battery
    {
        static float minVoltage = 10.9F;
        static int numtimesForAverageVolt = 3;
        static Queue<float> voltages = new Queue<float>();

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
            voltages.Enqueue(voltageIn);
            if (voltages.Count > numtimesForAverageVolt)
                voltages.Dequeue();

            var AverageVolt = voltages.Sum() / voltages.Count;

            if (AverageVolt < minVoltage)
            {
                Shutdown(voltageIn);
            }
        }
    }
}
