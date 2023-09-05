## Sample_ConnectType

<div  align="center">
<img src="../../../../../docs/res/cube/sample_connectType.png">
</div>

This sample is intended to verify how the connection varies between the simulator and the physical device due to differences in the connection settings.

In the Unity Editor hierarchy, select the `scene` object and you can change the way to connect to the cube by toggling the Connect Type in the inspector.

### ConnectType specification

`ConnectType` is an enumeration type, and you can set one of the following three values.

- `Auto`: Automatic (default value). If you press the Play button in Unity Editor, it connects to the cube within the simulator; otherwise (when building and running the app on the device), it connects to the actual cube.
- `Simulator`: In all cases, it connects to the cube within the simulator.
- `Real`: In all cases, it connects to the actual device. (Even when you press the Play button in Unity Editor, it connects to the actual device.)

If you are going through a lot of trial and error during development, it is more efficient to check on the Unity Editor rather than building and running the app on the device. By effectively switching between the quick execution of the simulator and the precise validation results of the actual device environment with ConnectType in Unity Editor, you can achieve more efficient development.

### Setting ConnectType in code

Whether you use `CubeManager`, or `CubeScanner` and `CubeConnecter`, you can set it by passing a variable of ConnectType type.

```c#
cm = new CubeManager(connectType);
// or
scanner = new CubeScanner(connectType);
connecter = new CubeConnecter(connectType);
```

In this sample, by defining the variable `connectType` as a public variable, you can select it from the Unity Editor inspector.

```c#
public class Sample_ConnectType : MonoBehaviour
{
    public ConnectType connectType;
    // ...
}
```
