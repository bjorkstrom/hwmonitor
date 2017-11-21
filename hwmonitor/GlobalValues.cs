using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hwmonitor
{
    public static class GlobalValues
    {
        static public float minVoltage = 10.9F;
        static public int numtimesForAverageVolt = 3;
        static public int NumtimesCheckedVolt { get; set; } 
        static public float AverageVolt {  get; set; }
        static public float[] voltages = new float[numtimesForAverageVolt];
        static public float[] Voltages { get { return voltages; } }
    }
}
