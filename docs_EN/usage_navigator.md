# Technical Documentation - Usage - Navigator Class

##  Table of Contents

- [1. Outline](usage_navigator.md#1-outline)
- [2. CubeNavigator class API](usage_navigator.md#2-cubenavigator-class-api)
  - [2.1. Enumerated types](usage_navigator.md#21-enumerated-types)
  - [2.2. Parameters](usage_navigator.md#22-parameters)
  - [2.3. Properties](usage_navigator.md#23-properties)
  - [2.4. NaviResult structure](usage_navigator.md#24-naviresult-structure)
  - [2.5. Methods](usage_navigator.md#25-methods)
- [3. HLAvoid class API](usage_navigator.md#3-hlavoid-class-api)
  - [3.1. Scan result structure ScanResult](usage_navigator.md#31-scan-result-structure-scanresult)
  - [3.2. Parameters](usage_navigator.md#32-parameters)
  - [3.3. Information that can be obtained](usage_navigator.md#33-information-that-can-be-obtained)
  - [3.4. Method](usage_navigator.md#34-method)
- [4. Boids class API](usage_navigator.md#4-boids-class-api)
  - [4.1. Parameters](usage_navigator.md#41-parameters)
  - [4.2. Method](usage_navigator.md#42-method)

# 1. Outline

Navigator is an algorithm designed to help multiple robots (Cubes) navigate well in the presence of each other.
Navigator is an algorithm designed to help multiple robots (Cubes) move well while taking each other's movements into account.

This algorithm is mainly based on two algorithms: "Human Like Collision Avoidance" (HLAvoid) and "Boids" (Boids).

- HLAvoid is a natural way to avoid
- Boids are a herding, synchronized behavioral technique.

All classes implemented in Navigator belong to `toio.Navigation` namespace.

### Class Diagrams

<div align="center"><img width=600 src="res/navigator/arch.png"></div>

Navigator class uses two algorithms, HLAvoid and Boids.<br>
CubeNavigator class inherits from the Navigator class and works with CubeHandle class as an interface.

For details, please refer to [Document describing the functions of Navigator](sys_navigator.md).

### Mode

There are three modes (Navigator.mode) in Navigator class.

- AVOID: Collision avoidance only
- BOIDS: Boids only
- BOIDS_AVOID: Combination of Boids and collision avoidance

<br>

# 2. CubeNavigator class API

When the user uses the navigation functions, it is CubeNavigator class, which inherits from Navigator class, that is directly touched.

## 2.1. Enumerated types

```c#
// Mode of navigator
public enum Mode : byte
{
    AVOID = 0,
    BOIDS = 1,
    BOIDS_AVOID = 2,
}

// Do you recognize others as Boids?
public enum Relation : byte
{
    NONE = 0,   // Do not Boids others.
    BOIDS = 1,  // Boids others, synchronize movement.
}
```

## 2.2. Parameters

```c#
// CubeNavigator
public bool usePred = false;    // Whether to use CubeHandle predictions.

// Navigator
public Mode mode = Mode.AVOID;

// for Navigator.GetWaypointTo
public double p_surrogate_target = 1;       // In BOIDS_AVOID mode, the influence coefficient of Boids direction.
public double p_speedratio_boidsavoid = 1;  // Effect factor of Boids on velocity coefficient in BOIDS_AVOID mode
public double p_speedratio_boids = 1;       // Effect factor of Boids on velocity coefficient in BOIDS mode
```

## 2.3. Properties

```c#
public Cube cube{ get; }
public CubeHandle handle{ get; }
public NaviResult result{ get; }    // Save the calculation results

// Properties inherited from Navigator
public Entity entity { get; }   // By oneself
public Boids boids { get; }     // Boids Algorithm
public HLAvoid avoid { get; }   // Avoidance algorithm
```

## 2.4. NaviResult structure

A structure with the results of the navigator's calculations.<br>
It has the following properties

```c#
public Vector waypoint { get; }     // Waypoint
public double speedRatio { get; }   // Velocity coefficient (from Boids) (default value is 1)
public double speedLimit { get; }   // Upper speed limit (from avoidance) (default value is the maximum value of Double)
public bool isCollision { get; }    // Collision condition

public Mode mode { get; }           // Navigator mode (backup information)
public Vector avoidVector { get; }  // Result vector of avoidance (backup information)
public Vector boidsVector { get; }  // Boids result vector (backup information)
```

## 2.5. Methods

### Get and set up a wall

> Please refer to the samples [Sample_VisualizeNavigator](/toio-sdk-unity/Assets/toio-sdk/Samples/Sample_VisualizeNavigator/) and [Sample_MultiMat](/toio-sdk-unity/Assets/toio-sdk/Samples/Sample_MultiMat/).

#### Walls

```c#
public System.Collections.Generic.IEnumerable<Wall> Walls();
```

Get the iterator of the wall.

#### AddWall

```c#
public void AddWall(Wall wall);
```

Add a wall.

- wall
  - 定義：壁

```c#
public void AddWall(List<Wall> walls);
```

Add multiple walls.

- walls
  - Definition of: wall list

#### RemoveWall

```c#
public void RemoveWall(Wall wall);
```

Remove the wall.

- wall
  - Definition: Wall

#### ClearWall

```c#
public void ClearWall();
```

Remove all walls.

#### AddBorder

```c#
public void AddBorder(int width=60, int x1=0, int x2=500, int y1=0, int y2=500);
```

If you want to add a border to the mat, the method is to create a wall on the east, west, north, south, and west all at the same time.<br>
When CubeNavigator is instantiated, `AddBorder(70);` is automatically called.

- width
  - Definition: half the thickness of a wall
  - Default value: 60
- x1
  - Definition: x-coordinate of the center of the first wall in the y-direction
  - Default value: 0
- x2
  - Definition: x-coordinate of the center of the second wall in the y-direction
  - Default value: 500
- y1
  - Definition: y-coordinate of the center of the first wall in the x-direction
  - Default value: 0
- y2
  - Definition: y-coordinate of the center of the second wall in the x direction
  - Default value: 500

Using the default values as an example, the x-coordinates -60 ~ 60, 440 ~ 560 and y-coordinates -60 ~ 60, 440 ~ 560 will be walled off and avoided by the navigator.

```c#
public void AddBorder(int width, RectInt rect);
```

- rect
  - Definition: Border position RectInt<br>
  [Mat.GetRectForMatType](usage_simulator.md#24-method) to get the RectInt for the size of the mat.


### Set up recognizable others.

#### AddOther

```c#
public void AddOther(Navigator other, Relation relation=Relation.BOIDS);
```

Make it possible to recognize other navigators.

- other
  - Definition of: other Navigator
- relation
  - Definition: void or not void
  - Default value: [Relation.BOIDS](usage_navigator.md#21-enumerated-types)

```c#
public void AddOther(List<CubeNavigator> others, Relation relation=Relation.BOIDS);
public void AddOther(List<Navigator> others, Relation relation=Relation.BOIDS);
```

Allows multiple other navigators to be recognized.

- others
  - Definition of: other Navigator list

#### RemoveOther

```c#
public void RemoveOther(Navigator other);
```

Makes it impossible to recognize other navigators.

- other
  - Definition of: other Navigator

#### ClearOther

```c#
public void ClearOther();
```

Deletes all recognizable targets.

#### ClearGNavigators

```c#
public static void ClearGNavigators();
```

Clears the `gNavigators` static list with all CubeNavigator.

`gNavigators` is a list to set the others to be recognized automatically when CubeNavigator is newly instantiated.

When CubeNavigator instance is recreated, either call this method to clear it, or each CubeNavigator instance should be set manually with `ClearOther` and `AddOther`.

### Set others to be Boids

#### SetRelation

```c#
public void SetRelation(Navigator other, Relation relation);
```

Set whether other navigators should be Boids or not.

- other
  - Definition of: other Navigator
- relation
  - Definition: Boids or not BoidS [(Relation)](usage_navigator.md#21-enumerated-types)

```c#
public void SetRelation(List<CubeNavigator> others, Relation relation);
public void SetRelation(List<Navigator> others, Relation relation);
```

Set whether multiple other navigators should be Boids or not.

- others
  - Definition of: other Navigator list

### Updating state

```c#
public void Update();
```

Update the state of Cube used for the calculation.

In the frame where the navigation is to be calculated, run it once before the calculation.

```c#
public void Update(bool usePred);
```

Update the state of Cube used for the calculation, with or without state prediction.

- usePred
  - Definition: with or without state prediction

### Calculating Waypoints

#### GetWaypointTo

```c#
public NaviResult GetWaypointTo(double x, double y);
```

Calculates the waypoints to be moved to the target coordinates.

- x
  - Definition: target x coordinates
  - Range: Any
- y
  - Definition: target y coordinates
  - Range: Any
- Return value
  - Definition: navigation calculation result [(NaviResult)]((usage_navigator.md#24-naviresult-structure)

```c#
public NaviResult GetWaypointTo(Vector pos);
public NaviResult GetWaypointTo(Vector2 pos);
public NaviResult GetWaypointTo(Vector2Int pos);
```

Calculates the waypoints to be moved to the target coordinates.

- pos
  -Target coordinate

```c#
public NaviResult GetWaypointTo(Entity target);
public NaviResult GetWaypointTo(Navigator target);
```

Calculate the waypoints to move to the target individual.

- target
  - Target Individuals

#### GetWaypointAway

```c#
public NaviResult GetWaypointAway(double x, double y);
```

Calculates the waypoints away from the target coordinates.

- x
  - Definition: target x coordinates
  - Range: Any
- y
  - Definition: target y coordinates
  - Range: Any
- Return value
  - Definition: navigation calculation result [(NaviResult)]((usage_navigator.md#24-naviresult-structure)

```c#
public NaviResult GetWaypointAway(Vector pos);
public NaviResult GetWaypointAway(Vector2 pos);
public NaviResult GetWaypointAway(Vector2Int pos);
```

Calculates the waypoints away from the target coordinates.

- pos
  - Target coordinate

```c#
public NaviResult GetWaypointAway(Entity target);
public NaviResult GetWaypointAway(Navigator target);
```

Calculate the waypoints away from the target individual.

- target
  - Target Individuals

<br>

Sample Code

```c#
// Calculate waypoints
NaviResult res = cubeNavigator.GetWaypointTo(x, y);
// Adjust target speed targetSpd to spd
double spd = Min(res.speedLimit, targetSpd * res.speedRatio);
// Calculate the Movement towards the waypoint
Movement mv = cubeNavigator.handle.Move2Target(res.waypoint, maxSpd:spd);
// Execute Movement
mv.Exec();
```

<br>

### Run navigator.

It integrates the process of calculating the aforementioned waypoints and using the calculation results to calculate Movement, and provides an easy-to-use method.

If you do not want to customize the calculation process, please use this method directly.

#### Navi2Target

```c#
public virtual Movement Navi2Target(double x, double y, int maxSpd=70, int rotateTime=250, double tolerance=20);
```

Calculates the Movement to navigate to the target coordinates.

- x
  - Definition: target x coordinates
  - Range: Any
- y
  - Definition: target y coordinates
  - Range: Any
- maxSpd
  - Definition: maximum speed indication
  - Range:
    - [Version 2.0.0] 0~100
    - [Version 2.1.0, 2.2.0] 0~115
  - Default value: 70
- rotateTime
  - Definition: desired rotation time (ms)
  - Range:100 ~ 2550
  - Default value: 250
  - Description: Specifies the time to rotate toward the target.<br>
    A smaller value will make it spin faster, while a larger value will make it spin slower. This is not an exact rotation time, but a rough guide.<br>
    If the value is less than 200, the system may become unstable.
- tolerance
  - Definition: Threshold of arrival judgment (distance)
  - Default value: 20
  - Explanation: When the distance to the target is less than tolerance, it is considered to be reached.
- Return value
  - Definition: Move command [(Movement)](usage_cubehandle.md#22-Movement-movement-structure)

```c#
public virtual Movement Navi2Target(Vector pos, int maxSpd=70, int rotateTime=250, double tolerance=20);
public virtual Movement Navi2Target(Vector2 pos, int maxSpd=70, int rotateTime=250, double tolerance=20);
public virtual Movement Navi2Target(Vector2Int pos, int maxSpd=70, int rotateTime=250, double tolerance=20);
```

Calculates the Movement to navigate to the target coordinates.

- pos
  - Definition: Target coordinates

#### NaviAwayTarget

```c#
public virtual Movement NaviAwayTarget(double x, double y, int maxSpd=70, int rotateTime=250);
```

Calculates the Movement to navigate away from the target coordinates.

- x
  - Definition: target x coordinates
  - Range: Any
- y
  - Definition: target y coordinates
  - Range: Any
- maxSpd
  - Definition: maximum speed indication
  - Range:
    - [Version 2.0.0] 0~100
    - [Version 2.1.0, 2.2.0] 0~115
  - Default value: 70
- rotateTime
  - Definition: desired rotation time (ms)
  - Range:100 ~ 2550
  - Default value: 250
  - Description: Specifies the time to rotate toward the target.<br>
    A smaller value will make it spin faster, while a larger value will make it spin slower. This is not an exact rotation time, but a rough guide.<br>
    If the value is less than 200, the system may become unstable.
- Return value
  - Definition: Move command [(Movement)](usage_cubehandle.md#22-Movement-movement-structure)
  - 説明：

The Movement.reached returned by Navi2Target is judged by the distance to the target, but since there is no clear definition of "reached" in the case of NaviAwayTarget, it returns the Movement of Move2Target that moves to the waypoint.

```c#
public virtual Movement NaviAwayTarget(Vector pos, int maxSpd=70, int rotateTime=250);
public virtual Movement NaviAwayTarget(Vector2 pos, int maxSpd=70, int rotateTime=250);
public virtual Movement NaviAwayTarget(Vector2Int pos, int maxSpd=70, int rotateTime=250);
```

Calculates the Movement to navigate away from the target coordinates.

- pos
  - Definition: Target coordinates

<br>

Simplified sample code

```c#
// Calculate the Movement to navigate to the target.
Movement mv = cubeNavigator.Navi2Target(x, y);
// Execution
mv.Exec();
```

Or

```c#
// Calculate and execute the Movement to navigate to the target.
Movement mv = cubeNavigator.Navi2Target(x, y).Exec();
```

<br>

# 3. HLAvoid class API

This class implements a collision avoidance algorithm.

Since CubeNavigator class has an instance of HLAvoid To change the parameters of HLAvoid or to get information from CubeNavigator class, do the following

```c#
CubeNavigator navigator = ...
// As an example, change the parameter range
navigator.avoid.range = 220;
```

## 3.1. Scan result structure ScanResult

```c#
public struct ScanResult
{
    public bool isCollision;    // Collision condition
    public double[] rads;       // Scanning direction
    public double[] dists;      // Distance
    public double[] safety;     // Safety
    public Vector[] points;     // Relative coordinates of points determined by direction and distance.

    // For creating an initialized ScanResult.
    public static ScanResult init(double[] rads, double maxRange);
    // For debugging
    public void print(Action<string> func);
    // Calculate points with rads and dists
    public void calcPoints();
}
```

## 3.2. Parameters

```c#
public double range = 200;  // Visible distance
public int nsample = 19;    // Number of angles when scanning the perimeter, recommend an odd number.
public double margin = 22;  // Own margin for evasion

// for RunTowards
public bool useSafety = true;                       // Whether to use ScanResult.safety
public double p_waypoint_safety_threshold = 0.15;   // Threshold of safety for waypoint selection

// for RunAway
public double p_runaway_penalty_away_k = 5;     // Penalties for staying away from the goal
public double p_runaway_penalty_keeprad_k = 10; // Penalty for maintaining direction
public double p_runaway_range = 250;            // Enlarge the scan result by p_runaway_range/range times
```

## 3.3. Information that can be obtained

This is useful for debugging.

```c#
// The latest calculation results saved.
public ScanResult scanResult;   // Scan Results
public int waypointIndex = 0;   // Index of the selected waypoint.
```

## 3.4. Method

It is called by GetWaypointTo and GetWaypointAway in Navigator.

There is no need to call it directly.

### RunTowards

```c#
public virtual (Vector, bool, double) RunTowards(List<Navigator> others, Entity target, List<Wall> walls);
```

Navigate to the target individual.

- others
  - Definition of: other Navigator list
- target
  - Definition: Target individual
- walls
  - Definition of: wall list
- Return value: Waypoints, collision status, and speed limits

### RunAway

```c#
public virtual (Vector, bool, double) RunAway(List<Navigator> others, Entity target, List<Wall> walls);
```

Navigate away from the target individual.

- others
  - Definition of: other Navigator list
- target
  - Definition: Target individual
- walls
  - Definition of: wall list
- Return value: Waypoints, collision status, and speed limits

<br>

# 4. Boids class API

This class implements Boids algorithm.

Since CubeNavigator class has an instance of Boids, you can change the parameters of Boids and get information about them from CubeNavigator class as follows

```c#
CubeNavigator navigator = ...
// As an example, change the parameter range
navigator.boids.range = 180;
```

## 4.1. Parameters

```c#
public double fov = Deg2Rad(120);       // Field of vision
public double range = 150;              // Visible distance
public double margin = 25;              // Margin for Boids

public double p_weight_attraction = 50; // Weight of the vector to move to the target
public double p_weight_separation = 1;  // Separation vector weights
public double p_max_separation = 100;   // Upper limit of vector separation
public double p_weight_cohesion = 0.3;  // Weight of the vector to be moved to the average position
public double p_max_cohesion = 50;      // Upper limit of vector to move to average position
public double p_weight_alignment = 0.3; // Vector weights toward the mean direction
public double p_max_alignment = 30;     // Upper limit of the vector going to the mean direction
public double p_max_all = 100;          // upper limit of the combined vector
```

## 4.2. Method

### Run

```c#
public Vector Run(List<Navigator> others, Vector tarPos);
```

Include the vector to the goal and calculate composite vector.

- others
  - Definition of: other Navigator list
- tarPos
  - Definition: Target coordinates
- Return value: Composite vector

```c#
// Compute composite vector, no target
public Vector Run(List<Navigator> others);
```

Calculate composite vector, except for the vector to the target.

- others
  - Definition of: other Navigator list
- Return value: Composite vector
