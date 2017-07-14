using System;
using LibUsbDotNet;
using LibUsbDotNet.Main;


public class M4ATXDeviceNotFound : Exception
{
    public M4ATXDeviceNotFound() : base("M4ATX device not found") { }
}

class M4ATX
{
    public static readonly int VendorID = 0x04d8;
    public static readonly int ProductID = 0xd001;
    public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(VendorID, ProductID);
    public static int bytesWritten;
    public static UsbDevice MyUsbDevice;
    public static byte[] readBuffer = new byte[24];
    public static ErrorCode ec = ErrorCode.None;
    public static int bytesRead;
    public static void Init()
    {
        // Find M4ATX PSU Device
        //MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder); /* Does not work for whatever reason */

        /* Iterate over USB devices until we find the M4ATX device */
        UsbRegDeviceList allDevices = UsbDevice.AllDevices;
        foreach (UsbRegistry usbRegistry in allDevices)
        {
            if (usbRegistry.Open(out MyUsbDevice))
            {
                var vid = MyUsbDevice.Info.Descriptor.VendorID;
                var pid = MyUsbDevice.Info.Descriptor.ProductID;
                if (MyUsbDevice.Info.Descriptor.VendorID == VendorID &&
                    MyUsbDevice.Info.Descriptor.ProductID == ProductID)
                {
                    /* bingo */
                    break;
                }
            }
        }

        if (MyUsbDevice == null)
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
        if (MyUsbDevice == null)
        {
            /* we failed to connect to M4ATX device, nothing to do here */
            return;
        }

        UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

        // specify data to send
        ec = writer.Write(new byte[] { 0x81, 0x00 }, 5000, out bytesWritten);

        if (ec != ErrorCode.None)
        {
            throw new Exception("M4ATX: Error sending command: " + ec);
        }

        // open read endpoint 1.
        UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

        // If the device hasn't sent data in the last 5 seconds,
        // a timeout error (ec = IoTimedOut) will occur.
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
