using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;


namespace toio.Simulator
{

    public class StandardID : MonoBehaviour
    {

        public enum Title
        {
            toio_collection,    //0
            simple_card,        //1
        }
        public static readonly string[] TitleNames = new string[]
        {
            "トイオ・コレクション",
            "簡易カード",
        };

        public enum ToioColleType
        {
            // Card
            id_card_typhoon, id_card_typhoon_back,  //0, 1
            id_card_rush, id_card_rush_back,
            id_card_auto_tackle, id_card_auto_tackle_back,
            id_card_random, id_card_random_back,
            id_card_tackle_power_up, id_card_tackle_power_up_back,  //8, 9
            id_card_swing_attack_power_up, id_card_swing_attack_power_up_back,
            id_card_side_attack, id_card_side_attack_back,
            id_card_automatic_chasing, id_card_automatic_chasing_back,

            // Rhythm   16~20
            id_rhythm_left, id_rhythm_right, id_rhythm_front, id_rhythm_back, id_rhythm_go,

            // Skunk    21~26
            id_skunk_blue, id_skunk_green, id_skunk_yellow,
            id_skunk_orange, id_skunk_red, id_skunk_brown,

            // Sticker  27~32
            id_sticker_speed_up, id_sticker_speed_down, id_sticker_wobble,
            id_sticker_panic, id_sticker_spin, id_sticker_shock,

            // Mark     33~38
            id_mark_craft_fighter, id_mark_rhythm_and_go, id_mark_skunk_chaser,
            id_mark_finger_strike, id_mark_finger_strike_1p, id_mark_free_move,
        }

        // https://toio.github.io/toio-spec/docs/hardware_standard_id
        public static readonly string[] ToioColleNames = new string[]
        {
            // Card
            "タイフーン（表）", "タイフーン（裏）",       //0, 1
            "ラッシュ（表）", "ラッシュ（裏）",
            "オートタックル（表）", "オートタックル（裏）",
            "ランダム（表）", "ランダム（裏）",
            "ツキパワーアップ（表）", "ツキパワーアップ（裏）", //8, 9
            "ハリテパワーアップ（表）", "ハリテパワーアップ（裏）",
            "サイドアタック（表）", "サイドアタック（裏）",
            "イージーモード（表）", "イージーモード（裏）",

            // Rhythm   16~20
            "ひだり", "みぎ", "まえ", "うしろ", "GO",

            // Skunk    21~26
            "スカンク（青色）", "スカンク（緑色）", "スカンク（黄色）",
            "スカンク（オレンジ色）", "スカンク（赤色）", "スカンク（茶色）",

            // Sticker  27~32
            "スピードアップ", "スピードダウン", "ふらつき",
            "パニック", "スピン", "ショック",

            // Mark     33~38
            "クラフトファイター", "リズム＆ゴー", "スカンクチェイサー",
            "フィンガーストライク", "フィンガーストライク 1 人プレイ", "フリームーブ",
        };
        public static readonly uint ToioColleIDOffset = 3670000;
        public static readonly uint[] ToioColleIDs = new uint[]
        {
            16, 16, 54, 54, 18, 18, 56, 56, 20, 20, 58, 58, 22, 22, 60, 60,     // Card
            24, 62, 26, 64, 28,       // Rhythm   16~20
            78, 42, 80, 44, 82, 46,   // Skunk    21~26
            66, 30, 68, 32, 70, 34,   // Sticker  27~32
            48, 52, 86, 50, 88, 84,   // Mark     33~38
        };

        // Simple Card
        public enum SimpleCardType
        {
            Num_0, Num_1, Num_2, Num_3, Num_4, Num_5, Num_6, Num_7, Num_8, Num_9,
            Char_A, Char_B, Char_C, Char_D, Char_E, Char_F, Char_G, Char_H, Char_I, Char_J, Char_K, Char_L, Char_M, Char_N, Char_O, Char_P, Char_Q, Char_R, Char_S, Char_T, Char_U, Char_V, Char_W, Char_X, Char_Y, Char_Z,
            Symbol_Exclamation, Arrow_Up, Symbol_Question, Operator_Plus, Operator_Minus, Operator_Equal,
            Arrow_Left, Arrow_Down, Arrow_Right, Operator_Times, Operator_Divide, Symbol_Percent,
            Full
        }

        public static readonly string[] SimpleCardNames = new string[]
        {
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "!", "↑", "?", "+", "-", "=",
            "←", "↓", "→", "×", "÷", "%", "全部",
        };
        public static readonly uint SimpleCardIDOffset = 3670300;
        public static readonly uint[] SimpleCardIDs = new uint[]
        {
            20, 21, 22, 23, 24, 25, 26, 27, 28, 29, //0~9
            37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, // A~Z
            5, 66, 35, 15, 17, 33, 32, 67, 34, 14, 19, 9    // Symbol
        };


        [SerializeField]
        public Title title;
        [SerializeField]
        public ToioColleType toioColleType;
        [SerializeField]
        public SimpleCardType simpleCardType;

        public uint id{ get
        {
            if (title == Title.toio_collection)
            {
                return StandardID.ToioColleIDs[(int)toioColleType] + StandardID.ToioColleIDOffset;
            }
            else if (title == Title.simple_card)
            {
                return StandardID.SimpleCardIDs[(int)simpleCardType] + StandardID.SimpleCardIDOffset;
            }
            else return 0;
        }}


        void Start()
        {
            this.ApplyStandardIDType();
        }

        /// <summary>
        /// StandardID の変更を反映
        /// </summary>
        internal void ApplyStandardIDType()
        {
            // Load Sprite
            string spritePath = "StandardID/"+title.ToString()+"/";
            if (title == Title.toio_collection) spritePath += toioColleType.ToString();
            else if (title == Title.simple_card) spritePath += simpleCardType.ToString();
            var sprite = (Sprite)Resources.Load<Sprite>(spritePath);
            GetComponent<SpriteRenderer>().sprite = sprite;

            // Create Mesh
            var mesh = SpriteToMesh(sprite);
            GetComponentInChildren<MeshFilter>().mesh = mesh;

            // Update Mesh Collider
            GetComponentInChildren<MeshCollider>().sharedMesh = null;
            GetComponentInChildren<MeshCollider>().sharedMesh = mesh;

            // Update Size
            float realWidth = 0.05f;
            if (title == Title.toio_collection)
            {
                if ((int)toioColleType > 32) realWidth = 0.03f;
                else if ((int)toioColleType < 21 || (int)toioColleType > 26) realWidth = 0.0575f;
                else    // Skunk
                {
                    if (toioColleType == ToioColleType.id_skunk_blue) realWidth = 0.179f;
                    else if (toioColleType == ToioColleType.id_skunk_green) realWidth = 0.162f;
                    else if (toioColleType == ToioColleType.id_skunk_yellow) realWidth = 0.145f;
                    else if (toioColleType == ToioColleType.id_skunk_orange) realWidth = 0.1335f;
                    else if (toioColleType == ToioColleType.id_skunk_red) realWidth = 0.1285f;
                    else realWidth = 0.1225f; //toioColleType = ToioColleType.id_skunk_brown
                }
            }
            else if (title == Title.simple_card)
            {
                if (simpleCardType == SimpleCardType.Full) realWidth = 0.297f;
                else realWidth = 0.04f;
            }
            var scale = RealWidthToScale(sprite, realWidth);
            this.transform.localScale = new Vector3(scale, scale, 1);

        }

        /// <summary>
        /// 実際の寸法に対応するオブジェクトのスケールを計算する
        /// </summary>
        public static float RealWidthToScale(Sprite sprite, float realWidth)
        {
            return sprite.pixelsPerUnit/(sprite.rect.width/realWidth);
        }

        private Mesh SpriteToMesh(Sprite sprite)
        {
            var mesh = new Mesh();
            mesh.SetVertices(Array.ConvertAll(sprite.vertices, v => (Vector3)v).ToList());
            mesh.SetUVs(0, sprite.uv.ToList());
            mesh.SetTriangles(Array.ConvertAll(sprite.triangles, t => (int)t), 0);
            return mesh;
        }


        // ==== 角度変換関数 ====

        /// <summary>
        /// Unity上の角度を本Standard ID上の角度に変換
        /// </summary>
        public int UnityDeg2MatDeg(float deg)
        {
            return Mathf.RoundToInt(deg-this.transform.eulerAngles.y-90)%360;
        }
        internal float UnityDeg2MatDegF(float deg)
        {
            return (deg-this.transform.eulerAngles.y-90)%360;
        }
        /// <summary>
        /// Unity上の角度をStandard ID上の角度に変換
        /// </summary>
        public static int UnityDeg2MatDeg(float deg, StandardID standardID)
        {
            if (standardID == null) return (int)(deg-90)%360;
            else return standardID.UnityDeg2MatDeg(deg);
        }

        /// <summary>
        /// 本Standard ID上の角度をUnity上の角度に変換
        /// </summary>
        public float MatDeg2UnityDeg(float deg)
        {
            return Mathf.RoundToInt(deg+this.transform.eulerAngles.y+90)%360;
        }
        /// <summary>
        /// Standard ID上の角度をUnity上の角度に変換
        /// </summary>
        public static float MatDeg2UnityDeg(float deg, StandardID standardID)
        {
            if (standardID == null) return (float)(deg+90)%360;
            else return standardID.MatDeg2UnityDeg(deg);
        }

    }

}
