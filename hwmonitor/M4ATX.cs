using System;
using LibUsbDotNet;
using LibUsbDotNet.Main;


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
            Console.WriteLine("M4ATX PSU device not found");
            return;
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

    public static string GetReport()
    {

        if (MyUsbDevice == null)
        {
            return "M4ATX PSU device not found";
        }

        string str = null;
        UsbEndpointWriter writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

        // write data, read data
        //int bytesWritten;

        // specify data to send
        ec = writer.Write(new byte[] { 0x81, 0x00 }, 5000, out bytesWritten);
        Console.WriteLine("\r\nDone!\r\n");

        if (ec != ErrorCode.None)
        {
            return "Bytes Write fail";
        }

        // open read endpoint 1.
        UsbEndpointReader reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

        // If the device hasn't sent data in the last 5 seconds,
        // a timeout error (ec = IoTimedOut) will occur.
        ec= reader.Read(readBuffer, 3000, out bytesRead);

        if (bytesRead == 0)
        {
            return "IoTimeOut Error";
        }

        str = string.Format("temperature {0}\n,Voltage on 12V rail {1}\n,Voltage on 3.3V{2}\n,Voltage on 5V rail{3}\n,Input voltage{4}\n,Ignition voltage{5}\n",
            readBuffer[12].ToString(),
            readBuffer[6].ToString(),
            readBuffer[4].ToString(),
            readBuffer[5].ToString(),
            readBuffer[2].ToString(),
            readBuffer[3].ToString());
        return str;
    }

}
