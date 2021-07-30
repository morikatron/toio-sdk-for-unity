# toio SDK for Unity Documentation

## Development Overview

### [1] Setup

1. Prepare the development environment ["Preparation"]. (preparation.md)
2. Create a new project in Unity and load toio SDK for Unity ["toio SDK for Unity Installation"](download_sdk.md)
3. Install optional functions ["Install BLE communication for Mac"](usage_macble.md)
<br><br>

### [2] Development

1. Check before development ["How to operate Simulator"](development_simulator.md).
2. Create a development folder under the Assets folder ["Script Dependencies"](development_basics.md#1-script-dependencies).
3. Create a new scene and load Simulator's Prefab ["Creating Scenes"](tutorials_basic.md#2-creating-scenes).
4. Create a program by playing it in Unity Editor and checking how it works in Simulator. (A)
<br><br>

### [3] Build

1. Build an app for a smart device and verify that it works on the device ["iOS Build"](build_ios.md). |["Android Build"](build_android.md) |["WebGL Build"](build_web.md) (B)
2. Repeat `(A)` `(B)` to finish the program.

<br>

## Tutorial

### Basics

- [1. Overview](tutorials_basic.md#1-overview)
- [2. Creating Scenes](tutorials_basic.md#2-creating-scenes)
- [3. Move](tutorials_basic.md#3-move)
- [4. Make a sound](tutorials_basic.md#4-make-a-sound)
- [5. Emit LED](tutorials_basic.md#5-emit-led)
- [6. Read toio ID(Position ID & Standard ID)](tutorials_basic.md#6-read-toio-idposition-id--standard-id)
- [7. Detect events (button, tilt, collision, coordinate and angle, Standard ID)](tutorials_basic.md#7-detect-events-button-tilt-collision-coordinate-and-angle-standard-id)
- [8. Move multiple Cubes](tutorials_basic.md#8-move-multiple-cubes)
- [9. Simplifying the source code using CubeManager class](tutorials_basic.md#9-simplifying-the-source-code-using-cubemanager-class)
- [10. Midway connection/disconnection](tutorials_basic.md#10-midway-connectiondisconnection)
- [11. Creating UI](tutorials_UI.md)

### Convenient movement functions - CubeHandle

- [1. Synchronized control of cubes using CubeManager](tutorials_cubehandle.md#1-synchronized-control-of-cubes-using-cubemanager)
- [2. CubeHandle](tutorials_cubehandle.md#2-cubehandle)
  - [2.1. CubeHandle's Move function and MoveRaw function](tutorials_cubehandle.md#21-cubehandles-move-function-and-moveraw-function)
  - [2.2. One-shot method to reduce the amount of communication with Cube](tutorials_cubehandle.md#22-one-shot-method-to-reduce-the-amount-of-communication-with-cube)
  - [2.3. Closed-Loop method to reach the specified coordinates/direction](tutorials_cubehandle.md#23-closed-loop-method-to-reach-the-specified-coordinatesdirection)
- [3. Follow TargetPole Demo](tutorials_cubehandle.md#3-follow-targetpole-demo)

### Group control - Navigator

- [1. CubeNavigator](tutorials_navigator.md#1-CubeNavigator)
  - [1.1. Using CubeManager with CubeNavigator](tutorials_navigator.md#11-using-cubemanager-with-cubenavigator)
    - [1.1.1. If you are controlling Cube asynchronously](tutorials_navigator.md#111-if-you-are-controlling-the-cube-asynchronously)
    - [1.1.2. When controlling Cube with synchronization](tutorials_navigator.md#112-want-to-control-cube-synchronously)
    - [1.1.3. Using CubeNavigator without CubeManager](tutorials_navigator.md#113-using-cubenavigator-without-cubemanager)
  - [1.2. Avoiding Collisions with CubeNavigator](tutorials_navigator.md#12-avoiding-collisions-with-cubenavigator)
    - [1.2.1. Navi2Target function to move to the target while avoiding collision](tutorials_navigator.md#121-navi2target-function-to-move-to-the-target-while-avoiding-collision)
    - [1.2.2. Move away from the target NaviAwayTarget function](tutorials_navigator.md#122-move-away-from-the-target-naviawaytarget-function)
  - [1.3. Population Control with Boids](tutorials_navigator.md#13-population-control-with-boids)
  - [1.4. Boids + Conflict Avoidance](tutorials_navigator.md#14-boids--conflict-avoidance)

<br>

## Sample

### Basics

- [Sample_Circling](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_Circling/)

  This is a sample to see how the behavior of many Cubes changes depending on the mode of CubeNavigator.

- [Sample_Cross](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_Cross/)

  This is a sample of how to use CubeNavigator to move multiple Cubes while avoiding collisions.

- [Sample_Sensor](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_Sensor/)

  This is a sample that detects each sensor value of Cube and displays it on UI.

- [Sample_Motor](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_Motor/)

  This is a sample of moving Cube using Cube.TargetMove and Cube.AccelerationMove.

- [Sample_VisualizeNavigator](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_VisualizeNavigator/)

  This is an example of visualizing the results of HLAvoid calculation of CubeNavigator and all defined walls.

### 応用

- [Sample_MultiMat](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_MultiMat/)

  This is a sample of using multiple mats together as one large mat.

- [Sample_Bluetooth](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_Bluetooth/)

  This is a sample of communicating with Cube by directly using BLE interface, which is a low-level module.

- [Sample_WebGL](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_WebGL/)

  This is a collection of sample web applications that run in a browser.

- [Sample_Scenes](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_Scenes/)

  This sample handles scene transitions while maintaining the connection with the real Cube and the correlated instance with Cube on Simulator.

<br>

## Technical Documents

### Diagram of the entire system

<p align="center">
<img src="./res/main/arch.png" width=550></img>
</p>

Explain from low level (left) to high level (right).
- Simulator: A simulator that can be run in Unity Editor
- BLE: A module for bluetooth communication with the real Cube on smart devices and the web
- Cube: A class that unifies both Simulator and the real world and handles Cube
- CubeHandle: A class that summarizes useful movement functions
- Navigator: A class that provides advanced group control
- CubeManager: A class that conveniently manages multiple cubes and various functions

### How to use

- [Cube Class](usage_cube.md)
- [Simulator](usage_simulator.md)
- [CubeHandle class](usage_cubehandle.md)
- [Navigator class](usage_navigator.md)

### Function Description

- [Cube Class](sys_cube.md)
- [BLE(Bluetooth Low Energy)](sys_ble.md)
- [Simulator](sys_simulator.md)
- [NNavigator class](sys_navigator.md)

<br>

## FAQ

- [FAQ](FAQ.md)
