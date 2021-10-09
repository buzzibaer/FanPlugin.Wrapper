# FanPlugin.Wrapper
3D holografic fan remote controll for virtual pinball

## What is this for? The Problem!
I bought a kindly cheap 3D holografic fan from china.

It has a Handy-App for remote controlling it via Wifi.

There is also a Windows App for ending videos for it and controlling it.

But i wanted to control it directly from my virtual pinball cabinet.

The idea ist that if i am going to load up a specific pinball table the fan has to select the corresponding video for the table.

So if you going to load up the table "DarkPrinzess" a topper video is shown on the fan for ambiente purposes.

## The Fan
I bought this device here: [AliExpress Link] (https://de.aliexpress.com/item/4000579865125.html?spm=a2g0s.9042311.0.0.659e4c4dMc6T5K)

![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install5.png)

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


## What does it do?
This small cluecode application is getting a pinball table and is than selection a corresponding pic / vid on the 3d holografic fan.

If you want to see it in action look here = https://youtu.be/gSEaMVhVHcs or here https://youtu.be/rK_Xfbv4QXQ
  
Or an update video with rom / pup events integration = https://youtu.be/YnqAcIEPP1M
  
## How does it work?
You can add this to your PupPack Folder for VPX.
It will then select a specific vid / pic on your fan for this table

## My PC will not connect automaticly to the FAN! Help!
Yes, this is happening due to the FAN will propagate a public WLAN.
Windows 10 will not connect automaticly to public wlan, even if autoconncet is enabled.
See:
![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install4.png)
Source = https://appuals.com/windows-10-will-not-connect-to-wifi-automatically/

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

Edit the Script for the bin file you want to select on your fan.

![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install9.png)

configure evnets for changing videos  
  
![explain pic](https://github.com/buzzibaer/FanPlugin.Wrapper/blob/main/docmedia/install9.png)

Enjoy!
