# Technical Documentation - Usage - Cube Class

## Table of Contents

- [1. Outline](usage_cube.md#1-outline)
- [2. Comparison with existing toio™ library (toio.js)](usage_cube.md#2-comparison-with-existing-toio™-library-toiojs)
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

In real Cube, you control real Cube through Bluetooth communication from Unity program according to the [toio™ Core Cube Technical Specification (Communication Specification)](https://toio.github.io/toio-spec/docs/ble_communication_overview.html).

### Real/Sim Performance Meter

Currently (12/15/2020), there are 3 firmware versions in Cube.

`2.0.0`　`2.1.0`　`2.2.0`

toio SDK for Unity has two internal implementations: Cube class that runs in real life (Real-compatible) and Cube class that runs in Simulator (Sim-compatible). Since the internal implementations are different, there are differences in the support status.<br>
The following table shows the implementation correspondence.

#### Firmware version 2.0.0

| Function Type         | Function                                                                                                                           | Real Support Status | Sim Support Status |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------ | ------------- | ------------ |
| Readout sensor   | [Position ID](https://toio.github.io/toio-spec/docs/2.0.0/ble_id#position-id)                                                  | o             | o            |
|                    | [Standard ID](https://toio.github.io/toio-spec/docs/2.0.0/ble_id#standard-id)                                                  | o             | o            |
| Motion sensor | [Horizontal detection](https://toio.github.io/toio-spec/docs/2.0.0/ble_sensor#水平検出)                                                    | o             | o            |
|                    | [Collision detection](https://toio.github.io/toio-spec/docs/2.0.0/ble_sensor#衝突検出)                                                    | o             | ※            |
| Button             | [Button status](https://toio.github.io/toio-spec/docs/2.0.0/ble_button#ボタンの状態)                                            | o             | o            |
| Battery         | [Battery life](https://toio.github.io/toio-spec/docs/2.0.0/ble_battery#バッテリー残量)                                       | o             | x            |
| Motor           | [Motor control](https://toio.github.io/toio-spec/docs/2.0.0/ble_motor#モーター制御)                                             | x             | x            |
|                    | [Motor control with time specification](https://toio.github.io/toio-spec/docs/2.0.0/ble_motor#時間指定付きモーター制御)                     | o             | o            |
| Lamp             | [Turn on/off](https://toio.github.io/toio-spec/docs/2.0.0/ble_light#点灯-消灯)                                                  | o             | o            |
|                    | [Continuous on/off](https://toio.github.io/toio-spec/docs/2.0.0/ble_light#連続的な点灯-消灯)                                  | o             | o            |
|                    | [Turn off all lamps.](https://toio.github.io/toio-spec/docs/2.0.0/ble_light#全てのランプを消灯)                                 | x             | x            |
|                    | [Turn off specific lamps.](https://toio.github.io/toio-spec/docs/2.0.0/ble_light#特定のランプを消灯)                                 | x             | x            |
| Sound           | [Playback of sound effects](https://toio.github.io/toio-spec/docs/2.0.0/ble_sound#効果音の再生)                                             | o             | o            |
|                    | [Playback MIDI note number](https://toio.github.io/toio-spec/docs/2.0.0/ble_sound#midi-note-number-の再生)                       | o             | o            |
|                    | [Stop playback](https://toio.github.io/toio-spec/docs/2.0.0/ble_sound#再生の停止)                                                 | o             | o            |
| Settings               | [Requesting BLE protocol version](https://toio.github.io/toio-spec/docs/2.0.0/ble_configuration#ble-プロトコルバージョンの要求) | o             | x            |
|                    | [Threshold setting for horizontal detection](https://toio.github.io/toio-spec/docs/2.0.0/ble_configuration#水平検出のしきい値設定)                 | o             | o            |
|                    | [Threshold setting for collision detection](https://toio.github.io/toio-spec/docs/2.0.0/ble_configuration#衝突検出のしきい値設定)                 | o             | x            |
|                    | [Get BLE protocol version](https://toio.github.io/toio-spec/docs/2.0.0/ble_configuration#ble-プロトコルバージョンの取得) | o             | x            |

> ... The detection function is not implemented on Simulator side, but you can manually switch the judgment on and off from the inspector. For more details, please refer to [[here](usage_simulator.md#41-inspector-in-cubesimulator)].

#### Firmware version 2.1.0

| Function Type         | Function                                                                                                                                | Real Support Status | Sim Support Status |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------- | ------------- | ------------ |
| Motion sensor | [Double-tap detection](https://toio.github.io/toio-spec/docs/ble_sensor#ダブルタップ検出)                                        | o             | ※            |
|                    | [Attitude detection](https://toio.github.io/toio-spec/docs/ble_sensor#姿勢検出)                                                        | o             | o            |
| Motor           | [Motor control (instruction value range change)](https://toio.github.io/toio-spec/docs/ble_motor#モーターの速度指示値)                       | o             | o            |
|                    | [Motor control with target specification](https://toio.github.io/toio-spec/docs/ble_motor#目標指定付きモーター制御)                         | o             | o            |
|                    | [Motor control with multiple target specification](https://toio.github.io/toio-spec/docs/ble_motor#複数目標指定付きモーター制御)                 | x             | x            |
|                    | [Motor control with acceleration specification](https://toio.github.io/toio-spec/docs/ble_motor#加速度指定付きモーター制御)                     | o             | o            |
|                    | [Response of motor control with target specification](https://toio.github.io/toio-spec/docs/ble_motor#目標指定付きモーター制御の応答)             | o             | o            |
|                    | [Response of motor control with multiple target specification](https://toio.github.io/toio-spec/docs/ble_motor#複数目標指定付きモーター制御の応答)     | x             | x            |
| Settings               | [Setting the time interval for double-tap detection](https://toio.github.io/toio-spec/docs/ble_configuration#ダブルタップ検出の時間間隔の設定) | o             | x            |

#### Firmware version 2.2.0

| Function Type         | Function                                                                                                                                       | Real Support Status | Sim Support Status |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------ | ------------- | ------------ |
| Motion sensor  | [Request motion sensor information](https://toio.github.io/toio-spec/docs/ble_sensor#モーション検出情報の要求)                                       | o             | o            |
|                   | [Shake detection](https://toio.github.io/toio-spec/docs/ble_sensor#シェイク検出)                                                      | o             | o            |
| Magnetic sensors        | [Request for magnetic sensor information](https://toio.github.io/toio-spec/docs/ble_magnetic_sensor#磁気センサー情報の要求)                          | x             | x            |
|                    | [Acquisition of magnetic sensor information](https://toio.github.io/toio-spec/docs/ble_magnetic_sensor#磁気センサー情報の取得)                          | x             | x            |
| Motor           | [Obtaining motor speed information](https://toio.github.io/toio-spec/docs/ble_motor#モーターの速度情報の取得)                                | o             | o            |
| Settings               | [Setting up magnetic sensor](https://toio.github.io/toio-spec/docs/ble_configuration#磁気センサーの設定)                                    | x             | x            |
|                    | [Magnetic sensor setting response](https://toio.github.io/toio-spec/docs/ble_configuration#磁気センサーの設定の応答)                       | x             | x            |
|                    | [Setting up the acquisition of motor speed information](https://toio.github.io/toio-spec/docs/ble_configuration#モーターの速度情報の取得の設定)            | o             | o            |
|                    | [Motor speed information acquisition setting response](https://toio.github.io/toio-spec/docs/ble_configuration#モーターの速度情報の取得の設定の応答)| o             | o            |

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

```C#
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

```c#
// Firmware version of the connected cube
public string version { get; }

// Unique identification ID of Cube
// In simulator environment, InstanceID of Cube game object becomes ID
public string id { get; protected set; }

// Address of Cube
public string addr { get; }

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
// It is provided for each firmware version differently.
public int maxSpd { get; }

// Variable representing the minimum speed of Cube
// It is provided for each firmware version differently.
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
```

<br>

## 3.2. callback

```C#
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
public virtual CallbackProvider<Cube> buttonCallback { get; }
// Tilt callback
public virtual CallbackProvider<Cube> slopeCallback { get; }
// Collision callback
public virtual CallbackProvider<Cube> collisionCallback { get; }
// Coordinate angle callback
public virtual CallbackProvider<Cube> idCallback { get; }
// Coordinate angle Missed callback
public virtual CallbackProvider<Cube> idMissedCallback { get; }
// StandardID callback
public virtual CallbackProvider<Cube> standardIdCallback { get; }
// StandardID missed callback
public virtual CallbackProvider<Cube> standardIdMissedCallback { get; }

// ver2.1.0
// Double tap callback
public virtual CallbackProvider<Cube> doubleTapCallback { get; }
// Attitude detection callback
public virtual CallbackProvider<Cube> poseCallback { get; }
// Response callback for motor control with target specification
public virtual CallbackProvider<Cube, int, TargetMoveRespondType> targetMoveCallback { get; }

// ver2.2.0
// Shake callback
public virtual CallbackProvider<Cube> shakeCallback { get; }
// Motor speed callback
public virtual CallbackProvider<Cube> motorSpeedCallback { get; }
```

## 3.3. Method

### Move

```C#
public void Move(int left, int right, int durationMs, ORDER_TYPE order=ORDER_TYPE.Weak);
```

Controls Cube's motor.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_motor#時間指定付きモーター制御)

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
    - 10~2550 : Accuracy is 10ms, the first place is omitted
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### TurnLedOn

```C#
public void TurnLedOn(int red, int green, int blue, int durationMs, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Controls the LEDs on the bottom of Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_light#点灯-消灯)

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

```C#
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
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_light#連続的な点灯-消灯)

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

```C#
public void TurnLedOff(ORDER_TYPE order=ORDER_TYPE.Strong);
```

Turn off the LED on the bottom of Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_light#全てのランプを消灯)

- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### PlayPresetSound

```C#
public void PlayPresetSound(int soundId, int volume=255, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Play the sound effects provided in Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_sound#効果音の再生)

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
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生)

```C#
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

```C#
// Buffered version (e.g. of a file)
public void PlaySound(byte[] buff, ORDER_TYPE order=ORDER_TYPE.Strong);
```

- buff
  - Definition : Data block defined by[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生)
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### StopSound

```C#
public void StopSound(ORDER_TYPE order=ORDER_TYPE.Strong);
```

Stops Cube from playing sound.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_sound#再生の停止)

- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### ConfigSlopeThreshold

```C#
public void ConfigSlopeThreshold(int angle, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Sets the threshold for horizontal detection of Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_configuration#水平検出のしきい値設定)

- angle
  - Definition : Threshold for tilt detection
  - Range : 1~45
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### ConfigCollisionThreshold

```C#
public void ConfigCollisionThreshold(int level, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Sets the threshold for collision detection for Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_configuration#衝突検出のしきい値設定)

- level
  - Definition: Collision detection threshold
  - Range : 1~10
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### ConfigDoubleTapInterval

```C#
public void ConfigDoubleTapInterval(int interval, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Set the time interval for double-tap detection of Cube <br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_configuration#ダブルタップ検出の時間間隔の設定)

- interval
  - Definition: Time interval for double-tap detection
  - Range : 1~7
- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong

<br>

### TargetMove

```C#
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
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_motor#目標指定付きモーター制御)

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

```C#
public void AccelerationMove(
            int targetSpeed,
            int acceleration,
            ushort rotationSpeed = 0,
            AccPriorityType accPriorityType = AccPriorityType.Translation,
            byte controlTime = 0,
            ORDER_TYPE order = ORDER_TYPE.Strong);
```

Performs motor control with acceleration specified for Cube.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_motor#加速度指定付きモーター制御)

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

```C#
public UniTask ConfigMotorRead(bool valid, float timeOutSec=0.5f, Action<bool,Cube> callback=null, ORDER_TYPE order=ORDER_TYPE.Strong);
```

Enables or disables the acquisition of Cube's motor speed information.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_configuration#モーターの速度情報の取得の設定)

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

```C#
public void RequestSensor(ORDER_TYPE order = ORDER_TYPE.Strong);
```

Requests Cube to notify you of motion sensor information once.<br>
[toio™ Core Cube Technical Specifications (Communication Specifications)](https://toio.github.io/toio-spec/docs/ble_sensor#モーションセンサー情報の要求)

> Since collision detection and double tap detection are notified only when they occur, it is impossible for the variable `Cube.isCollisionDetected` `Cube.isDoubleTap` to return from the `True` state to `False` unless it is notified by another motion sensor. Therefore, it is possible to update these two variables by using `RequestSensor` to ask for notification.

- order
  - Definition : [instruction priority](sys_cube.md#4-send-command)
  - Type : Weak, Strong
  
<br>

# 4. Cube connection settings

The internal implementation of communication connection is divided into simulator implementation and real implementation, and the connection method can be changed by specifying `ConnectType` in the constructor argument of the communication related class.

- Auto`, the internal implementation will automatically change depending on the build target.<br>
- In the case of Simulator setting (`ConnectType.Simulator`), Simulator cube will work regardless of the build target.<br>
- Real setting (`ConnectType.Real`), the real cube will work regardless of the build target.

### Definition

```C#
public enum ConnectType
{
    Auto, // The internal implementation changes automatically depending on the build target.
    Simulator, // Runs in Simulator cube regardless of the build target
    Real // Works with real (real) cubes regardless of the build target
}
```

```C#
public NearestScanner(ConnectType type = ConnectType.Auto);

public NearScanner(int satisfiedNum, ConnectType type = ConnectType.Auto);

public CubeScanner(ConnectType type = ConnectType.Auto);

public CubeConnecter(ConnectType type = ConnectType.Auto);

public CubeManager(ConnectType type = ConnectType.Auto);
```

### Sample Code

```C#
Cube[] cubes;
async void Start()
{
    // Auto is the default value if no argument is specified
    // The internal implementation changes automatically depending on the build target, so you don't need to write separate code for each platform.
    var peripherals = await new CubeScanner().NearScan(2);
    cube = await new CubeConnecter().Connect(peripherals);
}
```

```C#
CubeManager cubeManager;
async void Start()
{
    // Auto is the default value if no argument is specified
    // The internal implementation changes automatically depending on the build target, so you don't need to write separate code for each platform.
    cubeManager = new CubeManager();
    await cubeManager.MultiConnect(2);
}
```

```C#
Cube[] cubes;
async void Start()
{
    // Connect to Simulator cube on any platform
    var peripherals = await new CubeScanner(ConnectType.Simulator).NearScan(2);
    cube = await new CubeConnecter(ConnectType.Simulator).Connect(peripherals);
}
```

```C#
CubeManager cubeManager;
async void Start()
{
    // Connect to a real cube on any platform.
    cubeManager = new CubeManager(ConnectType.Real);
    await cubeManager.MultiConnect(2);
}
```

### Sample Projects

Please refer to [Sample_ConnectType](../toio-sdk-unity/Assets/toio-sdk/Samples/Sample_ConnectType/)