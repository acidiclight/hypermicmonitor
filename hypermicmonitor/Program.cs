using HidApi;

namespace HyperMicMonitor;

public class Program
{
    private const int productId = 0x098D;
    private const int vendorId = 0x03F0;
    private const int timeout = 3000;

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            Environment.Exit(1);
        }

        if (!bool.TryParse(args[0], out var turnOn))
        {
            PrintUsage();
            Environment.Exit(1);
        }
        
        SetMicMonitoring(turnOn);
    }

    public static void PrintUsage()
    {
        var exe = AppDomain.CurrentDomain.FriendlyName;
        Console.WriteLine($"Usage: {exe} <true|false>");
        Console.WriteLine();
        Console.WriteLine(
            @"This program is extremely simple. It uses LibUSB to communicate with HyperX Cloud Alpha headsets to
turn on/off their hardware mic-monitoring feature.

Pass 'true' to turn it on.

Pass 'false' to turn it off.

It's that simple. Because I can't be arsed to do more packet captures. <3

More info: https://acidiclight.dev/my-work/hypermicmonitor");
    }

    public static void SetMicMonitoring(bool turnOn)
    {
        var deviceInfo = Hid.Enumerate(vendorId, productId).FirstOrDefault();
        if (deviceInfo == null)
        {
            Console.WriteLine(
                "Could not find a compatible HyperX Cloud Alpha Wireless USB receiver! Please make sure that it is plugged in.");
            return;
        }

        Console.WriteLine($"Found {deviceInfo.ManufacturerString} {deviceInfo.ProductString}");

        Device? connection = null;

        try
        {
            connection = deviceInfo.ConnectToDevice();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to the HID device: {ex.Message} - ensure you have the correct privileges, or that you have set up the udev rules if you're on Linux.");
            return;
        }
        
        Console.WriteLine("Connected.");

        Span<byte> packet = stackalloc byte[96];
        
        // first 3 bytes are static
        packet[0] = 0x21;
        packet[1] = 0xbb;
        packet[2] = 0x10;
            
        // fourth is whether we want mic monitoring on or off
        packet[3] = turnOn ? (byte) 0x01 : (byte) 0x00;
        
        connection.Write(packet);

        Console.WriteLine("Mic monitoring is now {0}.", turnOn ? "on" : "off");

        connection.Dispose();
    }
}