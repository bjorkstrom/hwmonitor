using System;
using LibUsbDotNet;
using LibUsbDotNet.Main;

public class M4ATXException : Exception
{
    public M4ATXException(string message) : base(message) { }
}

class M4ATXDevice
{
    const int TIMEOUT = 500;
    const int VENDOR_ID = 0x04d8;
    const int PRODUCT_ID = 53249; //0xd001;

    static byte[] GetDiagnosticsCommand = new byte[] { 0x81, 0x00 };

    IUsbDevice UsbDev = null;
    UsbEndpointWriter Writer;
    UsbEndpointReader Reader;

    byte[] ReadBuffer = new byte[24];

    public float Temperature;
    public float VoltageIn;
    public float VoltageOn12V;
    public float VoltageOn3V;
    public float VoltageOn5V;

    IUsbDevice FindDevice()
    {
        /* Iterate over USB devices until we find the M4ATX device */
        UsbRegDeviceList allDevices = UsbDevice.AllLibUsbDevices;
        foreach (UsbRegistry usbRegistry in allDevices)
        {
            if (usbRegistry.Vid == VENDOR_ID & usbRegistry.Pid == PRODUCT_ID)
            {
                return (IUsbDevice)usbRegistry.Device;
            }
        }

        throw new M4ATXException("M4ATX USB device not found");
    }

    void Setup()
    {
        if (UsbDev != null)
        {
            return;
        }

        UsbDev = FindDevice();

        if (!UsbDev.SetConfiguration(1))
        {
            throw new M4ATXException("Failed to set device config");
        }

        if (!UsbDev.ClaimInterface(0))
        {
            throw new M4ATXException("Failed to claim interface #0");
        }

        if (!UsbDev.SetAltInterface(0))
        {
            throw new M4ATXException("Failed to set alternate interface to 0");
        }

        Writer = UsbDev.OpenEndpointWriter(WriteEndpointID.Ep01);
        Reader = UsbDev.OpenEndpointReader(ReadEndpointID.Ep01);
    }

    void Reset()
    {
        if (UsbDev == null)
        {
            /* USB device not initilized, nothing to reset */
            return;
        }

        Log.WriteLine("Resetting M4ATX device");
        UsbDev.ResetDevice();
        UsbDev = null;
    }

    ///
    /// <returns>
    ///   true if successfully updated values,
    ///   false on error and there are no new values
    /// </returns>
    ///
    public bool UpdateState()
    {
        try
        {
            Setup();

            int count;
            var ec = Writer.Write(GetDiagnosticsCommand, TIMEOUT, out count);
            if (ec != ErrorCode.None)
            {
                throw new M4ATXException("Write failed");
            }

            ec = Reader.Read(ReadBuffer, TIMEOUT, out count);
            if (ec != ErrorCode.None)
            {
                throw new M4ATXException("Read failed");
            }

            /* store temperature value */
            Temperature = ReadBuffer[12];

            /*
             * M4ATX reports voltage values in custom scales,
             * in order to utilize express as mush presision
             * as possible with 8-bit values.
             *
             * Apply rescale factors to convert to regular scale.
             */
            VoltageIn = (float)(ReadBuffer[2] * 0.1552);
            VoltageOn3V = (float)(ReadBuffer[4] * 0.0195);
            VoltageOn5V = (float)(ReadBuffer[5] * 0.0389);
            VoltageOn12V = (float)(ReadBuffer[6] * 0.1165);

            return true;

        }
        catch (M4ATXException e)
        {
            Log.Exception(e);
            Reset();
        }

        return false;
    }
}

public class M4ATX
{
    static M4ATXDevice M4Device;
    public static byte[] readBuffer = new byte[24];

    public static void Init()
    {
        M4Device = new M4ATXDevice();
    }

    public static void Update(Record Record)
    {
        if (!M4Device.UpdateState())
        {
            /* error, no M4ATX values this update cycle */

            Record[DataPoint.M4ATXTemperature] = null;
            Record[DataPoint.M4ATXVoltageIn] = null;
            Record[DataPoint.M4ATXVoltageOn12V] = null;
            Record[DataPoint.M4ATXVoltageOn3V] = null;
            Record[DataPoint.M4ATXVoltageOn5V] = null;

            return;
        }

        Record[DataPoint.M4ATXTemperature] = M4Device.Temperature;
        Record[DataPoint.M4ATXVoltageIn] = M4Device.VoltageIn;
        Record[DataPoint.M4ATXVoltageOn12V] = M4Device.VoltageOn12V;
        Record[DataPoint.M4ATXVoltageOn3V] = M4Device.VoltageOn3V;
        Record[DataPoint.M4ATXVoltageOn5V] = M4Device.VoltageOn5V;
    }
}
