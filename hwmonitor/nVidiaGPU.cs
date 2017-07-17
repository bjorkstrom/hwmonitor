using System;
using System.Runtime.InteropServices;
using System.Threading;

///
/// Use the NVML API to read GPU power
///
class nVidiaGPU
{
    [DllImport("kernel32")]
    public static extern bool SetDllDirectory(string lpPathName);

    ///
    /// Simle NVML API 'wrapper', for parts we are using
    ///
    enum nvmlReturn : int
    {
        NVML_SUCCESS = 0,
        /* .... */
    }

    [DllImport("nvml")]
    static extern nvmlReturn nvmlInit();

    [DllImport("nvml")]
    static extern nvmlReturn nvmlDeviceGetHandleByIndex(uint index, out IntPtr device);

    [DllImport("nvml")]
    static extern nvmlReturn nvmlDeviceGetPowerUsage(IntPtr device, out uint power);

    static IntPtr GPUDevice;

    public static void Init()
    {
        /*
         * Init NVML API and get handle to GPU #0
         */

        /* make sure we can find the nvml.dll */
        /* TODO: check that file C:\Program Files\NVIDIA Corporation\NVSMI\nvml.dll exists */
        SetDllDirectory(@"C:\Program Files\NVIDIA Corporation\NVSMI");

        nvmlReturn ret;
        if (nvmlReturn.NVML_SUCCESS != (ret = nvmlInit()))
        {
            throw new Exception("nvmlInit() failed " + ret);
        }

        /*
         * here we assume that there is only 1 nVIDIA GPU available
         */
        if (nvmlReturn.NVML_SUCCESS != (ret = nvmlDeviceGetHandleByIndex(0, out GPUDevice)))
        {
            throw new Exception("nvmlDeviceGetHandleByIndex() failed " + ret);
        }
    }

    public static void Update(Record Record)
    {
        uint power;
        nvmlReturn ret;
        if (nvmlReturn.NVML_SUCCESS != (ret = nvmlDeviceGetPowerUsage(GPUDevice, out power)))
        {
            throw new Exception("nvmlDeviceGetPowerUsage() failed " + ret);
        }

        Record.Set(Record.DataPoint.GPUPower,
                   ((float)power)/1000); /* convert from miliwatts to watts */
    }
}
