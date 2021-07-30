# toio SDK for Unity

## Overview

**toio SDK for Unity** is a development environment for controlling toio™ Core Cube (hereafter referred to as Cubes) in Unity.

The toio SDK for Unity makes it easy to develop applications that use Cubes. In addition, the simulator function allows you to check the operation of the cube on the screen without using the actual device, so you can develop quickly and efficiently.


<p align="center">
<img src="./docs/res/main/overview.gif" width=720></img>
</p>

<details>
<summary>One of the key features of the toio SDK for Unity is that it allows you to run both the simulator and the real Cube with a single source code (click to expand), as shown above. </summary>

```C#
using UnityEngine;
using toio;

public class Hello_Toio : MonoBehaviour
{
    CubeManager cubeManager;
    Cube cube;

    async void Start()
    {
        // create a cube manager
        cubeManager = new CubeManager();
        // connect to the nearest cube
        cube = await cubeManager.SingleConnect();
    }

    void Update()
    {
        // check connection status and order interval
        if(cubeManager.IsControllable(cube))
        {
            cube.Move(100, 70, 200);
            //         |    |   `--- duration [ms]
            //         |    `------- right motor speed
            //         `------------ left motor speed
        }
    }
}
```

</details>


## Feature list

- Simulators
  - Ability to move Cubes in Unity Editor
  - Interactions like Drag&Drop, force pull, push, etc.
  - Multiple official or custom mats can be installed
  - Toyo Collection and Standard ID for developers.
  - One convenient set of mats, lights, and cameras for simulators
- BLE communication module
  - Communicate with the Cube via iOS app
  - Communicating with the Cube via Web App
  - Communicate with the Cube in Android app
  - Communicate with the Cube when playing in Unity Editor on Mac
- Cube
  - One source code to run both simulator and real Cubes alike
  - Scan, connect, and reconnect Cubes
- CubeHandle (useful movement feature)
  - Guide the Cube to the target position and angle
  - Borders can be restricted to keep the Cube from leaving the mat
  - Can move a fixed distance or change angle with a single call
- Navigator (advanced group control)
  - Human-like collision avoidance allows multiple cubes to avoid colliding with each other
  - Voiding allows multiple Cubes to move as a herd
  - Collision avoidance and voids can be combined and run simultaneously
  - In addition to moving to the target, navigation away from the target is also possible.


## Operating Environment

- toio™ Core Cube
- Mat for toio™Core Cube (Play mat included in the toio™ Core Cube collection, Simple play mat included in the toio™Core Cube (standalone), toio play mat for development)
- Mac (macOS ver. 10.14 or higher)
- Windows 10 (64 bit version only)
- iOS devices (iOS ver. 12 or higher)
- Android device (Android OS 9.0 or higher * Currently being supported by Experimental.)
- Unity (2019.4.0f1 LTS)


## Documentation

For details on installation, tutorials, sample introductions, and feature explanations, please refer to the following documents.

- ["toio SDK for Unity Documentation"](docs_EN/README.md)

## License

- [LICENSE](LICENSE)
- [Third Party Notices](Third-Party-Notices.md)
- [Intellectual Property Notice](Trademark-Notices_EN.md)
