# Make installer

## Requirements

Download and install [InstallForge](https://installforge.net/)

## Procedure to generate the installer

1. Build project with Unity in a *Build* folder
2. In the *Build* folder, create a *ThirdParty* folder and copy the
    - [Gstreamer Windows Runtime](
https://gstreamer.freedesktop.org/data/pkg/windows/1.24.9/msvc/gstreamer-1.0-msvc-x86_64-1.24.9.msi) 
    - [Visual C++ redistributable package](https://aka.ms/vs/17/release/vc_redist.x64.exe)
3. Launch InstallForge and open [install_config.ifp](../Installer/install_config.ifp)
4. In files section, make sure that everything is correct (especially the location of your Unity build)
5. Press F5 to generate the installer
