using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace toio.Simulator
{

    [CustomEditor(typeof(CubeSimulator))]
    public class CubeSimulatorEditor : Editor
    {

        string[] poseNames = new string[6]{"up", "down", "front", "back", "right", "left"};
        string[] magnetStateNames = new string[7]{"None", "S Center", "N Center", "S Right", "N Right", "S Left", "N Left"};
        string[] versionNames = new string[4]{"2.0.0", "2.1.0", "2.2.0", "2.3.0"};

        public override void OnInspectorGUI()
        {
            var cube = target as CubeSimulator;
            serializedObject.Update();

            var version = serializedObject.FindProperty("version");
            var motorTau = serializedObject.FindProperty("motorTau");
            var delay = serializedObject.FindProperty("delay");
            var forceStop = serializedObject.FindProperty("forceStop");

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("【Settings】");
                EditorGUILayout.BeginVertical(GUI.skin.box);

                base.OnInspectorGUI();
                // version.intValue = (int)EditorGUILayout.Popup("Version",
                //     (int)version.intValue, versionNames
                // );
                EditorGUILayout.EndVertical();
            }
            // Runtime
            else
            {
                // ==== 【Cube Information】 ====
                EditorGUILayout.LabelField("【Cube Information】");
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Version", versionNames[version.intValue]);
                cube.power = EditorGUILayout.Toggle("Power", cube.power);
                EditorGUILayout.LabelField("Running", cube.isRunning? "YES":" NO");
                EditorGUILayout.LabelField("Connected", cube.isConnected? "YES":" NO");
                forceStop.boolValue = EditorGUILayout.Toggle("Force Stop", forceStop.boolValue);
                EditorGUILayout.EndVertical();


                // ==== 【Change states manually】 ====
                if (cube.isConnected)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("【Change states manually】");
                    EditorGUILayout.BeginVertical(GUI.skin.box);

                    var button_new = GUILayout.Toggle(cube.button, "button");
                    if (cube.button!=button_new){
                        cube.button = button_new;
                    }
                    EditorGUILayout.Space();

                    cube.isSimulateSloped = !GUILayout.Toggle(!cube.isSimulateSloped, "【Change sloped manually】");
                    if (!cube.isSimulateSloped)
                    {
                        var sloped_new = GUILayout.Toggle(cube.sloped, " └ sloped");
                        if (cube.sloped!=sloped_new){
                            cube.sloped = sloped_new;
                        }
                    }
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Trigger Collision"))
                    {
                        cube._TriggerCollision();
                    }
                    EditorGUILayout.Space();

                    // v2.1.0
                    if (version.intValue > 0)
                    {
                        // double tap
                        if (GUILayout.Button("Trigger DoubleTap"))
                        {
                            cube._TriggerDoubleTap();
                        }
                        EditorGUILayout.Space();

                        // pose
                        int pose_new = (int)EditorGUILayout.Popup(
                            "pose",
                            (int)cube.pose-1,
                            poseNames
                        )+1;
                        if (pose_new!=(int)cube.pose)
                        {
                            cube.transform.position += new Vector3(0,0.03f,0); // 上に持ち上がって
                            switch (pose_new)
                            {
                                case 1 : cube.transform.up  = Vector3.up; break;
                                case 2 : cube.transform.up  = -Vector3.up; break;
                                case 3 : cube.transform.forward  = Vector3.up; break;
                                case 4 : cube.transform.forward  = -Vector3.up; break;
                                case 5 : cube.transform.right  = Vector3.up; break;
                                case 6 : cube.transform.right  = -Vector3.up; break;
                            }
                        }
                        EditorGUILayout.Space();
                    }

                    // v2.2.0
                    if (version.intValue > 1)
                    {
                        // shake
                        var shake_new = EditorGUILayout.IntSlider("shake level", cube.shakeLevel, 0, 10);
                        if (cube.shakeLevel!=shake_new){
                            cube.shakeLevel = shake_new;
                        }
                        EditorGUILayout.Space();

                        // magnet
                        cube.isSimulateMagneticSensor = !GUILayout.Toggle(!cube.isSimulateMagneticSensor, "【Change Magnetic Sensor manually】");
                        if (!cube.isSimulateMagneticSensor)
                        {
                            var magnetState_new = (int)EditorGUILayout.Popup(" └ magnet state", (int)cube.magnetState, magnetStateNames);
                            if ((int)cube.magnetState != magnetState_new){
                                cube._SetMagneticField((Cube.MagnetState)magnetState_new);
                            }
                        }
                    }

                    // v2.3.0
                    if (version.intValue > 2)
                    {
                        // magnetic force
                        if (!cube.isSimulateMagneticSensor)
                        {
                            var scaledMagneticField = cube._GetScaledMagneticField();
                            var field_new = EditorGUILayout.Vector3Field(" └ magnetic force", scaledMagneticField);
                            if (field_new.magnitude > 255) field_new *= 255f/field_new.magnitude;
                            if (scaledMagneticField != field_new){
                                cube._SetMagneticField(field_new);
                            }
                        }
                        EditorGUILayout.Space();
                    }

                    EditorGUILayout.EndVertical();
                }

                serializedObject.ApplyModifiedProperties();
            }

        }

    }

}
#endif