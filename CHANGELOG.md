# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased](https://github.com/morikatron/toio-sdk-for-unity/tree/develop)

### Added

- [Simulator] Implement sound presets.

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