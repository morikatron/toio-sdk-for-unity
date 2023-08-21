using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace toio.Simulator
{

    [CustomEditor(typeof(StandardID))]
    public class StandardIDEditor : Editor
    {
        void OnEnable()
        {
            Undo.undoRedoPerformed += HandleUndo;
        }

        void HandleUndo()
        {
            var stdID = target as StandardID;
            stdID.ApplyStandardIDType();
        }


        public override void OnInspectorGUI()
        {
            var stdID = target as StandardID;
            serializedObject.Update();
            var title = serializedObject.FindProperty("title");
            var toioColleType = serializedObject.FindProperty("toioColleType");
            var simpleCardType = serializedObject.FindProperty("simpleCardType");

            EditorGUI.BeginChangeCheck();

            // StandardID Title list
            title.intValue = EditorGUILayout.Popup(
                "Title",
                title.intValue,
                StandardID.TitleNames
            );

            if (title.intValue == (int)StandardID.Title.toio_collection)
            {
                // toio Collection Type list
                toioColleType.intValue = EditorGUILayout.Popup(
                    "Type",
                    toioColleType.intValue,
                    StandardID.ToioColleNames
                );
            }
            else if (title.intValue == (int)StandardID.Title.simple_card)
            {
                // simple card Type list
                simpleCardType.intValue = EditorGUILayout.Popup(
                    "Type",
                    simpleCardType.intValue,
                    StandardID.SimpleCardNames
                );
            }

            // Apply Changes
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) stdID.ApplyStandardIDType();
        }

    }

}
#endif
