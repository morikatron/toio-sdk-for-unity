using UnityEngine;


namespace toio.Simulator
{
    public class MatAssetLoader : MonoBehaviour
    {
        public Material blank;
        public Material toio_collection_front;
        public Material toio_collection_back;
        public Material gesundroid;
        public Material simple_playmat;

        public Material GetMaterial(Mat.MatType mat){
            switch (mat){
                case Mat.MatType.toio_collection_front: return toio_collection_front;
                case Mat.MatType.toio_collection_back: return toio_collection_back;
                case Mat.MatType.simple_playmat: return simple_playmat;
                case Mat.MatType.developer: return simple_playmat;
                case Mat.MatType.gesundroid: return gesundroid;
                case Mat.MatType.custom: return blank;
            }
            return null;
        }
    }
}
