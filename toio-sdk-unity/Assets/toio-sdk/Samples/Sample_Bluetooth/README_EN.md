## Sample_Bluetooth

<div align="center">
<img height=280 src="/docs/res/samples/real.gif" title="How Cube works" alt="How Cube works">
</div>

<br>

This sample uses BLE interface, a low-level module, directly to communicate with Cube.<br>
When run on a smart phone build, Cube will spin around.

> This sample only uses Bluetooth module, so it will not work in Simulator.

* All communication modules are implemented by inheriting BLE interface, and can be replaced with communication modules other than BLE by developing new inheritance classes.
* The internal implementation of the communication module can be injected by calling `BLEService.Instance.SetImplement()` function of BLEService, a singleton class.
* The sample injects the internal implementation of mobile BLE by `BLEService.Instance.SetImplement(new BLEMobileService())`
* This sample relies only on BLE interface and can be used to check the operation of your own communication module.
* Please refer to [BLE Function Description](/docs_EN/sys_ble.md) for details.