using UnityEngine;


namespace toio.Simulator
{
    public class MatAssetLoader : MonoBehaviour
    {
        // public Material blank;
        // public Material toio_collection_front;
        // public Material toio_collection_back;
        // public Material gesundroid;
        // public Material simple_playmat;
        public Sprite toio_collection_front;
        public Sprite toio_collection_back;
        public Sprite gesundroid;
        public Sprite simple_playmat;
        public Sprite custom;

        public Sprite GetSprite(Mat.MatType mat){
            switch (mat){
                case Mat.MatType.toio_collection_front: return toio_collection_front;
                case Mat.MatType.toio_collection_back: return toio_collection_back;
                case Mat.MatType.simple_playmat: return simple_playmat;
                case Mat.MatType.developer: return simple_playmat;
                case Mat.MatType.gesundroid: return gesundroid;
                case Mat.MatType.custom:
                    if (custom == null) {
                        var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                        texture.SetPixel(0, 0, Color.white);
                        texture.Apply();
                        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                    }
                    return custom;
            }
            return null;
        }
    }
}
