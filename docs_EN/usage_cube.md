# Technical Documentation - Usage - Cube Class

## Table of Contents

- [1. Outline](usage_cube.md#1-outline)
- [2. Comparison with existing toio™ library (toio.js)](usage_cube.md#2-comparison-with-existing-toio-library-toiojs)
- [3. Cube Class API](usage_cube.md#3-cube-class-api)
  - [3.1. Variable](usage_cube.md#31-variable)
  - [3.2. Callback](usage_cube.md#32-callback)
  - [3.3. Method](usage_cube.md#33-method)
- [4. Cube connection settings](usage_cube.md#4-cube-connection-settings)
  <br>

# 1. Outline

Cube class implements the functionality for manipulating toio™ core cubes (henceforth called cubes).<br>
This class is a multi-platform class that can switch between implementations for different environments, and supports two execution environments: Cube running on Unity system (Simulator) and a real Cube.

For more information about Simulator, please refer to [here](usage_simulator.md) page.

In real Cube, you control real Cube through Bluetooth communication from Unity program according to the [toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_communication_overview).

### Real/Sim Performance Meter

Currently (09/01/2021), there are 4 BLE protocol versions in Cube.

`2.0.0`　`2.1.0`　`2.2.0`　`2.3.0`

toio SDK for Unity has two internal implementations: Cube class that runs in real life (Real-compatible) and Cube class that runs in Simulator (Sim-compatible). Since the internal implementations are different, there are differences in the support status.<br>
The following table shows the implementation correspondence.

#### BLE protocol version 2.0.0

> Links are of version 2.3.0, which is the lowerest version supporting English.

| Function Type      | Function                                                                                                                     | Real Support Status | Sim Support Status |
| ------------------ | ---------------------------------------------------------------------------------------------------------------------------------- | ------------- | ------------ |
| Identification sensor | [Position ID](https://toio.github.io/toio-spec/en/docs/ble_id#position-id)                                                      | o             | o            |
|                    | [Standard ID](https://toio.github.io/toio-spec/en/docs/ble_id#standard-id)                                                         | o             | o            |
| Motion sensor      | [Horizontal detection](https://toio.github.io/toio-spec/en/docs/ble_sensor#horizontal-detection)                                   | o             | o            |
|                    | [Collision detection](https://toio.github.io/toio-spec/en/docs/ble_sensor#collision-detection)                                     | o             | ※           |
| Button             | [State of button](https://toio.github.io/toio-spec/en/docs/ble_button#state-of-button)                                             | o             | o            |
| Battery            | [Remaining battery level](https://toio.github.io/toio-spec/en/docs/ble_battery#remaining-battery-level)                            | o             | x            |
| Motor              | [Motor control](https://toio.github.io/toio-spec/en/docs/ble_motor#motor-control)                                                  | x             | x            |
|                    | [Motor control with specified duration](https://toio.github.io/toio-spec/en/docs/ble_motor#motor-control-with-specified-duration)  | o             | o            |
| Indicator          | [Turning the indicator on and off](https://toio.github.io/toio-spec/en/docs/ble_light#turning-the-indicator-on-and-off)            | o             | o            |
|                    | [Repeated turning on and off of indicator](https://toio.github.io/toio-spec/en/docs/ble_light#repeated-turning-on-and-off-of-indicator) | o        | o            |
|                    | [Turn off all indicators.](https://toio.github.io/toio-spec/en/docs/ble_light#turn-off-all-indicators)                             | x             | x            |
|                    | [Turn off a specific indicator.](https://toio.github.io/toio-spec/en/docs/ble_light#turn-off-a-specific-indicator)                 | x             | x            |
| Sound              | [Playing sound effects](https://toio.github.io/toio-spec/en/docs/ble_sound#playing-sound-effects)                                  | o             | o            |
|                    | [Playing the MIDI note numbers](https://toio.github.io/toio-spec/en/docs/ble_sound#playing-the-midi-note-numbers)                  | o             | o            |
|                    | [Stop playing](https://toio.github.io/toio-spec/en/docs/ble_sound#stop-playing)                                                    | o             | o            |
| Configuration      | [Requesting the BLE protocol version](https://toio.github.io/toio-spec/en/docs/ble_configuration#requesting-the-ble-protocol-version) | o          | x            |
|                    | [Horizontal detection threshold settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#horizontal-detection-threshold-settings) | o  | o            |
|                    | [Collision detection threshold settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#collision-detection-threshold-settings) | o    | x            |
|                    | [Obtaining the BLE protocol version](https://toio.github.io/toio-spec/en/docs/ble_configuration#obtaining-the-ble-protocol-version)         | o    | x            |

> ... The detection function is not implemented on Simulator side, but you can manually switch the judgment on and off from the inspector. For more details, please refer to [[here]](usage_simulator.md#41-inspector-in-cubesimulator).

#### BLE protocol version 2.1.0

> Links are of version 2.3.0, which is the lowerest version supporting English.

| Function Type      | Function                                                                                                                              | Real Support Status | Sim Support Status |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------- | ------------- | ------------ |
| Motion sensor      | [Double-tap detection](https://toio.github.io/toio-spec/en/docs/ble_sensor/#double-tap-detection)                                           | o             | ※           |
|                    | [Posture detection](https://toio.github.io/toio-spec/en/docs/ble_sensor#posture-detection)                                                  | o             | o            |
| Motor              | [Motor speed command values (updated)](https://toio.github.io/toio-spec/en/docs/ble_motor#motor-speed-command-values)                       | o             | o            |
|                    | [Motor control with target specified](https://toio.github.io/toio-spec/en/docs/ble_motor#motor-control-with-target-specified)               | o             | o            |
|                    | [Motor control with multiple targets specified](https://toio.github.io/toio-spec/en/docs/ble_motor#motor-control-with-multiple-targets-specified) | x       | x            |
|                    | [Motor control with acceleration specified](https://toio.github.io/toio-spec/en/docs/ble_motor#motor-control-with-acceleration-specified)         | o       | o            |
|                    | [Responses to motor control with target specified](https://toio.github.io/toio-spec/en/docs/ble_motor#responses-to-motor-control-with-target-specified) | o | o            |
|                    | [Responses to motor control with multiple targets specified](https://toio.github.io/toio-spec/en/docs/ble_motor#responses-to-motor-control-with-multiple-targets-specified) | x | x |
| Configuration      | [Double-tap detection time interval settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#double-tap-detection-time-interval-settings) | o   | x            |

#### BLE protocol version 2.2.0

> Links are of version 2.3.0, which is the lowerest version supporting English.

| Function Type      | Function                                                                                                                              | Real Support Status | Sim Support Status |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------- | ------------- | ------------ |
| Motion sensor      | [Requesting motion detection information](https://toio.github.io/toio-spec/en/docs/ble_sensor#requesting-motion-detection-information)      | o             | o            |
|                    | [Shake detection](https://toio.github.io/toio-spec/en/docs/ble_sensor#shake-detection)                                                      | o             | o            |
| Magnetic sensor    | [Requests for magnetic sensor information](https://toio.github.io/toio-spec/en/docs/ble_magnetic_sensor#requests-for-magnetic-sensor-information) | o       | o            |
|                    | [Magnet state](https://toio.github.io/toio-spec/en/docs/ble_magnetic_sensor#magnet-state)                                                   | o             | o            |
| Motor              | [Obtaining motor speed information](https://toio.github.io/toio-spec/en/docs/ble_motor#obtaining-motor-speed-information)                   | o             | o            |
| Configuration      | [Identification sensor ID notification settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#identification-sensor-id-notification-settings) | o     | o    |
|                    | [Identification sensor ID missed notification settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#identification-sensor-id-missed-notification-settings)| o | o |
|                    | [Responses to identification sensor ID notification settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#responses-to-identification-sensor-id-notification-settings) | o | o |
|                    | [Responses to identification sensor ID missed notification settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#responses-to-identification-sensor-id-missed-notification-settings) | o | o |
|                    | [Magnetic sensor settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#magnetic-sensor-settings-)                            | o             | o            |
|                    | [Responses to magnetic sensor settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#responses-to-magnetic-sensor-settings)   | o             | o            |
|                    | [Motor speed information acquisition settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#motor-speed-information-acquisition-settings) | o | o            |
|                    | [Responses to motor speed information acquisition settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#responses-to-motor-speed-information-acquisition-settings)| o | o |

#### BLE protocol version 2.3.0

| Function Type      | Function                                                                                                                              | Real Support Status | Sim Support Status |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------- | ------------- | ------------ |
| Magnetic sensor    | [Magnetic force detection](https://toio.github.io/toio-spec/en/docs/ble_sensor#magnetic-force-detection-)                                   | o             | o            |
| Posture Angle      | [Requesting posture angle detection](https://toio.github.io/toio-spec/en/docs/ble_high_precision_tilt_sensor#requesting-posture-angle-detection) | o        | o            |
|                    | [Obtaining posture angle information (notifications in Euler angles)](https://toio.github.io/toio-spec/en/docs/ble_high_precision_tilt_sensor#obtaining-posture-angle-information-notifications-in-euler-angles) | o | o |
|                    | [Obtaining posture angle information (notifications in quaternions)](https://toio.github.io/toio-spec/en/docs/ble_high_precision_tilt_sensor#obtaining-posture-angle-information-notifications-in-quaternions) | o | o |
| Configuration      | [Magnetic sensor settings(updated)](https://toio.github.io/toio-spec/en/docs/ble_configuration#magnetic-sensor-settings-)                   | o             | o            |
|                    | [Posture angle detection settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#posture-angle-detection-settings-)            | o             | o            |
|                    | [Responses to posture angle detection settings](https://toio.github.io/toio-spec/en/docs/ble_configuration#responses-to-posture-angle-detection-settings-) | o | o         |

<br>

# 2. Comparison with existing toio™ library (toio.js)

[toio.js](https://github.com/toio/toio.js) is an existing toio library written in javascript.<br>
This library is built in node.js and runs in a PC environment.<br>
We will use two classes, scanner and cube, to manipulate Cube.

toio.js sample code is shown below.

```javascript
const { NearestScanner } = require("@toio/scanner");

async function main() {
  // start a scanner to find the nearest cube
  const cube = await new NearestScanner().start();

  // connect to Cube
  await cube.connect();

  // move cube
  cube.move(100, 100, 1000);
  //         |    |     `--- duration [ms]
  //         |    `--------- right motor speed
  //          `------------- left motor speed
}

main();
```

toio SDK for Unity is designed to be easy to use for toio.js users, and uses two classes, scanner and Cube, to manipulate Cube.

The following is a sample code of this program with the same behavior.

```csharp
public class SimpleScene : MonoBehaviour
{
    async void Start()
    {
        // start a scanner to find the nearest cube
        var peripheral = await new NearestScanner().Scan();

        // connect to Cube
        var cube = await new CubeConnecter().Connect(peripheral);

        // move cube
        cube.Move(100, 100, 1000);
        //         |    |     `-- duration [ms]
        //         |    `-------- right motor speed
        //          `------------ left motor speed
    }
}
```

<br>

# 3. Cube Class API

## 3.1. Variable

```csharp
// BLE protocol version of the connected cube
public string version { get; }

// Unique identification ID of Cube
// In simulator environment, InstanceID of Cube game object becomes ID
public string id { get; protected set; }

// Address of Cube
public string addr { get; }

// Complete Local Name
public string localName { get; }

// Connection status of Cube
public bool isConnected { get; }

// Cube battery status
public int battery { get; protected set; }

// X-coordinate of Cube on the mat
// The coordinates are based on the center position of Cube.
// It will automatically update while Cube is on the mat.
// Callback function: standardIdCallback
public int x { get; protected set; }

// Y-coordinate of Cube on the mat
// The coordinates are based on the center position of Cube.
// It will automatically update while Cube is on the mat.
// Callback function: standardIdCallback
public int y { get; protected set; }

// XY coordinates of Cube on the mat
// The coordinates are based on the center position of Cube.
// It will automatically update while Cube is on the mat.
// Callback function: standardIdCallback
public Vector2 pos { get; }

// Angle of Cube on the mat
// The coordinates are based on the center position of Cube.
// It will automatically update while Cube is on the mat.
// Callback function: standardIdCallback
public int angle { get; protected set; }

// XY coordinates of Cube on the mat
// The coordinates are based on the optical sensor position of Cube.
// It will automatically update while Cube is on the mat.
// Callback function: standardIdCallback
public Vector2 sensorPos { get; }

// Angle of Cube on the mat
// The coordinates are based on the optical sensor position of Cube.
// It will automatically update while Cube is on the mat.
// Callback function: standardIdCallback
public int sensorAngle { get; protected set; }

// Special sticker IDs that can be read
// The ID is obtained from Cube's optical sensor.
// Callback function: standardIdCallback
public uint standardId { get; protected set; }

// Variable for the button-press state of Cube
// Callback function: buttonCallback
public bool isPressed { get; protected set; }

// Cube tilt state variable
// Callback function: slopeCallback
public bool isSloped { get; protected set; }

// Cube collision state variables
// The internal implementation may vary greatly depending on the inherited class.
// Callback function: collisionCallback
public bool isCollisionDetected { get; protected set; }

// Variable to determine if Cube is on the mat.
public bool isGrounded { get; protected set; }

// Variable representing the maximum speed of Cube.
// It is provided for each BLE protocol version differently.
public int maxSpd { get; }

// Variable representing the minimum speed of Cube
// It is provided for each BLE protocol version differently.
public int deadzone { get; }

// ver2.1.0
// Double-tap state of Cube
// Once it is tapped, it will be tapped again within a certain period of time.
// Callback function: doubleTapCallback
public bool isDoubleTap { get; protected set; }

// Cube posture
// The value changes when the orientation of Cube relative to the horizontal plane changes.
// Callback function: poseCallback
public PoseType pose { get; protected set; }

// ver2.2.0
// Shaking state of Cube
// When you shake Cube, the value changes according to the strength of the shake.
// Callback function: shakeCallback
public int shakeLevel { get; protected set; }

// Speed of Cube's motor ID 1 (left)
// Callback function: motorSpeedCallback
public int leftSpeed { get; protected set; }

// Speed of Cube's motor ID 2 (Right)
// Callback function: motorSpeedCallback
public int rightSpeed { get; protected set; }

// Magnet state of Cube
// Callback function: magnetStateCallback
public MagnetState magnetState { get; protected set; }

// ver2.3.0
// Magnetic force of Cube
// Callback function: magneticForceCallback
public Vector3 magneticForce { get; protected set; }

// Attitude of Cube in eulers
// Callback function: attitudeCallback
public Vector3 eulers { get; protected set; }

// Attitude of Cube in quaternion
// Callback function: attitudeCallback
// Currently (2021.09.01), the coordinates of quaternion is different from euler's. The euler's is correct to the specification.
public Quaternion quaternion { get; protected set; }
```

<br>

## 3.2. callback

```csharp
// CallbackProvider<T1>
// CallbackProvider<T1, T2>
// CallbackProvider<T1, T2, T3>
// CallbackProvider<T1, T2, T3, T4>
// ※Pseudo code
public class CallbackProvider<T...>
{
    public virtual void AddListener(string key, Action<T...> listener);
    public virtual void RemoveListener(string key);
    public virtual void ClearListener();
    public virtual void Notify(T... args);
}

// Button callback
public CallbackProvider<Cube> buttonCallback { get; }
// Tilt callback
public CallbackProvider<Cube> slopeCallback { get; }
// Collision callback
public CallbackProvider<Cube> collisionCallback { get; }
// Coordinate angle callback
public CallbackProvider<Cube> idCallback { get; }
// Coordinate angle Missed callback
public CallbackProvider<Cube> idMissedCallback { get; }
// StandardID callback
public CallbackProvider<Cube> standardIdCallback { get; }
// StandardID missed callback
public CallbackProvider<Cube> standardIdMissedCallback { get; }

// ver2.1.0
// Double tap callback
public CallbackProvider<Cube> doubleTapCallback { get; }
// Attitude detection callback
public CallbackProvider<Cube> poseCallback { get; }
// Response callback for motor control with target specification
public CallbackProvider<Cube, int, TargetMoveRespondType> targetMoveCallback { get; }

// ver2.2.0
// Shake callback
public CallbackProvider<Cube> shakeCallback { get; }
// Motor speed callback
public CallbackProvider<Cube> motorSpeedCallback { get; }
// Magnet state callback
public CallbackProvider<Cube> magnetStateCallback { get; }

// ver2.3.0
// Magnetic force detection callback
public CallbackProvider<Cube> magneticForceCallback { get; }
// Attitude callback
public CallbackProvider<Cube> attitudeCallback { get; }
```

## 3.3. Method

### Move

```csharp
public void Move(int left, int right, int durationMs, ORDER_TYPE order=ORDER_TYPE.Weak);
```

Controls Cube's motor.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_motor/#motor-control-with-specified-duration)

- left
  - Definition: Left motor speed
  - Range :
    - [Version 2.0.0] -100 to -10; -9 to 9 is equivalent to 0; 10 to 100
    - [Version 2.1.0] -115 ~ -8; -7 ~ 7 is equivalent to 0; 8 ~ 115
- right
  - Definition: Right motor speed
  - Range :
    - [Version 2.0.0] -100 to -10; -9 to 9 is equivalent to 0; 10 to 100
    - [Version 2.1.0] -115 ~ -8; -7 ~ 7 is equivalent to 0; 8 ~ 115
- durationMs
  - Definition: Duration (milliseconds)
  - Range :
    - 0~9 : No time limit
    - 10~2550 : Precision is 10ms, ones place is omitted
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### TurnLedOn

```csharp
public void TurnLedOn(int red, int green, int blue, int durationMs, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Controls the LEDs on the bottom of Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_light#turning-the-indicator-on-and-off)

- red
  - Definition : Red intensity
  - Range : 0~255
- green
  - Definition : Green strength
  - Range : 0~255
- blue
  - Definition : Strength of blue color
  - Range : 0~255
- durationMs
  - Definition: Duration (milliseconds)
  - Range :
    - 0~9 : No time limit
    - 10~2550 : Accuracy is 10ms, 1st place is omitted
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### TurnOnLightWithScenario

```csharp
// Configuration structure per luminescence
public struct LightOperation
{
    public int durationMs; // Millisecond
    public byte red;         // Red intensity
    public byte green;       // Green intensity
    public byte blue;        // Blue intensity
}
public void TurnOnLightWithScenario(int repeatCount, Cube.LightOperation[] operations, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Continuously controls the LEDs on the bottom of Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_light#repeated-turning-on-and-off-of-indicator)

- repeatCount
  - Definition: number of repetitions
  - Range : 0~255
- operations
  - Definition: instruction array
  - Quantity : 1~29
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### TurnLedOff

```csharp
public void TurnLedOff(ORDER_TYPE order=ORDER_TYPE.Strong);
```

Turn off the LED on the bottom of Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_light#turn-off-all-indicators)

- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### PlayPresetSound

```csharp
public void PlayPresetSound(int soundId, int volume=255, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Play the sound effects provided in Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_sound#playing-sound-effects)

- soundId
  - Definition: Sound ID
  - Range : 0~10
- volume:
  - Definition of: volume
  - Range : 0~255
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### PlaySound

Play any sound from Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_sound#playing-the-midi-note-numbers)

```csharp
// Argument version
// A configuration structure for each pronunciation.
public struct SoundOperation
{
    public int durationMs; // Millisecond
    public byte volume;      // Volume (0~255)
    public byte note_number; // Notes(0~128)
}
public void PlaySound(int repeatCount, SoundOperation[] operations, ORDER_TYPE order=ORDER_TYPE.Strong);
```

- repeatCount
  - Definition: number of repetitions
  - Range : 0~255
- operations
  - Definition: instruction array
  - Quantity : 1~59
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

```csharp
// Buffered version (e.g. of a file)
public void PlaySound(byte[] buff, ORDER_TYPE order=ORDER_TYPE.Strong);
```

- buff
  - Definition : Data block defined by[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_sound#playing-the-midi-note-numbers)
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### StopSound

```csharp
public void StopSound(ORDER_TYPE order=ORDER_TYPE.Strong);
```

Stops Cube from playing sound.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_sound#stop-playing)

- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### ConfigSlopeThreshold

```csharp
public void ConfigSlopeThreshold(int angle, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Sets the threshold for horizontal detection of Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_configuration#horizontal-detection-threshold-settings)

- angle
  - Definition : Threshold for tilt detection
  - Range : 1~45
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### ConfigCollisionThreshold

```csharp
public void ConfigCollisionThreshold(int level, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Sets the threshold for collision detection for Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_configuration#collision-detection-threshold-settings)

- level
  - Definition: Collision detection threshold
  - Range : 1~10
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### ConfigDoubleTapInterval

```csharp
public void ConfigDoubleTapInterval(int interval, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Set the time interval for double-tap detection of Cube <br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_configuration#double-tap-detection-time-interval-settings)

- interval
  - Definition: Time interval for double-tap detection
  - Range : 1~7
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### TargetMove

```csharp
public void TargetMove(
            int targetX,
            int targetY,
            int targetAngle,
            byte configID = 0,
            byte timeOut = 0,
            TargetMoveType targetMoveType = TargetMoveType.RotatingMove,
            byte maxSpd = 80,
            TargetSpeedType targetSpeedType = TargetSpeedType.UniformSpeed,
            TargetRotationType targetRotationType = TargetRotationType.AbsoluteLeastAngle,
            ORDER_TYPE order = ORDER_TYPE.Strong);
```
Controls Cube's motor with target designation.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_motor#motor-control-with-target-specified)

- targetX
  - Definition: X-coordinate value of the target point
  - Range : -1, 0~65534
    - -1, the X coordinate is set to the same as in the write operation
- targetY
  - Definition : Y-coordinate value of the target point
  - Range : -1, 0~65534
    - -1, the Y coordinate is set to the same as in the write operation
- targetAngle
  - Definition : Angle of Cube at the target point Θ
  - Range : 0~8191
- configID
  - Definition: Control identification value, a value used to identify the response of a control. The value set here will also be included in the corresponding response.
  - Range : 0~255
- timeOut
  - Definition : timeout period
  - Range : 0~255
    -  The only exception is 0, which is 10 seconds.
- targetMoveType
  - Definition: move type
  - Type :
    - RotatingMove : Move while rotating
    - RoundForwardMove : Move while spinning (no retreat)
    - RoundBeforeMove : Rotate and then move.
- maxSpd
  - Definition: maximum speed indication value of motor
  - Range : 10~255
- targetSpeedType
  - Definition: motor speed change type
  - Type :
    - UniformSpeed : Constant speed
    - Acceleration : Gradual acceleration to target point
    - Deceleration : Gradual deceleration to target point
    - VariableSpeed : Gradually accelerate to the midpoint and then decelerate to the target point.
- targetRotationType
  - Definition : Type (meaning) of angle Θ of Cube at the target point
  - Type :
    - AbsoluteLeastAngle : Direction with small absolute angle and rotation amount
    - AbsoluteClockwise : Absolute angle, forward direction (clockwise)
    - AbsoluteCounterClockwise : Absolute angle, negative direction (counterclockwise)
    - RelativeClockwise : Relative angle, forward direction (clockwise)
    - RelativeCounterClockwise : Relative angle, negative direction (counterclockwise)
    - NotRotate : No rotation
    - Original : The same as the writing operation, but with a smaller amount of rotation
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### AccelerationMove

```csharp
public void AccelerationMove(
            int targetSpeed,
            int acceleration,
            ushort rotationSpeed = 0,
            AccPriorityType accPriorityType = AccPriorityType.Translation,
            byte controlTime = 0,
            ORDER_TYPE order = ORDER_TYPE.Strong);
```

Performs motor control with acceleration specified for Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_motor#motor-control-with-acceleration-specified)

- targetSpeed
  - Definition: the speed at which Cube moves in the direction of travel.
  - Range : 8~115
- acceleration
  - Definition: acceleration of Cube
  - Range : 0~255
    -  If 0, the speed will be the one specified in "Cube translation speed".
- rotationSpeed
  - Definition: Rotation speed in the direction of Cube [degrees/second] A minus sign indicates a negative direction (counterclockwise).
  - Range : 0~65535

- accPriorityType
  - Definition: priority designation
  - Type :
    - Translation : Give priority to translation speed and adjust rotation speed.
    - Rotation : Give priority to rotational speed and adjust translational speed.
- controlTime
  - Definition : Control time [10ms].
  - Range : 0~255
    -  0 means "no time limit".
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### ConfigMotorRead

```csharp
public UniTask ConfigMotorRead(bool valid, float timeOutSec=0.5f, Action<bool,Cube> callback=null, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Enables or disables the acquisition of Cube's motor speed information.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_configuration#motor-speed-information-acquisition-settings)

- valid
  - Definition : enable/disable flag
  - Type : true, false
- timeOutSec
  - Definition: timeout (in seconds)
  - Range : 0.5
- callback
  - Definition : Exit callback (set success flag, cube)
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### RequestSensor

Deprecated. Please use `RequestMotionSensor` instead.

<br>

### RequestMotionSensor

```csharp
public void RequestMotionSensor(ORDER_TYPE order = ORDER_TYPE.Strong);
```

Requests Cube to notify you of motion sensor information once.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_sensor#requesting-motion-detection-information)

> Since collision detection and double tap detection are notified only when they occur, it is impossible for the variable `Cube.isCollisionDetected` `Cube.isDoubleTap` to return from the `True` state to `False` unless it is notified by another motion sensor. Therefore, it is possible to update these two variables by using `RequestMotionSensor` to ask for notification.

- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### ConfigIDNotification

```csharp
public UniTask ConfigIDNotification(
    int intervalMs,
    IDNotificationType notificationType = IDNotificationType.Balanced,
    Balanced, float timeOutSec = 0.5f,
    Action<bool,Cube> callback = null,
    ORDER_TYPE order = ORDER_TYPE.Strong);
````

Set the notification frequency for the Position ID and Standard ID from the identification sensor. Notifications are sent when both the [Minimum Notification Interval] and [Notification Conditions] are met. <br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_configuration/#identification-sensor-id-notification-settings)

- intervalMs
  - Definition : Minimum notification interval (in milliseconds)
  - Range : 0~2550, precision is 10ms, ones place is omitted.
- notificationType
  - Definition : Notification condition
  - Type : Always, OnChanged, Balanced
- timeOutSec
  - Definition: timeout (in seconds)
  - Range : 0.5~
- callback
  - Definition : end callback (set success flag, cube)
- order
  - Definition : [instruction priority](sys_cube.md#4-Send-Command)
  - Type : Weak, Strong

<br>

### ConfigIDMissedNotification

```csharp
public UniTask ConfigIDMissedNotification(
    int sensitivityMs,
    float timeOutSec = 0.5f,
    Action<bool,Cube> callback = null,
    ORDER_TYPE order = ORDER_TYPE.Strong);
````

Set the Position ID missed and Standard ID missed notification sensitivity of the identification sensor. <br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_configuration/#identification-sensor-id-missed-notification-settings)

- sensitivityMs
  - Definition : Minimum notification interval (in milliseconds)
  - Range : 0~2550, precision is 10ms, ones place is omitted
- timeOutSec
  - Definition : Timeout (sec)
  - Range : 0.5~
- callback
  - Definition : end callback (set success flag, cube)
- order
  - Definition : [instruction priority](sys_cube.md#4-Send-Command)
  - Type : Weak, Strong

<br>

### ConfigMagneticSensor

```csharp
public UniTask ConfigMagneticSensor(
    MagneticMode mode,
    float timeOutSec = 0.5f,
    Action<bool,Cube> callback = null,
    ORDER_TYPE order = ORDER_TYPE.Strong);
````

Sets the mode of the cube's magnetic sensor. It is disabled by default. (Supported since v2.2.0) <br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_configuration/#magnetic-sensor-settings-)

- mode
  - Definition : Function setting
  - Type :
    - [ver2.2.0] Off, MagnetState (enable magnet state detection)
    - [ver2.3.0] Off, MagnetState (enable magnet state detection), MagneticForce (enable magnetic force detection)
- timeOutSec
  - Definition: timeout (sec)
  - Range : 0.5~
- callback
  - Definition : end callback (set success flag, cube)
- order
  - Definition : [instruction priority](sys_cube.md#4-Send-Command)
  - Type : Weak, Strong


```csharp
public UniTask ConfigMagneticSensor(
    MagneticMode mode,
    int intervalMs,
    MagneticNotificationType notificationType,
    float timeOutSec = 0.5f,
    Action<bool,Cube> callback = null,
    ORDER_TYPE order = ORDER_TYPE.Strong);
````

Sets the mode of the cube's magnetic sensor. It is disabled by default. (Supported since v2.3.0) <br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_configuration/#magnetic-sensor-settings-)

- intervalMs
  - Definition : Notification interval (in milliseconds)
  - Range : 0~2550, precision is 20ms, the part less than 20ms is omitted.
- notificationType
  - Definition : notification condition
  - Type : Always, OnChanged

<br>

### ConfigAttitudeSensor

```csharp
public UniTask ConfigAttitudeSensor(
    AttitudeFormat format,
    int intervalMs,
    AttitudeNotificationType notificationType,
    float timeOutSec = 0.5f,
    Action<bool,Cube> callback = null,
    ORDER_TYPE order = ORDER_TYPE.Strong);
````

Enables or disables the cube's attitude angle detection feature. It is disabled by default. (Supported since v2.3.0) <br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_configuration/#posture-angle-detection-settings-)

- format
  - Definition : Type of notification content
  - Type : Eulers, Quaternion
- intervalMs
  - Definition : Minimum notification interval (in milliseconds)
  - Range : 0~2550, precision is 10ms, ones place is omitted.
- notificationType
  - Definition : Notification condition
  - Type : Always, OnChanged
- timeOutSec
  - Definition: timeout (in seconds)
  - Range : 0.5~
- callback
  - Definition : end callback (set success flag, cube)
- order
  - Definition : [instruction priority](sys_cube.md#4-Send-Command)
  - Type : Weak, Strong

<br>

### RequestMagneticSensor

```csharp
public void RequestMagneticSensor(ORDER_TYPE order = ORDER_TYPE.Strong);
````

Requests the cube to notify the magnetic sensor information once. <br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_magnetic_sensor#requests-for-magnetic-sensor-information)

- order
  - Definition : [instruction priority](sys_cube.md#4-Send-Command)
  - Type : Weak, Strong

<br>

### RequestAttitudeSensor

```csharp
public void RequestAttitudeSensor(AttitudeFormat format, ORDER_TYPE order = ORDER_TYPE.Strong);
````

Requests that the cube be notified once of the specified type of attitude angle detection information. <br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/en/docs/ble_high_precision_tilt_sensor#requesting-posture-angle-detection)

- format
  - Definition : Type of notification content
  - Type : Eulers, Quaternion
- order
  - Definition : [instruction priority](sys_cube.md#4-Send-Command)
  - Type : Weak, Strong

<br>


# 4. Cube connection settings

The internal implementation of communication connection is divided into simulator implementation and real implementation, and the connection method can be changed by specifying `ConnectType` in the constructor argument of the communication related class.

- Auto`, the internal implementation will automatically change depending on the build target.<br>
- In the case of Simulator setting (`ConnectType.Simulator`), Simulator cube will work regardless of the build target.<br>
- Real setting (`ConnectType.Real`), the real cube will work regardless of the build target.

### Definition

```csharp
public enum ConnectType
{
    Auto, // The internal implementation changes automatically depending on the build target.
    Simulator, // Runs in Simulator cube regardless of the build target
    Real // Works with real (real) cubes regardless of the build target
}
```

```csharp
public NearestScanner(ConnectType type = ConnectType.Auto);

public NearScanner(int satisfiedNum, ConnectType type = ConnectType.Auto);

public CubeScanner(ConnectType type = ConnectType.Auto);

public CubeConnecter(ConnectType type = ConnectType.Auto);

public CubeManager(ConnectType type = ConnectType.Auto);
```

### Sample Code

```csharp
Cube[] cubes;
async void Start()
{
    // Auto is the default value if no argument is specified
    // The internal implementation changes automatically depending on the build target, so you don't need to write separate code for each platform.
    var peripherals = await new CubeScanner().NearScan(2);
    cube = await new CubeConnecter().Connect(peripherals);
}
```

```csharp
CubeManager cubeManager;
async void Start()
{
    // Auto is the default value if no argument is specified
    // The internal implementation changes automatically depending on the build target, so you don't need to write separate code for each platform.
    cubeManager = new CubeManager();
    await cubeManager.MultiConnect(2);
}
```

```csharp
Cube[] cubes;
async void Start()
{
    // Connect to Simulator cube on any platform
    var peripherals = await new CubeScanner(ConnectType.Simulator).NearScan(2);
    cube = await new CubeConnecter(ConnectType.Simulator).Connect(peripherals);
}
```

```csharp
CubeManager cubeManager;
async void Start()
{
    // Connect to a real cube on any platform.
    cubeManager = new CubeManager(ConnectType.Real);
    await cubeManager.MultiConnect(2);
}
```

### Sample Projects

Please refer to [Sample_ConnectType](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_ConnectType/README_EN.md)