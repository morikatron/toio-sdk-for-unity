# Install toio SDK for Unity

## Unity Project Creation

Open Unity Hub and click [New] in the upper right corner.

<img width=500 src="res/download_sdk/new_project1.png">

When the project creation screen opens, set the following settings.

- Template: 3D
- Project Name: Anything is possible (in this case New toio Project)
- Save in: Anything is possible (in this case, Desktop)

After completing the settings, click [Create].

<img width=500 src="res/download_sdk/new_project2.png">

Make sure that Unity is up and running.

## Install UniTask

toio SDK for Unity uses Unity-optimized asynchronous processing library [UniTask](https://github.com/Cysharp/UniTask) . Therefore, you need to install UniTask (2.1.0 or later) in this Unity project.<br>

To install UniTask, follow the steps below (as of September 1, 2021; Unity 2020.3 LTS).
1. In Unity, go to the [Window] menu and select [Package Manager] to open the Package Manager.
1. Click the [+] icon and select [Add package from git URL...]
1. Add https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask

The following document contains installation instructions and screenshots of UniTask for your reference.
* [Description of the UniTask Github repository](https://github.com/Cysharp/UniTask#install-via-git-url)
* You can also refer to Unity manual ["Installing from a Git URL"](https://docs.unity3d.com/2020.3/Documentation/Manual/upm-ui-giturl.html).

## Adding the SDK

### 1. download unitypackage
Open [Assets] of the latest release version of [Click here](https://github.com/morikatron/toio-sdk-for-unity/releases/), find and download [toio-sdk-for-unity.unitypackage].

### 2. Drag and drop to Unity project
After opening the downloaded folder, drag and drop **[toio-sdk-for-unity.unitypackage]** into the **[Assets]** folder.

<img src="res/download_sdk/import_sdk.png">

### 3.Run samples and check operation
Once loaded into your Unity project, navigate to the `Assets/toio-sdk/Tutorials/1.Basic/0.BasicScene/` folder and double-click on `0.BasicScene scene file` to open it.

Once the scene file is open, click on the Play button at the top of the editor.

<img src="res/download_sdk/play.png">

 Make sure that the sample works (on Simulator) as shown below.
 To connect to the real toio™ Core Cube, please refer to [docs_EN/README.md "Build"](README.md#3-build).

<img src="res/download_sdk/sample.gif">

This completes the installation of toio SDK for Unity. There are various tutorials available for this SDK. For more information about the tutorials, please refer to [Tutorial (Basic)](tutorials_basic.md) .