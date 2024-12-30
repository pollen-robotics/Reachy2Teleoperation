# Reachy2Teleoperation

Unity-based application that allows to control a Reachy 2 robot with a VR headset. The user documentation is available [here](https://docs.pollen-robotics.com/vr/introduction/introduction/).

## Requirements

The app should run with any VR headset compatible with Unity. It has been tested with the Oculus Quest 2 and 3. The Oculus Quest headsets needs the Oculus link for the app to work properly.

For any custom development, we recommend to use Unity LTS 2022.3, which has been used for development (*2022.3.20f1* and *2022.3.41f1*).

The project relies on GStreamer. It will be installed directly with the app if you use the installer (see below). Otherwise, please install the [Windows Runtime](
https://gstreamer.freedesktop.org/data/pkg/windows/1.24.8/msvc/gstreamer-1.0-msvc-x86_64-1.24.8.msi) (make sure you select the **complete** installation), and the [development files](https://gstreamer.freedesktop.org/data/pkg/windows/1.24.8/msvc/gstreamer-1.0-devel-msvc-x86_64-1.24.8.msi). 

<details>
<summary>Check that the environment variable PATH contains <i>C:\gstreamer\1.0\msvc_x86_64\bin</i> (default installation). </summary>

For that, look for “Edit the system environment variables” in the Windows search bar. Then, click on Environment variables. A new window shows up : double click on "Path" in the user variables. If you don't see the gstreamer variable, select "New" and add the pathway above. 
<img src="Docs/img/env_variables.jpg" alt="My cool logo"/>

</details>

Reboot after the installation. 

## Installation

### Using a [release build](https://github.com/pollen-robotics/Reachy2Teleoperation/releases) [recommended]

For the Oculus Quest 2 or 3, you may ask to join the list of beta users to install the app directly from the app store. Please contact us on our [discord channel](https://discord.com/channels/519098054377340948/991321051835404409)!

For Windows and Android platforms, the simplest way to use the application is to download a [release here](https://github.com/pollen-robotics/ReachyTeleoperation/releases) (*Assets* section). You can use the installer to install the application on your computer, as well as GStreamer, or download the zip file that contains the .exe to run. Your VR headset should be plugged in and ready to be used.


### From source

Clone the **master** branch of the repo. Make sure that Git LFS is enabled. If you want to contribue to the project please see the *Issues/Contribution* section.
```
git clone --recurse-submodules -b master https://github.com/pollen-robotics/Reachy2Teleoperation.git
```

Make sure you have [Git](https://git-scm.com/book/en/v2/Getting-Started-Installing-Git) installed on your computer.

In Unity Editor, check the settings are well set : go to Edit > Project Settings > XR Plugin Management, and check that your device is selected. 

## Usage

This is the quick-start documentation. For a detailed manual, please visit the [main documentation website](https://docs.pollen-robotics.com/vr/introduction/introduction/).

Teleoperating a robot takes place in three basic steps:

### 1. Connect to a robot

The first step is to select the robot you want to control. For that you'll need the IP address of the robot (please refer to the [robot documentation](https://docs.pollen-robotics.com/dashboard/introduction/first-connection/) for the first connection). Press *new robot button* and add your robot.

<p align="center"> 
    <img src="Docs/img/change_robot.jpg" alt="change robot" width='40%; margin-right: 10px;'/>
    <img src="Docs/img/select_robot.jpg" alt="select robot" width='40%'/>
</p>

Note that there's a built-in virtual robot for local testing of the application interface. 

### 2. Get ready for the teleoperation

This step checks that the connection to the robot is fine, and allows to set various parameters. Side menus (status, help, settings) can be opened by clicking on the related icons. You can change the different parameters according to what you want to do in teleoperation : you can set your height, navigation features, the gripper control, etc. 

Once you are ready, press the *Ready* button and then hold down the A button to take control of the robot.

*Please not that A and X refers to the buttons of the Oculus controllers. They may differ on your device.*

<p align="center"> 
    <img src="Docs/img/mirror1.png" alt="mirror1" height='200; margin-right: 10px;'/>
    <img src="Docs/img/mirror2.jpg" alt="mirror2" height='200; margin-right: 10px;'/>
    <img src="Docs/img/mirror3.png" alt="mirror3" height='200; margin-right: 10px;'/>
    <img src="Docs/img/mirror4.png" alt="mirror4" height='200'/>
</p>


### 3. Take control!

You are then in the teleoperation view, but can only use the head and the mobile base.
1. Check the robot surroudings to make sure there is no obstacle or people around. 
2. Use the mobile base to get to a more appropriate location to start if needed.
3. Finally press A to take control of the arms.

You are now controlling Reachy! Press and hold A to return to the previous step. 
<p align="center"> 
    <img src="Docs/img/teleop.png" alt="teleop_view" width='60%'/>
</p>

## Issues / Contributions

If you have any problem, you can create an issue on GitHub or send a message on our [forum](https://forum.pollen-robotics.com/c/users/vr-tele-operation/6). 

### Gstreamer Log files

The webRTC plugin is based on Gstreamer which ships with its own [logging system](https://gstreamer.freedesktop.org/documentation/tutorials/basic/debugging-tools.html?gi-language=c). To enable gstreamer logging, you need to set these two environment variables:
```
GST_DEBUG_FILE=C:\Users\<UserName>\gstreamer.log
GST_DEBUG=3
```
*Note that they are already set in the built version (see [Installer/launch.bat](Installer/launch.bat))*
