using UnityEditor;

#if UNITY_EDITOR
namespace toio.Simulator
{

    [CustomEditor(typeof(Mat))]
    public class MyScriptEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            var mat = target as Mat;
            bool toApply = false;

            serializedObject.Update();
            var matType = serializedObject.FindProperty("matType");
            var developerMatType = serializedObject.FindProperty("developerMatType");
            var xMin = serializedObject.FindProperty("xMin");
            var xMax = serializedObject.FindProperty("xMax");
            var yMin = serializedObject.FindProperty("yMin");
            var yMax = serializedObject.FindProperty("yMax");

            // Mat Type list
            var matType_new = (int)EditorGUILayout.Popup("Type",
                matType.intValue,
                Mat.MatTypeNames
            );
            toApply |= matType.intValue != matType_new;
            matType.intValue = matType_new;

            // Developer Mat No. list
            if (matType_new == (int)Mat.MatType.developer)
            {
                var developerMatType_new = (int)EditorGUILayout.Popup(
                    "Developer Mat No.",
                    developerMatType.intValue,
                    Mat.DeveloperMatTypeNames
                );
                toApply |= developerMatType.intValue != developerMatType_new;
                developerMatType.intValue = developerMatType_new;
            }


            // Custom Mat Range
            if (mat.matType == Mat.MatType.custom)
            {
                var xMin_new = (int)EditorGUILayout.IntField("x Min", xMin.intValue);
                var xMax_new = (int)EditorGUILayout.IntField("x Max", xMax.intValue);
                var yMin_new = (int)EditorGUILayout.IntField("y Min", yMin.intValue);
                var yMax_new = (int)EditorGUILayout.IntField("y Max", yMax.intValue);
                toApply |= xMin.intValue != xMin_new;
                toApply |= xMax.intValue != xMax_new;
                toApply |= yMin.intValue != yMin_new;
                toApply |= yMax.intValue != yMax_new;
                xMin.intValue = xMin_new;
                xMax.intValue = xMax_new;
                yMin.intValue = yMin_new;
                yMax.intValue = yMax_new;
            }

            // Apply Changes
            serializedObject.ApplyModifiedProperties();
            if (toApply) mat.ApplyMatType();
        }

    }

}
#endif
