# FanPlugin.Wrapper
3D holografic fan remote controll for virtual pinball

## What is this for? The Problem!
I bought a kindly cheap 3D holografic fan from china.

It has a Handy-App for remote controlling it via Wifi.

There is also a Windows App for ending videos for it and controlling it.

But i wanted to control it directly from my virtual pinball cabinet.

The idea ist that if i am going to load up a specific pinball table the fan has to select the corresponding video for the table.

So if you going to load up the table "DarkPrinzess" a topper video is shown on the fan for ambiente purposes.


## What does it do?
This small cluecode application is getting a pinball table and is than selection a corresponding pic / vid on the 3d holografic fan.

If you want to see it in action look here = https://youtu.be/gSEaMVhVHcs or here https://youtu.be/rK_Xfbv4QXQ
  
Or an update video with rom / pup events integration = https://youtu.be/YnqAcIEPP1M
  
## How does it work?
You can add this to your PupPack Folder for VPX.
It will then select a specific vid / pic on your fan for this table

## The Fan
I bought this device here: [AliExpress Link] (https://de.aliexpress.com/item/4000579865125.html?spm=a2g0s.9042311.0.0.659e4c4dMc6T5K)

![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install5.png) (Not available any more with this link)

But it seems kindly generic, so maybe it works also for other models

Original Software for App and Desktop came from here >> https://huangbanjin.gitee.io/bergerh/

### Configuring the fan to static ip
this is needed if you want to have still internet for your cab

the fan is pushing a standard gateway to your wlan adapter via dhcp and this is a problem if you do have ethernet or a second wlan adapter up and running for internet acess.
we need to get rid of the gateway from the fan

![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install3.png)

Just disable DHCP on the WLAN Chip and set it to manual

The Fan has the IP = 192.168.4.1

Your WLAN Dongle / Net should have the IP = 192.168.4.2

Your Subnet = 255.255.255.0

Your Subnetmask = <EMPTY> Delete everything here

Change your setup accordingly and your internet will run like charm :)

### USB Wlan Dongle - Why using a separete WLAN Dongle

The China Fan is quite a cheap product an the software is shit.
Since the FAN is propagating a OPEN Wlan you have to connect to, you dont want to have this in yoour private network environment.
I bought a cheap usb wlan dongle for my cab windows pc and attaced this dongle exclusivly to the FAN.

Configuration of FAN and Dongle is described below.


## My PC will not connect automaticly to the FAN! Help!
Yes, this is happening due to the FAN will propagate a public WLAN.
Windows 10 will not connect automaticly to public wlan, even if autoconncet is enabled.
See:
> ![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install4.png)
  
Source = https://appuals.com/windows-10-will-not-connect-to-wifi-automatically/

## How to sort my files on the SD-Card / Problems with selection of file IDs

  The files on the SD card of your fan have to be unique and identified by the program.
  
  To avoid pitfalls, i recomend to go for a strict sort order.
  
  The FAN itself trys to sort the filenames bitwhise and alphabeticly.
  
  To keep a strict order i recommend to use 6 characters with numbers only.
  
  Example = 000001.bin
  
  Trailing zeros are neccassary due to bytewhise comparison.
  
![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install11.png)
  
## How to install

### Reg DLL
Download the latest DLL File from = https://github.com/buzzibaer/FanPlugin.Wrapper/releases
Put it into any folder you like (e.g. c:\FanPlugin\)

Open up a cmd in admin mode

![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install6.png)

find RegAsm.exe from your latest .NET on your Computer

![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install7.png)


Go into your folder were your dll is.
Open up a command shell command and register the DLL like:

![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install8.png)

'''
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe FanPlugin.Wrapper.dll /codebase
'''

### Install pupscript
Download the Pupscript here = https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/pupscript/pupscript_js.pup

Put it in your PupPack Root Folder of your choise.

#### Select the fan hardware version

At the top of `pupscript_js.pup`, set `FAN_HARDWARE_VERSION` to match your fan:

```js
var FAN_HARDWARE_VERSION = 2;
```

- `2` selects `Fan` (`FanPlugin.Wrapper.Fan`, Version 2 hardware). Its default endpoint is `192.168.4.1:5233`.
- `3` selects `FanV3` (`FanPlugin.Wrapper.FanV3`, Version 3 hardware). Its default endpoint is `192.168.4.1:5233`.
- `20320` selects `Fan20320` (`FanPlugin.Wrapper.Fan20320`), which uses the Android-app protocol at `192.168.4.1:20320`. It selects an existing file by the numeric ID in its six-digit filename: `playVideoWithId(5)` selects `000005.bin`. The wrapper sends the fan's internal file-list position; callers must not pass that position. Media upload is not implemented.

The PupScript uses the selected implementation for startup and all configured table/ROM events. Each class keeps its own version-specific default address and port. Both variants use a default connect timeout of 3000 ms and read/write timeout of 3000 ms. Leave optional overrides empty/zero to use the selected class defaults, or set them near the top of the script to override the endpoint and timeouts:

```js
var FAN_SERVER_IP_OVERRIDE = "192.168.4.1";
var FAN_SERVER_PORT_OVERRIDE = 5233;
var FAN_CONNECT_TIMEOUT_OVERRIDE = 3000;
var FAN_SOCKET_TIMEOUT_OVERRIDE = 3000;
```

Invalid video IDs (anything other than a decimal value from `0` to `99`) are rejected before the wrapper contacts the fan or updates playback history. Network failures and timeouts are returned to PupScript callers as error strings.

Both classes are included in the wrapper project; make sure the registered `FanPlugin.Wrapper.dll` is built from a version that includes `FanV3.cs` before selecting version `3`.

Edit the Script for the bin file you want to select on your fan.

![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install9.png)

configure evnets for changing videos  
  
![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install10.png)


### Ho to configure events

  The easiest way is to have a look into the events from any Puppack.
  There you can see what is already defined as events.
  Get your information from there:
  ![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install12.png)
  
  If you want to learn more about events of tables, you can always find them in Nailbusters documentation right here = https://www.nailbuster.com/wikipinup/doku.php?id=pup_capture
  
## Problems?

### Standalone UI

  I made a little Standalone UI for testing the Fan without anything else but just the fan.

  Have a look here >> https://github.com/buzzibaer/FanPlugin.Test/releases

  With that you can experiment if the funktions work well.
