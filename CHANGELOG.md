# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased](https://github.com/morikatron/toio-sdk-for-unity/tree/develop)

### Changed

- Improve CubeHandle and Navigator's border feature
  - [Mat] Add method `RectInt GetRectForMatType(MatType, DeveloperMatType)`.
  - [CubeHandle] Add field `borderRect` and method `SetBorderRect`, and deprecate fields `CenterX` `CenterY` `SizeX` `SizeY` `RangeX` `RangeY`.
  - [Navigator] Add overload for `AddBorder`.
  - Corresponding changes in tutorials, samples, tests and documents.


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