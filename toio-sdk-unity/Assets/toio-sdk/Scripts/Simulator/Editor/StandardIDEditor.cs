using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace toio.Simulator
{

    [CustomEditor(typeof(StandardID))]
    public class StandardIDEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            var stdID = target as StandardID;
            bool toApply = false;
            serializedObject.Update();
            var title = serializedObject.FindProperty("title");
            var toioColleType = serializedObject.FindProperty("toioColleType");
            var simpleCardType = serializedObject.FindProperty("simpleCardType");

            // StandardID Title list
            int title_new = (int)EditorGUILayout.Popup("Title",
                (int)title.intValue,
                StandardID.TitleNames
            );
            toApply |= title.intValue != title_new;
            title.intValue = title_new;

            if (title_new == (int)StandardID.Title.toio_collection)
            {
                // toio Collection Type list
                int toioColleType_new = (int)EditorGUILayout.Popup("Type",
                    (int)toioColleType.intValue,
                    StandardID.ToioColleNames
                );
                toApply |= toioColleType.intValue != toioColleType_new;
                toioColleType.intValue = toioColleType_new;
            }
            else if (title_new == (int)StandardID.Title.simple_card)
            {
                // simple card Type list
                int simpleCardType_new = (int)EditorGUILayout.Popup("Type",
                    (int)simpleCardType.intValue,
                    StandardID.SimpleCardNames
                );
                toApply |= simpleCardType.intValue != simpleCardType_new;
                simpleCardType.intValue = simpleCardType_new;
            }

            // Apply Changes
            serializedObject.ApplyModifiedProperties();
            if (toApply) stdID.ApplyStandardIDType();
        }

    }

}
#endif
