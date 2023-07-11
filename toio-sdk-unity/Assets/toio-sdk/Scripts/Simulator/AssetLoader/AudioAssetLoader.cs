using UnityEngine;

namespace toio.Simulator
{
    public class AudioAssetLoader : MonoBehaviour
    {
        public AudioClip note9;
        public AudioClip note12;
        public AudioClip note33;
        public AudioClip note45;
        public AudioClip note57;
        public AudioClip note69;
        public AudioClip note81;
        public AudioClip note93;
        public AudioClip note105;
        public AudioClip note117;
        public AudioClip note129;

        public AudioClip GetAudioCLip(int octave){
            switch (octave) {
                case 0: return note9;
                case 1: return note12;
                case 2: return note33;
                case 3: return note45;
                case 4: return note57;
                case 5: return note69;
                case 6: return note81;
                case 7: return note93;
                case 8: return note105;
                case 9: return note117;
                case 10: return note129;
                default: return null;
            }
        }
    }
}
