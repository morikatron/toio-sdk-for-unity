## Sample_Scenes

This sample handles scene transitions while maintaining the connection to real Cube and the correlated instance with Cube on Simulator.

It is made up of three scenes:
- `preload`:The first scene to load. This scene should be able to hold the game objects and instances that all the scenes share.
- `scene1`:A scene with the meaning "Home", connected to Cube by CubeManager.
- `scene2`:Normal scene。

To run and build
- In "Build Setting", add the above three scenes, and set 'preload' to 0.

### Technical Highlights

#### Holding game objects

**Problem**:Real Cubes continue to exist regardless of scene transitions, while simulator Cubes (game objects) are scene dependent. The result is that the same code cannot be used in both the real and simulator due to scene transitions.

The following methods can be used to move and hold game objects in a common space independent of the scene.

> [DontDestroyOnLoad(UnityEngine.Object object)](https://docs.unity3d.com/2020.3/Documentation/ScriptReference/Object.DontDestroyOnLoad.html)

```c#
// Smaple_Scenes_Preload.cs
void Start(){
    // ...
#if UNITY_EDITOR
    // Keep Cubes across scenes
    foreach (var c in GameObject.FindGameObjectsWithTag("Cube"))
        DontDestroyOnLoad(c);
#endif
    // ...
}
```

However, when a scene is loaded, any objects in the scene will be created anew.
If you load an object from a scene using `DontDestroyOnLoad` and then load the same scene again, the same object will be created again.

As a countermeasure, it is possible to delete Cube object by `Find`, but in this sample, **we recommend to create and keep Cube object in the `Preload` scene and do not place it in other scenes.**

#### Maintaining connections and Cube related instances

**Problem**:The class that manages the connection to the real cube, Cube class, CubeHandle, and CubeNavigator together is CubeManager. If you do not keep CubeManager and make a scene transition, there will be no entry point to call the original instance, so in the real case, the connection will not be broken and you will not be able to reconnect, and Cube will be uncontrollable.

If you have an instance of CubeManager as a static variable of a class, you can use it repeatedly.

```c#
// Sample_Scenes_Preload.cs
public class Sample_Scenes_Preload : MonoBehaviour
{
    // ...
    public static CubeManager cm;
    // ...
}

// Sample_Scenes_scene1.cs
public class Sample_Scenes_scene1 : MonoBehaviour
{
    private CubeManager cm;
    async void Start()
    {
        cm = Sample_Scenes_Preload.cm;
    }
    // ...
}
```

As an alternative, you can add a callback to `SceneManager.sceneLoaded` and pass an instance of `CubeManager` in the callback.

#### Script retention

If the user has created a script to control Cube and wants to keep it, he can attach the script to a single empty object and `DontDestroyOnLoad` the object.

In the inspector of scene `preload`, if you check the `keep_script` variable of `Sample_Scenes_Preload`, `Sample_Scenes_Preload.cs` will be kept and the script of the destination scene will deactivate itself and ` Sample_Scenes_Preload.cs` will be kept, and the script of the transitioned scene will deactivate itself and `Sample_Scenes_Preload.cs` will be executed continuously.

