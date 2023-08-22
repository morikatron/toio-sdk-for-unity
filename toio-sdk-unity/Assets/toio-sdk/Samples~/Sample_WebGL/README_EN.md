# Sample_WebGL

This is a collection of samples for WebGL builds.<br>
If you build the application with WebGL as the platform, BLE module will be automatically changed to a module for WebGL, and you can communicate with the application on the browser page with Cube.

* Implementing a BLE module for WebGL using the low-level [BLE interface](/docs/sys_ble.md)
 used in [Sample_Bluetooth](../Sample_Bluetooth/README_EN.md).
* Inside BLE module, we will use [web-bluetooth](https://github.com/WebBluetoothCG/web-bluetooth) to communicate with Cube.
* <b>In web-bluetooth, we need to connect with one device at a time from the user operation event. </b> Therefore, in our sample, we will provide a connect button to connect to Cube one device at a time.

<br>

### Sample collection

- [Sample_UI](./Sample_UI/README_EN.md)

This sample shows how the user can manipulate UI to move Cube directly.

- [Sample_Circling](./Sample_Circling/README_EN.md)

This is a sample to see how the behavior of many Cubes changes depending on the mode of CubeNavigator.

- [Sample_WebBluetooth_LowLevel](./Sample_WebBluetooth_LowLevel/README_EN.md)

This sample uses BLE interface, a low-level module, directly to communicate with Cube.

- [Sample_WebPlugin_VeryLowLevel](./Sample_WebPlugin_VeryLowLevel/README_EN.md)

This sample uses the lowest level module, WebBLE plugin, directly to communicate with Cube.
