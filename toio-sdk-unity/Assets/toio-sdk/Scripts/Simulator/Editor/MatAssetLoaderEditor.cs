using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace toio.Simulator
{

    [CustomEditor(typeof(MatAssetLoader))]
    public class MatAssetLoaderEditor : Editor
    {
        void OnEnable()
        {
            Undo.undoRedoPerformed += HandleUndo;
        }

        void HandleUndo()
        {
            var loader = target as MatAssetLoader;
            var mat = loader.GetComponent<Mat>();
            mat.ApplyMatType();
        }


        public override void OnInspectorGUI()
        {
            var loader = target as MatAssetLoader;
            var mat = loader.GetComponent<Mat>();

            if (DrawDefaultInspector()) {
                mat.ApplyMatType();
            }
        }

    }

}
#endif
