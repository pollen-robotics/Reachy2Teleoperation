# Reachy2Teleoperation

Unity-based application that allows to control a Reachy 2 robot with a VR headset. The user documentation is available [here](https://docs.pollen-robotics.com/vr/introduction/introduction/).

## Requirements

The app should run with any VR headset compatible with Unity. It has been tested with the Oculus Quest 2 and 3. The Oculus Quest headsets can be used in standalone mode or with the Oculus link.

For any custom development we recommend to use Unity LTS 2022.3, which has been used for development.
The project relies on GStreamer. Please install the [Windows Runtime](
https://gstreamer.freedesktop.org/data/pkg/windows/1.24.0/msvc/gstreamer-1.0-msvc-x86_64-1.24.0.msi) (complete install), and the [development files](https://gstreamer.freedesktop.org/data/pkg/windows/1.24.2/msvc/gstreamer-1.0-devel-msvc-x86_64-1.24.2.msi). Check that the environment variable PATH contains *C:\gstreamer\1.0\msvc_x86_64\bin* (default installation). Reboot after the installation.

## Installation

### Using a [release build](https://github.com/pollen-robotics/ReachyTeleoperation/releases) [recommended]

For the Oculus Quest 2 or 3, you may ask to join the list of beta users to install the app directly from the app store. Please contact us on our [discord channel](https://discord.com/channels/519098054377340948/991321051835404409)!

For Windows and Android platforms, the simplest way to use the application is to download a [release here](https://github.com/pollen-robotics/ReachyTeleoperation/releases) (*Assets* section). The Windows package is a zip file that contains the .exe to run. Your VR headset should be plugged in and ready to be used. The Android package is an *.apk that should be installed on your device.


### From source

Clone the **main** branch of the repo. Make sure that git lfs is enabled. If you want to contribue to the project please see the *Issues/Contribution* section.
```
git clone -b main https://github.com/pollen-robotics/Reachy2Teleoperation.git
```

Make sure you have [Git](https://git-scm.com/book/en/v2/Getting-Started-Installing-Git) installed on your computer.

## Usage

This is the quick-start documentation. For a detailed manual, please visit the [main documentation website](https://docs.pollen-robotics.com/vr/introduction/introduction/).

Teleoperating a robot takes place in three basic steps:

### 1. Connect to a robot

The first step is to select the robot you want to control. For that you'll need the ip address of the robot (please refer to the [robot documentation](https://docs.pollen-robotics.com/dashboard/introduction/first-connection/) for the first connection), or the ip address of the computer running the Unity simulator. Press *new robot button* and add your robot.

![alt text](Docs/img/connection.jpg)

Note that there is a built-in virtual robot for local testing of the application (*not available yet*).

### 2. Get ready for the teleoperation

This step checks that the connection to the (virtual) robot is fine, and allows to set various parameters. Side menus (status, help, settings) can be opened by clicking on the related icons.

Get familiar with the controls of the robot. Press X to play with the emotions or change the gripper grasping mode. (*not available yet*)  
Once you are ready, press the *Ready* button and button A to take control of the robot.

*Please not that A and X refers to the buttons of the Oculus controllers. They may differ on your device.*

![alt text](Docs/img/mirror.jpg)

### 3. Take control!

You are then in the teleoperation view, but can only use the head and the mobile base.
1. Check the robot surroudings to make sure there is no obstacle or people around. 
2. Use the mobile base to get to a more appropriate location to start if needed.
3. Finally press A to take control of the arms.

You are now controlling Reachy! Press and hold A to return to the previous step. 

![alt text](Docs/img/teleop.jpg)

## Issues / Contributions

If you have any problem, you can create an issue or chat with us on our [discord server](https://discord.com/channels/519098054377340948/991321051835404409). 

### Gstreamer Log files

The webRTC plugin is based on gstreamer which ships with its own [logging system](https://gstreamer.freedesktop.org/documentation/tutorials/basic/debugging-tools.html?gi-language=c). To enable gstreamer logging, you need to set these two environment variables:
```
GST_DEBUG_FILE=C:\Users\<UserName>\gstreamer.log
GST_DEBUG=3
```
*Note that they are already set in the built version (see [Installer/launch.bat](Installer/launch.bat))*