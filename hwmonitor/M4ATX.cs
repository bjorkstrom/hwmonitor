using System;
using LibUsbDotNet;
using LibUsbDotNet.Main;


public class M4ATXDeviceNotFound : Exception
{
    public M4ATXDeviceNotFound() : base("M4ATX device not found") { }
}

public class M4ATXReadError : Exception
{
    public M4ATXReadError(ErrorCode err) : base("Error reading from USB: " + err) { }
}


class M4ATX
{
    public static readonly int VendorID = 0x04d8;
    public static readonly int ProductID = 0xd001;
    public static int bytesWritten;
    public static UsbDevice MyUsbDevice;
    public static byte[] readBuffer = new byte[24];
    public static byte[] command = new byte[] { 0x81, 0x00 };
    public static ErrorCode ec = ErrorCode.None;
    public static int bytesRead;
    public static void Init()
    {
        /*
         * Find M4ATX PSU Device
         */
        //UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(VendorID, ProductID & 0xffff);
        //MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder); /* Does not work for whatever reason */

        if (MyUsbDevice != null && MyUsbDevice.IsOpen)
        {
            MyUsbDevice.Close();
        }

        bool DeviceFound = false;

        /* Iterate over USB devices until we find the M4ATX device */
        UsbRegDeviceList allDevices = UsbDevice.AllDevices;
        foreach (UsbRegistry usbRegistry in allDevices)
        {
            if (usbRegistry.Open(out MyUsbDevice))
            {
                if (MyUsbDevice.Info.Descriptor.VendorID == VendorID &&
                    /* not sure why we must mask out higher bits, TODO investigate */
                    (MyUsbDevice.Info.Descriptor.ProductID & 0xffff) == ProductID)
                {
                    /* bingo */
                    DeviceFound = true;
                    break;
                }
            }
        }

        if (!DeviceFound)
        {
            throw new M4ATXDeviceNotFound();
        }

        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
        if (!ReferenceEquals(wholeUsbDevice, null))
        {
            /*
             * This is a "whole" USB device. Before it can be used,
             * the desired configuration and interface must be selected.
             */

            /* Select config #1 */
            wholeUsbDevice.SetConfiguration(1);

            /* Claim interface #0 */
            wholeUsbDevice.ClaimInterface(1);
        }
    }

    public static void Update(Record Record)
    {
        /*
         * reset M4ATX values, in case we fail to read new values,
         * so we don't log old values
         */
        Record.Set(Record.DataPoint.M4ATXTemperature, null);
        Record.Set(Record.DataPoint.M4ATXVoltageIn, null);
        Record.Set(Record.DataPoint.M4ATXVoltageOn12V, null);
        Record.Set(Record.DataPoint.M4ATXVoltageOn3V, null);
        Record.Set(Record.DataPoint.M4ATXVoltageOn5V, null);

        /*
         * If we lost connection to the device in last
         * update cycle, try to reconnect
         */
        if (MyUsbDevice == null)
        {
            Init();
        }

        UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

        /* specify data to send */
        ec = writer.Write(command, 5000, out bytesWritten);

        if (ec != ErrorCode.None)
        {
            /* try to reconnect on next update cycle */
            MyUsbDevice = null;
            throw new M4ATXReadError(ec);
        }

        /* open read endpoint 1 */
        UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

        /*
         * If the device hasn't sent data in the last 5 seconds
         * a timeout error (ec = IoTimedOut) will occur
         */
        reader.Read(readBuffer, 3000, out bytesRead);

        if (ec != ErrorCode.None || bytesRead != readBuffer.Length)
        {
            var msg = string.Format("M4ATX: Error reading result, error {0}, got {1} bytes",
                 ec, bytesRead);
            throw new Exception(msg);
        }

        Record.Set(Record.DataPoint.M4ATXTemperature, (float)readBuffer[12]);
        Record.Set(Record.DataPoint.M4ATXVoltageIn,(float) (readBuffer[2] * 0.1552));
        Record.Set(Record.DataPoint.M4ATXVoltageOn12V, (float)(readBuffer[6] * 0.1165));
        Record.Set(Record.DataPoint.M4ATXVoltageOn3V, (float)(readBuffer[4] * 0.0195));
        Record.Set(Record.DataPoint.M4ATXVoltageOn5V, (float)(readBuffer[5] * 0.0389));
    }
}
