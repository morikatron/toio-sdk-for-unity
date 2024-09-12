# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased](https://github.com/morikatron/toio-sdk-for-unity/tree/develop)

## [1.6.0](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.5.0) - 2024-09-12

★ **Upgraded to Unity 2022.3.44f1 LTS**

### Added

- [Cube] Support toio core cube BLE protocol version 2.4.0.
  - APIs for connection interval.
  - APIs for posture detection: high precision eulers and quaternions.
- [Simulator] Implement toio core cube BLE protocol version 2.4.0.
  - APIs for posture detection: high precision eulers and quaternions.
- [BLE] Add methods `CubeScanner.StartScan`, `CubeScanner.StopScan`.
- [Sample] Add Sample_ConnectName, Sample_DigitalTwin.

### Changed

- [BLE] Update `AndroidManifest.xml` for Android 12 (API level 31). This update is **NOT backward-compatible**.
- [Simulator] Get rid of `Resources` folder. All assets of simulator are now static. Thus, **if you are using `Resource.Load` to dynamically load simulator assets (e.g. Cube prefab or texture of mat), you need to replace it with static links.**
- [Navigator] Now, no border is set by default. Corresponding samples are also updated. (However, you should still care for CubeHandle's border.)
- [Docs] Upgrade to google analysis 4.
- [Docs] Improve documents on `ConnectType` and visual scripting.

## [1.5.1](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.5.1) - 2022-10-05

### Fixed

- [BLE] Fix WebGL BLE plugin not working after scene switching issue. [issue#258](https://github.com/morikatron/toio-sdk-for-unity/issues/258)
- [Simulator] Fix Custom Mat size calculation issue.

### Removed

- [BLE] Remove deprecated `NearestScanner`, `NearScanner`.
- [BLE] Remove `CubeScanner.NearScanAsync`. Remove corresponding `CubeManager.MultiConnectAsync`.

## [1.5.0](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.5.0) - 2022-04-25

★ **Upgraded to Unity 2021.3.0f1 LTS**

### Added

- [Docs] Documents are published with [Github Pages](https://morikatron.github.io/toio-sdk-for-unity/)
- [Visual Scripting] Supports Unity VIsual Scripting (Version 1.7.6).
  - [Visual Scripting Library] Add `VisualScriptingLibrary`.This library includes functions for using toio-sdk-for-unity in Unity Visual Scripting.
  - [Tutorials] Add Tutorials with Unity Visual Scripting.
  - [Docs] Add Tutorial Document in Unity Visual Scripting.

### Fixed

- [Cube] Fix connecting to connected cube on WebGL blocking `CubeScanner.NearestScan` issue.
- [Cube] Fix disconnecting connected cube blocking `CubeConnecter` issue.
- [Cube] Fix `CubeManager.connectedAction`'s null reference issue.
- [Simulator] Fix tag `t4u_Magnet` not defined issue.


## [1.4.0](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.4.0) - 2021-11-30

★ **Upgraded to Unity 2020.3.17f1 LTS.**

### Added

- [BLE] Add Windows BLE plugin. Support for Windows 10 64bit (version 20H2 or later). (Thanks to contribution of [Yusuke Kurokawa](https://github.com/wotakuro))
- [Cube Real/Simulator] Implement rest features of BLE protocol v2.2.0.
  - [ID] Identification sensor ID notification settings, Identification sensor ID missed notification settings, Responses to identification sensor ID notification settings, Responses to identification sensor ID missed notification settings
  - [Magnetic Sensor] Magnetic sensor settings, Requests for magnetic sensor information, Magnet state, Responses to magnetic sensor settings
- [Cube Real/Simulator] Implement all features of BLE protocol v2.3.0.
  - [Magnetic Sensor] Magnetic sensor settings(updated), Magnetic force
  - [Posture Angle detection] Requesting posture angle detection, Obtaining posture angle information, Posture angle detection settings, Responses to posture angle detection settings
- [Simulator] Implement Magnet prefab.
- [Simulator] Mat prefab includes a new mat type `Gesundroid`.

### Changed

- [BLE] iOS BLE Plugin now supports XCode 13.
- [WebGL] Update WebGL template `webble` for Unity 2020. The old template is renamed as `webble.unity2019`.
- [General] Unity tags used by toio SDK for unity are changed to `t4u_Cube`, `t4u_Mat`, `t4u_StandardID` and `t4u_Magnet`, to avoid potential conflits with users' codes.
- [Sample] Sample_Sensor updated with new Cube features.
- [Simulator] Optimize command processing logic.
- [Simulator] Change the shape of "LED" on the Cube prefab.
- [Simulator] Regenerate octave audio clips for Unity 2020.
- [Cube] Method `RequestSensor` is deprecated. Please use `RequestMotionSensor` instead.

### Removed

- [CubeHandle] Remove deprecated members (`CenterX`, `CenterY`, `SizeX`, `SizeY`, `RangeX`, `RangeY`). Please use `borderRect` or `SetBorderRect` instead.

## [1.3.1](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.3.1) - 2021-08-24

### Added

- [Docs] English documents added.

### Changed

- [Simulator] Camera used by `CubeInteraction` can be assigned from inspector.
(THANKS to contribution from [**zurachu**](https://github.com/zurachu))

### Fixed

- [Simulator] Behaviour of sloped simualtion fixed. The same as real cube, sloped is now set false when cube is upside-down.
- [CubeReal] Add delay after `StartNotifications` at initialization, which otherwise may fail.

## [1.3.0](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.3.0) - 2021-07-29

### Added

- [BLE] Add MacOS BLE plugin. (Thanks to contribution of [Akihiro Komori](https://github.com/comoc))
- [Scanner] Add class `CubeScanner` that integrates `NearestScanner` and `NearScanner`.
- [Simulator] Implement `CubeSimulator.power`, which can be accessed from script or Editor Inspector.
- [Simulator] Implement diconnection and re-connection. Simulator has more similar behaviour as real cube.
- [Simulator] Add sound effects including 'power on/off' and 'conncect/disconnect'.

### Changed

- [Simulator] Simulator's cube object won't disappear when built to mobile device, since new `CubeSanncer` `CubeConnect` require Simulator being able to work under any environment. `CubeSimulator` won't run simulation unless attached to a `CubeUnity` instance.
- [Simulator] Reimplment delay simulation using IEnumerator.
- [Scanner] Replace internal implementations of `NearestScanner` and `NearScanner` with `CubeScanner`.
- [CubeManager] Remove method `CubeManager.SetNearestScanner` and `CubeManager.SetNearScanner` and add method `CubeManager.SetCubeScanner`.

### Fixed.

- [CubeHandle] Not moving backwards on the outside of borders issue fixed.
- [CubeManager] Fix logic of `synced` that is always false after any cube is disconnected. And `syncedCubes` `syncedHandles` `syncedNavigators` now return connected entities only.
- [BLE] Fix Android BLE Plugin disconnection not working issue.
- [BLE] Fix Android BLE Plugin keeping returning characteristics issue.

## [1.2.1](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.2.1) - 2021-04-27

### Added

- [Cube API] Implement method [`Cube.RequestSensor`](https://toio.github.io/toio-spec/docs/ble_sensor#書き込み操作). (THANKS to contribution from [**zurachu**](https://github.com/zurachu))

### Changed

- [Simulator] Checkbuttons on inspector for simulating collison and double tap are changed to buttons.
- [Navigator] `Wall` will be created as a line segement rather than a infinite line.

### Fixed

- [Simulator] Make motion sensors' behaviour of Simulator same with real cube.
  - Specifically, change of each sensor will invoke a callback that updates all motion sensors' state in Cube class.
  - Also, `Cube.isCollisionDetected` or `Cube.isDoubleTap` cannot transit from `true` to `false` **UNTIL** **ANOTHER** sensor invokes an update. **This feature may block callbacks**, therefore calling `Cube.RequestSensor` is suggested to manually force an update.
- [Cube API] `collisionCallback` and `doubleTapCallback` will only be invoked when corresponding states transitting from `false` to `true` (i.e. collision or doubleTap just happened). The reason is similar to above that state transition from `true` to `false` does not mean anything but motion sensors' states updated.
- [Simulator.Mat] Fix developerMat number unable to switch from inspector issue.
- [Simulator.Stage] Fix targetPole operation issue.
- [BLE] Fix error of peripheral.disconnect when using iOS plugin.

## [1.2.0](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.2.0) - 2021-02-24

### Added

- [Simulator] Implement sound presets.
- [BLE] Add Android BLE plugin. (Thanks to contribution of [Yusuke Kurokawa](https://github.com/wotakuro))

### Changed

- [Simulator] Change names of mat types.

## [1.1.0](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.1.0) - 2020-12-15

### Added

- [Cube API] Implementation of firmware version 2.1.0 & 2.2.0
  - Implement updated motor's deadzone and max value.
  - Implement pose detection feature (callback & property).
  - Implement doubleTap detection feature (callback & property) and its internal config method.
  - Implement motor speed reading feature (callback & property) and its config method.
  - Implement shake detection feature (callback & property).
  - Implement motor's TargetMove(目標指定付きモーター制御), AccelerationMove(加速度指定付きモーター制御) methods and enums for options and response callbacks.
- [Simulator] Implementation of firmware version 2.1.0 & 2.2.0
  - Implementation classes added; Version list on inspector appended.
  - Implement updated motor's deadzone and max value.
  - Implement pose detection feature (physical simulation supported).
  - Implement doubleTap detection feature (only changeable on inspector).
  - Implement motor speed reading feature and its config method.
  - Implement shake feature (only changeable on inspector).
  - Implement motor's TargetMove(目標指定付きモーター制御), AccelerationMove(加速度指定付きモーター制御) features.
- [Simulator] Add building support for MAC/Windows.
- [Sample] Add Sample_Motor which demonstrates Cube.TargetMove and Cube.AccelerationMove.

### Changed

- Improve CubeHandle and Navigator's border feature
  - [Mat] Add method `RectInt GetRectForMatType(MatType, DeveloperMatType)`.
  - [CubeHandle] Add field `borderRect` and method `SetBorderRect`, and deprecate fields `CenterX` `CenterY` `SizeX` `SizeY` `RangeX` `RangeY`.
  - [Navigator] Add overload for `AddBorder`.
  - Corresponding changes in tutorials, samples, tests and documents.
- [Simulator] Change motor simulation.
  - Seperate tire's speed from object's speed.
  - Deadzone applied on receiving commands instead of internal motor simulation.
- [Simulator] Refactoring implementation of command queuing.
- [Cube API] Replace CallbackProvider with multiple templates.
- [Sample] Append new features to Sample_Sensor.

### Fixed

- [Cube API] Fix wrong max size for operation list of TurnOnLightWithScenario and PlaySound.
- [CubeHandle] Fix border not working when cube going inside from outside.

## [1.0.2](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.0.2) - 2020-11-05

### Fixed

- Add an optional argument 'border' to method 'Movement.Exec()'
- Fix link error when installing a unity asset that include Assembly-Definition-Files

## [1.0.1](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.0.1) - 2020-10-08

### Added

- Add version.txt.
- Add guidance for git.

### Changed

- Update ble-plugin-unity.
- Update comments for codes of Simulator.

### Fixed

- Fix warning when importing cube_model.fbx.

### Removed

- Remove contents of CocoaPods from documents.


## [1.0.0](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.0.0) - 2020-09-29

### Added

- Add Trademark Notice in README.md.
- Add some sections in FAQ.

### Changed

- Change notations on UI in Sample_Sensor demo.

### Fixed

- Fix null reference exception in BoidsTutorial and BoidsAvoidTutorlal.
- Fix warning in ble-plugin-unity.
- Fix documents.

## [1.0.0-beta0](https://github.com/morikatron/toio-sdk-for-unity/tree/v1.0.0-beta0) - 2020-09-22

Initial Release. (Internal Release)
