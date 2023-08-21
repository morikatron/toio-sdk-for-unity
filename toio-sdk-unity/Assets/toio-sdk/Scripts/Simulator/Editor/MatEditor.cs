using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace toio.Simulator
{

    [CustomEditor(typeof(Mat))]
    public class MatEditor : Editor
    {
        void OnEnable()
        {
            Undo.undoRedoPerformed += HandleUndo;
        }

        void HandleUndo()
        {
            var mat = target as Mat;
            mat.ApplyMatType();
        }

        public override void OnInspectorGUI()
        {
            var mat = target as Mat;

            serializedObject.Update();
            var matType = serializedObject.FindProperty("matType");
            var developerMatType = serializedObject.FindProperty("developerMatType");
            var xMin = serializedObject.FindProperty("xMinCustom");
            var xMax = serializedObject.FindProperty("xMaxCustom");
            var yMin = serializedObject.FindProperty("yMinCustom");
            var yMax = serializedObject.FindProperty("yMaxCustom");

            EditorGUI.BeginChangeCheck();
            // Mat Type list
            matType.intValue = (int)EditorGUILayout.Popup("Type",
                matType.intValue,
                Mat.MatTypeNames
            );

            // Developer Mat No. list
            if (matType.intValue == (int)Mat.MatType.developer)
            {
                developerMatType.intValue = (int)EditorGUILayout.Popup(
                    "Developer Mat No.",
                    developerMatType.intValue,
                    Mat.DeveloperMatTypeNames
                );
            }

            // Custom Mat Range
            if (mat.matType == Mat.MatType.custom)
            {
                xMin.intValue = (int)EditorGUILayout.IntField("x Min", xMin.intValue);
                xMax.intValue = (int)EditorGUILayout.IntField("x Max", xMax.intValue);
                yMin.intValue = (int)EditorGUILayout.IntField("y Min", yMin.intValue);
                yMax.intValue = (int)EditorGUILayout.IntField("y Max", yMax.intValue);
            }

            // Apply Changes
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) mat.ApplyMatType();
        }

    }

}
#endif
