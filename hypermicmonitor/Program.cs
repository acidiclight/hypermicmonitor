using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;

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
        using (var context = new UsbContext())
        {
            context.SetDebugLevel(LogLevel.Info);
            using var devices = context.List();

            var headsetReceiver = devices.FirstOrDefault(x => x.ProductId == productId && x.VendorId == vendorId);

            if (headsetReceiver == null)
            {
                Console.WriteLine(
                    "Could not find the HyperX Cloud Alpha headset's wireless receiver! Ensure that it is connected and that your headset is on.");
                Environment.Exit(1);
            }

            headsetReceiver.Open();
            
            Console.WriteLine($"Located device: {headsetReceiver.Info.Manufacturer} {headsetReceiver.Info.Product}");

            headsetReceiver.ClaimInterface(headsetReceiver.Configs[0].Interfaces[6].Number);

            var writeEndpoint = headsetReceiver.OpenEndpointWriter(WriteEndpointID.Ep04);
            var readEndpoint = headsetReceiver.OpenEndpointReader(ReadEndpointID.Ep04);
            

            var buffer = new byte[96];
            
            // first 3 bytes are static
            buffer[0] = 0x21;
            buffer[1] = 0xbb;
            buffer[2] = 0x10;
            
            // fourth is whether we want mic monitoring on or off
            buffer[3] = turnOn ? (byte) 0x01 : (byte) 0x00;

            var writeError = writeEndpoint.Write(buffer, timeout, out int what);
            if (writeError != Error.Success)
            {
                Console.WriteLine("Failed to write to device");
                Environment.Exit(2);
            }

            var readBuffer = new byte[96];
            var readError = readEndpoint.Read(readBuffer, timeout, out int whatAgain);

            Console.WriteLine("Mic monitoring is now {0}.", turnOn ? "on" : "off");
        }
    }
}