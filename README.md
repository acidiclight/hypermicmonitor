# hypermicmonitor
A basic program for turning Mic Monitoring on/off on a HyperX Cloud Alpha wireless headset, through the command-line, on any platform supporting HidApi and .NET.

### Why?
Like many of you, I want to be a daily-driving Linux user. But I'm visually impaired and struggle to both read and type. I have to use a headset with a mic on it that can play back what it hears through the headset as if it were a hearing aid. This lets me hear myself type and be more confident that the keys I'm pressing are actually being pressed, especially when I can't see my screen.

Many gaming headsets have this feature - some call it sidetone, others call it something else, but HyperX calls it mic monitoring. Typically, the mic moonitoring is done in hardware so as to not introduce latency. Otherwise the headset would speech-jam you, and you don't want that. However, most headsets, including my HyperX Cloud Alpha, only let you configure it in software.

And.....that software is usually some form of bloated Electron or UWP app that only runs on Windows and, if you're extremely lucky, macOS. And I've never been able to find anything in the open-source community that lets me turn mic monitoring on/off with my headset on Linux. So I wrote it.

### How?
By using Wireshark and USBPcap, I was able to do three packet captures of me using HyperX NGINUITY to flick the mic monitoring switch.

I noticed that HyperX's software would send two packets to the headset every time I flip the switch. The headset would also immediately reply with the same two packets just before the switch completed its animation. I also noticed that if the headset replied with something else, the switch would revert to its previous state.

Regardless of whether the switch is flicked on or off, both the headset and host PC send a 96-byte packet back and forth. I noticed that most of the packet's data was zeroed out, but the first 4 bytes were what actually mattered.

Flicking the switch on sends `0x21bb1001` to the headset, whereas flicking the switch off sends `0x21bb1000` to the headset. The only difference being one bit being on or off.

All this program does is accept a `true` or `false` as a command-line argument, and sends the corresponding byte sequence to the headset's receiver using `libusb` via LibUsbDotNet. This means this'll work on Windows and Linux provided you have `libusb` on your system, which you most likely do.

### Why .NET?
Because I know C#.

That's my business, not yours.

But hey, isn't it cool that I could do this in a language that used to only be used for shitty enterprise apps in WinForms and maybe ASP.NET?

### How do I use this?

1. Build the solution. I won't teach you how, you're in the weeds here. I kinda wrote this for me as a proof of concept anyway.
2. Plug your headset's receiver in.
3. Run the program. Pass `true` to turn mic monitoring on, `false` to turn it off.

It's genuinely that simple.

You could even put it in your desktop environment's startup list or your window manager's config. And you'd have a better experience than what HyperX ships.

### NOTE ABOUT LINUX
I initially wrote this on Windows, and it worked fine there. I assumed that it'd just work fine on Linux as well. But that's not the case - it requires some additional configuration.

You need to add some udev rules to your system that allow non-root users to write to the headset's HID interface. If you're feeling lazy, just copy the `70-hypermicmonitor-hid.rules` file to `/etc/udev/rules.d` and reboot your system. If you're feeling less lazy, look at the contents of the file and see what it does.

### Gotchas
This only works with one single headset, the one I have. It's a HyperX Cloud Alpha Wireless, but past that... I don't know what you have. But if you know the Vendor ID and Product ID of your headset's receiver, and it's also a HyperX headset and you know it has mic monitoring support, maybe you can adapt this code to work. If the IDs match what's already in the code, then you have the same headset I do anyway.

The program uses the receiver's vendor ID and product ID to find the receiver. If it isn't plugged in, or otherwise can't be found, you'll get an error telling you so.

At least with my headset, power-cycling it will have it forget the mic monitoring setting. You'll have to reset it every time you turn the headset on. It also may or may not persist across PC reboots, which is why I designed this to be usable in a login script.

Also, keep in mind this isn't a full driver for the headset. It just controls mic monitoring.

### Where are the packet captures?
On my computer. Remember that this is a headset with a microphone on it, and I was speaking into it and watching Brodie Robertson videos while doing the captures. Plus I have other devices hooked up to my system and I don't know what else got captured. There's probably at least some sensitive data in them, so I'm not taking that risk by posting 'em online. If you want packet captures, you're going to have to get your own. Sorry!
