This program monitors varius hardware releated metrics, such as temperature, power and voltage. The values
are fetched with 5 second intervalls and written to a file (~/Desktop/HardwareMonitorReport.txt).

INSTALLING

The libusb-win32 driver for M4 ATX PSU.

In order for this program to be able to communicate with the M4 ATX PSU over the USB interface, we need to override the M4 ATX default device driver with libusb-win32 driver.

 * start the zadig-2.3.exe program
 * goto Options menu man make sure 'List All Devices' is checked
 * pick the 'M4 ATX PSU' device in the drop down (USB ID 04D0 D001)
 * pick 'libusb-win32' under driver
 * click 'Replace Driver'

To be able to read all sensor values from the motherboard, this application need to run as Administrator.
