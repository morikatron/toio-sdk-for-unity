## Sample_VisualizeNavigator

> Debugging module is not available in WebGL version.

<div align="center">
<img src="/docs/res/samples/visualize_navigator.gif">
</div>

<br>

This sample visualizes the results of HLAvoid calculation of CubeNavigator and all defined walls.

The target coordinates of Cube (pink bars) can be set by holding down `CTRL` and right-clicking.

To turn on visualization, turn on "Gismo" in the upper right corner of "Scene" or "Game" window in Unity Editor.

- The black line represents the position of the Wall. Margins are not represented.
- The green line connects the optimal waypoint calculated by HLAvoid to Cube.
- The red lines connect waypoints and Cubes that were eliminated due to collision conditions.
- The blue line connects the other candidate waypoints to Cube.
