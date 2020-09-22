using UnityEngine;

namespace toio.Simulator
{

    /// <summary>
    /// マットオブジェクトに一対一に付けてください。
    /// </summary>
    public class Mat : MonoBehaviour
    {
        public enum MatType
        {
            toio_collection_front = 0,
            toio_collection_back = 1,
            simple_playmat = 2,
            developer = 3,
            Custom = 4  // 座標範囲をカスタマイズ
        }

        public static readonly string[] MatTypeNames = new string[]
        {
            "トイコレ付属マット（土俵面）",     //0
            "トイコレ付属マット（色タイル面）",     //1
            "キューブ（単体）付属簡易マット",        //2
            "開発用マット",        //3
            "カスタマイズ",           //4
        };

        public enum DeveloperMatType
        {
            _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12,
        }
        public static readonly string[] DeveloperMatTypeNames = new string[]
        { "#1", "#2", "#3", "#4", "#5", "#6", "#7", "#8", "#9", "#10", "#11", "#12" };

        public static readonly float DotPerM = 411f/0.560f; // 411/0.560 dot/m

        [SerializeField]
        public MatType matType;
        [SerializeField]
        public DeveloperMatType developerMatType;
        [SerializeField]
        public int xMin, xMax, yMin, yMax;
        public float xCenter { get{ return (xMin+xMax)/2f; } }
        public float yCenter { get{ return (yMin+yMax)/2f; } }

        void Start()
        {
            this.ApplyMatType();
        }

        void Update()
        {

        }

        /// <summary>
        /// マットのタイプ、座標範囲の変更を反映
        /// </summary>
        internal void ApplyMatType()
        {
            switch (matType){
                case MatType.toio_collection_front:
                    xMin = 45; xMax = 455; yMin = 45; yMax = 455;
                    GetComponent<Renderer>().material = (Material)Resources.Load<Material>("Mat/toio_collection_front");;
                    break;
                case MatType.toio_collection_back:
                    xMin = 545; xMax = 955; yMin = 45; yMax = 455;
                    GetComponent<Renderer>().material = (Material)Resources.Load<Material>("Mat/toio_collection_back");
                    break;
                case MatType.simple_playmat:
                    xMin = 98; xMax = 402; yMin = 142; yMax = 358;
                    GetComponent<Renderer>().material = (Material)Resources.Load<Material>("Mat/simple_playmat");
                    break;
                case MatType.developer:
                    switch (developerMatType){
                        case DeveloperMatType._1: xMin = 34; xMax = 339; yMin = 35; yMax = 250; break;
                        case DeveloperMatType._2: xMin = 34; xMax = 339; yMin = 251; yMax = 466; break;
                        case DeveloperMatType._3: xMin = 34; xMax = 339; yMin = 467; yMax = 682; break;
                        case DeveloperMatType._4: xMin = 34; xMax = 339; yMin = 683; yMax = 898; break;
                        case DeveloperMatType._5: xMin = 340; xMax = 644; yMin = 35; yMax = 250; break;
                        case DeveloperMatType._6: xMin = 340; xMax = 644; yMin = 251; yMax = 466; break;
                        case DeveloperMatType._7: xMin = 340; xMax = 644; yMin = 467; yMax = 682; break;
                        case DeveloperMatType._8: xMin = 340; xMax = 644; yMin = 683; yMax = 898; break;
                        case DeveloperMatType._9: xMin = 645; xMax = 949; yMin = 35; yMax = 250; break;
                        case DeveloperMatType._10: xMin = 645; xMax = 949; yMin = 251; yMax = 466; break;
                        case DeveloperMatType._11: xMin = 645; xMax = 949; yMin = 467; yMax = 682; break;
                        case DeveloperMatType._12: xMin = 645; xMax = 949; yMin = 683; yMax = 898; break;
                    }
                    GetComponent<Renderer>().material = (Material)Resources.Load<Material>("Mat/simple_playmat");
                    break;
                case MatType.Custom:
                    GetComponent<Renderer>().material = (Material)Resources.Load<Material>("Mat/mat_null");
                    break;
            }
            this.transform.localScale = new Vector3((xMax-xMin+1)/DotPerM, (yMax-yMin+1)/DotPerM, 1);
        }


        // ==== 角度変換関数 ====

        /// <summary>
        /// Unity上の角度を本マット上の角度に変換
        /// </summary>
        public int UnityDeg2MatDeg(float deg)
        {
            return (int)(deg-this.transform.eulerAngles.y-90+0.49999f)%360;
        }
        /// <summary>
        /// Unity上の角度をマットmat上の角度に変換
        /// </summary>
        public static int UnityDeg2MatDeg(float deg, Mat mat)
        {
            if (mat == null) return (int)(deg-90)%360;
            else return mat.UnityDeg2MatDeg(deg);
        }

        /// <summary>
        /// 本マット上の角度をUnity上の角度に変換
        /// </summary>
        public float MatDeg2UnityDeg(float deg)
        {
            return (int)(deg+this.transform.eulerAngles.y+90+0.49999f)%360;
        }
        /// <summary>
        /// マットmat上の角度をUnity上の角度に変換
        /// </summary>
        public static float MatDeg2UnityDeg(float deg, Mat mat)
        {
            if (mat == null) return (float)(deg+90)%360;
            else return mat.MatDeg2UnityDeg(deg);
        }


        // ==== 座標変換関数 ====

        /// <summary>
        /// Unity の3D空間座標から、本マットにおけるマット座標に変換。
        /// </summary>
        public Vector2Int UnityCoord2MatCoord(Vector3 unityCoord)
        {
            var matPos = this.transform.position;
            var drad = - this.transform.eulerAngles.y * Mathf.Deg2Rad;
            var _cos = Mathf.Cos(drad);
            var _sin = Mathf.Sin(drad);

            // 座標系移動：本マットに一致させ
            var dx = unityCoord[0] - matPos[0];
            var dy = -unityCoord[2] + matPos[2];

            // 座標系回転：本マットに一致させ
            Vector2 coord = new Vector2(dx*_cos-dy*_sin, dx*_sin+dy*_cos);

            // マット単位に変換
            return new Vector2Int(
                (int)(coord.x*DotPerM + this.xCenter + 0.4999f),
                (int)(coord.y*DotPerM + this.yCenter + 0.4999f)
            );
        }

        /// <summary>
        /// マット mat におけるマット座標から、Unity の3D空間に変換。mat が null の場合、mat.prefab の初期位置に基づく。
        /// </summary>
        public static Vector3 MatCoord2UnityCoord(float x, float y, Mat mat)
        {
            if (mat==null)
                return new Vector3((float)(x-250)/DotPerM, 0, -(float)(y-250)/DotPerM);
            else
            {
                return mat.MatCoord2UnityCoord(x, y);
            }
        }
        public static Vector3 MatCoord2UnityCoord(Vector2Int matCoord, Mat mat)
        {
            if (mat==null)
                return new Vector3((matCoord.x-250)/DotPerM, 0, -(matCoord.y-250)/DotPerM);
            else
            {
                return mat.MatCoord2UnityCoord(matCoord.x, matCoord.y);
            }
        }

        /// <summary>
        /// 本マットにおけるマット座標から、Unity の3D空間に変換。
        /// </summary>
        public Vector3 MatCoord2UnityCoord(float x, float y)
        {
            var matPos = this.transform.position;
            var drad = this.transform.eulerAngles.y * Mathf.Deg2Rad;
            var _cos = Mathf.Cos(drad);
            var _sin = Mathf.Sin(drad);

            // メーター単位に変換
            var dx = ((float)x - xCenter)/DotPerM;
            var dy = ((float)y - yCenter)/DotPerM;

            // 座標系回転：Unityに一致させ
            Vector2 coord = new Vector2(dx*_cos-dy*_sin, dx*_sin+dy*_cos);

            // 座標系移動：Unityに一致させ
            coord.x += matPos.x;
            coord.y += -matPos.z;

            return new Vector3(coord.x, matPos.y, -coord.y);
        }

    }

}
