using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace toio.Simulator
{

    [CustomEditor(typeof(CubeSimulator))]
    public class CubeSimulatorEditor : Editor
    {

        string[] poseNames = new string[6]{"up", "down", "front", "back", "right", "left"};
        string[] versionNames = new string[3]{"2.0.0", "2.1.0", "2.2.0"};

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
                EditorGUILayout.LabelField("【シミュレータの設定】");
                EditorGUILayout.BeginVertical(GUI.skin.box);

                base.OnInspectorGUI();
                // version.intValue = (int)EditorGUILayout.Popup("Version",
                //     (int)version.intValue, versionNames
                // );
                EditorGUILayout.EndVertical();
            }
            else
            {
                // ==== Information】 ====
                EditorGUILayout.LabelField("【Cube Information】");
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Version", versionNames[version.intValue]);
                cube.power = EditorGUILayout.Toggle("Power", cube.power);
                EditorGUILayout.LabelField("Running", cube.isRunning? "YES":" NO");
                EditorGUILayout.LabelField("Connected", cube.isConnected? "YES":" NO");
                forceStop.boolValue = EditorGUILayout.Toggle("Force Stop", forceStop.boolValue);
                EditorGUILayout.EndVertical();



                // ==== 【手動でキューブの状態を変更】 ====
                // 実行時のみに表示
                if (cube.isConnected)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("【手動でキューブの状態を変更】");
                    EditorGUILayout.BeginVertical(GUI.skin.box);

                    var button_new = GUILayout.Toggle(cube.button, "button 状態");
                    if (cube.button!=button_new){
                        cube.button = button_new;
                    }
                    EditorGUILayout.Space();

                    cube.isSimulateSloped = !GUILayout.Toggle(!cube.isSimulateSloped, "【sloped の変更を手動で行う】");
                    if (!cube.isSimulateSloped)
                    {
                        var sloped_new = GUILayout.Toggle(cube.sloped, "└ sloped 状態");
                        if (cube.sloped!=sloped_new){
                            cube.sloped = sloped_new;
                        }
                    }
                    EditorGUILayout.Space();

                    if (GUILayout.Button("'衝突'を発生"))
                    {
                        cube._TriggerCollision();
                    }
                    EditorGUILayout.Space();

                    // v2.1.0
                    if (version.intValue > 0)
                    {
                        // double tap
                        if (GUILayout.Button("'ダブルタップ'を発生"))
                        {
                            cube._TriggerDoubleTap();
                        }
                        EditorGUILayout.Space();

                        // pose
                        int pose_new = (int)EditorGUILayout.Popup(
                            "pose 状態",
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
                        var shake_new = EditorGUILayout.IntSlider("shake レベル", cube.shakeLevel, 0, 10);
                        if (cube.shakeLevel!=shake_new){
                            cube.shakeLevel = shake_new;
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
