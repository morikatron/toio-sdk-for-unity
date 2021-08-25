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
            gesundroid = 4,
            custom = 99  // 座標範囲をカスタマイズ
        }

        public static readonly string[] MatTypeNames = new string[]
        {
            "トイコレ付属マット（土俵面）",     //0
            "トイコレ付属マット（色タイル面）",     //1
            "簡易マット・開発用マット（表面）1~6",        //2
            "開発用マット（裏面）",        //3
            "ゲズンロイド",                 //4
            "カスタマイズ",           //99
        };

        public enum DeveloperMatType
        {
            _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12,
        }
        public static readonly string[] DeveloperMatTypeNames = new string[]
        { "#1", "#2", "#3", "#4", "#5", "#6", "#7", "#8", "#9", "#10", "#11", "#12" };

        public const float DotPerM = 411f/0.560f; // 411/0.560 dot/m

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
            // Resize
            if (matType != MatType.custom)
            {
                var rect = GetRectForMatType(matType, developerMatType);
                xMin = rect.xMin; xMax = rect.xMax;
                yMin = rect.yMin; yMax = rect.yMax;
            }
            this.transform.localScale = new Vector3((xMax-xMin+1)/DotPerM, (yMax-yMin+1)/DotPerM, 1);

            // Change material
            switch (matType){
                case MatType.toio_collection_front:
                    GetComponent<Renderer>().material = (Material)Resources.Load<Material>("Mat/toio_collection_front");;
                    break;
                case MatType.toio_collection_back:
                    GetComponent<Renderer>().material = (Material)Resources.Load<Material>("Mat/toio_collection_back");
                    break;
                case MatType.simple_playmat:
                    GetComponent<Renderer>().material = (Material)Resources.Load<Material>("Mat/simple_playmat");
                    break;
                case MatType.developer:
                    GetComponent<Renderer>().material = (Material)Resources.Load<Material>("Mat/simple_playmat");
                    break;
                case MatType.gesundroid:
                    GetComponent<Renderer>().material = (Material)Resources.Load<Material>("Mat/gesundroid");
                    break;
                case MatType.custom:
                    GetComponent<Renderer>().material = (Material)Resources.Load<Material>("Mat/mat_null");
                    break;
            }
        }

        public static RectInt GetRectForMatType(MatType matType, DeveloperMatType devMatType=default)
        {
            switch (matType){
                case MatType.toio_collection_front:
                    return new RectInt(45, 45, 410, 410);
                case MatType.toio_collection_back:
                    return new RectInt(545, 45, 410, 410);
                case MatType.simple_playmat:
                    return new RectInt(98, 142, 304, 216);
                case MatType.developer:
                    switch (devMatType){
                        case DeveloperMatType._1:  return new RectInt( 34,  35, 305, 215);
                        case DeveloperMatType._2:  return new RectInt( 34, 251, 305, 215);
                        case DeveloperMatType._3:  return new RectInt( 34, 467, 305, 215);
                        case DeveloperMatType._4:  return new RectInt( 34, 683, 305, 215);
                        case DeveloperMatType._5:  return new RectInt(340,  35, 304, 215);
                        case DeveloperMatType._6:  return new RectInt(340, 251, 304, 215);
                        case DeveloperMatType._7:  return new RectInt(340, 467, 304, 215);
                        case DeveloperMatType._8:  return new RectInt(340, 683, 304, 215);
                        case DeveloperMatType._9:  return new RectInt(645,  35, 304, 215);
                        case DeveloperMatType._10: return new RectInt(645, 251, 304, 215);
                        case DeveloperMatType._11: return new RectInt(645, 467, 304, 215);
                        case DeveloperMatType._12: return new RectInt(645, 683, 304, 215);
                    }
                    throw new System.Exception("devMatType out of range.");
                case MatType.gesundroid:
                    return new RectInt(1050, 45, 410, 410);
                case MatType.custom:
                    Debug.LogError("Custom MatType not supported in this method.");
                    return new RectInt(0, 0, 0, 0);
            }
            throw new System.Exception("matType out of range.");
        }


        // ==== 角度変換関数 ====

        /// <summary>
        /// Unity上の角度を本マット上の角度に変換
        /// </summary>
        public int UnityDeg2MatDeg(float deg)
        {
            return Mathf.RoundToInt((deg-this.transform.eulerAngles.y-90)%360+360)%360;
        }
        internal float UnityDeg2MatDegF(float deg)
        {
            return ((deg-this.transform.eulerAngles.y-90)%360+360)%360;
        }
        /// <summary>
        /// Unity上の角度をマットmat上の角度に変換
        /// </summary>
        public static int UnityDeg2MatDeg(float deg, Mat mat)
        {
            if (mat == null) return (int)((deg-90)%360+360)%360;
            else return mat.UnityDeg2MatDeg(deg);
        }

        /// <summary>
        /// 本マット上の角度をUnity上の角度に変換
        /// </summary>
        public float MatDeg2UnityDeg(float deg)
        {
            return Mathf.RoundToInt(deg+this.transform.eulerAngles.y+90)%360;
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
            var coord = UnityCoord2MatCoordF(unityCoord);

            // マット単位に変換
            return new Vector2Int(
                Mathf.RoundToInt(coord.x),
                Mathf.RoundToInt(coord.y)
            );
        }

        internal Vector2 UnityCoord2MatCoordF(Vector3 unityCoord)
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
            return new Vector2(
                coord.x*DotPerM + this.xCenter,
                coord.y*DotPerM + this.yCenter
            );
        }

        /// <summary>
        /// マット mat におけるマット座標から、Unity の3D空間に変換。mat が null の場合、mat.prefab の初期位置に基づく。
        /// </summary>
        public static Vector3 MatCoord2UnityCoord(float x, float y, Mat mat=null)
        {
            if (mat==null)
                return new Vector3((float)(x-250)/DotPerM, 0, -(float)(y-250)/DotPerM);
            else
            {
                return mat.MatCoord2UnityCoord(x, y);
            }
        }
        /// <summary>
        /// マット mat におけるマット座標から、Unity の3D空間に変換。mat が null の場合、mat.prefab の初期位置に基づく。
        /// </summary>
        public static Vector3 MatCoord2UnityCoord(Vector2Int matCoord, Mat mat=null)
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
