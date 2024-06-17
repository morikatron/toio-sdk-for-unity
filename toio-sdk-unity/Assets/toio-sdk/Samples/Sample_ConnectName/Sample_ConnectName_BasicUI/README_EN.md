## Sample_ConnectName_BasicUI

This sample lists the Local Names of the scanned cubes on the screen, allowing the user to select, connect, and disconnect.

<div align="center">
<img src="../../../../../../docs_en/res/samples/connectName_basic.png">
</div>
<br>

Press the 【Scan】 button to start scanning for cubes.
The scan continues for about 10 seconds. The scanned cubes are displayed in the list.
If a cube is turned off or otherwise becomes unavailable, it will be removed from the list.

You can connect to the cube with the corresponding Local Name by clicking on the list item of the scanned cubes.

### Technical Highlights

In the scan callback, the scanned peripherals are saved, and the GUI list is updated in the `Update` function.
Here is the main update code:

```csharp
void Update ()
{
    // ...
    // Display scanned items
    foreach (var peripheral in this.scannedPeripherals) {
        if (peripheral == null) continue;
        var item = TryGetCubeItem(peripheral);
        item.transform.SetSiblingIndex(idx++);
        addrsToRemove.Remove(peripheral.device_address);
    }
    // ...
}
```
※ Connected cubes are not scanned, so different processing is required to display them. For details, please refer to the sample code.

### Notes

If a scanned cube is turned off, it will not immediately disappear from the list returned by OnScanUpdate.
Attempting to connect in this state will generally result in a timeout.
However, on iOS/MacOS, if a timeout occurs twice, there is a potential issue where access to the BLE device itself may be lost.

### Related Materials

- You can set the `ConnectType` (whether to connect to a real or simulator cube) in the Inspector. For details on `ConnectType`, please refer to [Sample_ConnectType](../../Sample_ConnectType/README_EN.md).
- For information on how to create the UI, please refer to [Tutorial (Creating UI)](../../../../../../docs_en/tutorials_UI.md).
