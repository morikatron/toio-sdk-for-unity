using System;
using System.Collections.Generic;
using UnityEngine;

namespace toio
{
    /// <summary>
    /// 再利用性を保つために、このクラスにはコアキューブ通信仕様以外の機能が存在しません。
    /// </summary>
    public abstract class Cube
    {
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      プロパティ
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        // 接続したコアキューブのファームウェアバージョン
        public abstract string version { get; }
        // コアキューブの固有識別ID
        public abstract string id { get; protected set; }
        // コアキューブのアドレス
        public abstract string addr { get; }
        // コアキューブの接続状態
        public abstract bool isConnected { get; }
        // コアキューブのバッテリー状態
        public abstract int battery { get; protected set; }
        // マット上のコアキューブのX座標
        public abstract int x { get; protected set; }
        // マット上のコアキューブのY座標
        public abstract int y { get; protected set; }
        // マット上のコアキューブのXY座標
        public abstract Vector2 pos { get; }
        // マット上のコアキューブの角度
        public abstract int angle { get; protected set; }
        // マット上のコアキューブのXY座標 (光学センサー位置)
        public abstract Vector2 sensorPos { get; }
        // マット上のコアキューブの角度 (光学センサー角度)
        public abstract int sensorAngle { get; protected set; }
        // 読み取り可能な特殊ステッカーのID
        public abstract uint standardId { get; protected set; }
        // コアキューブのボタン押下状態
        public abstract bool isPressed { get; protected set; }
        // コアキューブの傾き状態
        public abstract bool isSloped { get; protected set; }
        // コアキューブの衝突状態
        public abstract bool isCollisionDetected { get; protected set; }
        // コアキューブのマット接地状態
        public abstract bool isGrounded { get; protected set; }
        // コアキューブの最高速度
        public abstract int maxSpd { get; }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      仮想関数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        /// <summary>
        /// キューブのモーターを制御します
        /// https://toio.github.io/toio-spec/docs/ble_motor#時間指定付きモーター制御
        /// </summary>
        /// <param name="left">左モーター速度</param>
        /// <param name="right">右モーター速度</param>
        /// <param name="durationMs">持続時間(ミリ秒)</param>
        /// <param name="order">命令の優先度</param>
        public virtual void Move(int left, int right, int durationMs, ORDER_TYPE order = ORDER_TYPE.Weak) { UnsupportedWarning(); }

        /// <summary>
        /// キューブ底面についている LED を制御します
        /// https://toio.github.io/toio-spec/docs/ble_light#点灯-消灯
        /// </summary>
        /// <param name="red">赤色の強さ</param>
        /// <param name="green">緑色の強さ</param>
        /// <param name="blue">青色の強さ</param>
        /// <param name="durationMs">持続時間(ミリ秒)</param>
        /// <param name="order">命令の優先度</param>
        public virtual void TurnLedOn(int red, int green, int blue, int durationMs, ORDER_TYPE order = ORDER_TYPE.Strong) { UnsupportedWarning(); }

        /// <summary>
        /// キューブ底面についている LED を連続的に制御します
        /// https://toio.github.io/toio-spec/docs/ble_light#連続的な点灯-消灯
        /// </summary>
        /// <param name="repeatCount">繰り返し回数</param>
        /// <param name="operations">命令配列</param>
        /// <param name="order">命令の優先度</param>
        public virtual void TurnOnLightWithScenario(int repeatCount, Cube.LightOperation[] operations, ORDER_TYPE order = ORDER_TYPE.Strong) { UnsupportedWarning(); }

        /// <summary>
        /// キューブ底面についている LED を消灯させます
        /// https://toio.github.io/toio-spec/docs/ble_light#全てのランプを消灯
        /// </summary>
        /// <param name="order">命令の優先度</param>
        public virtual void TurnLedOff(ORDER_TYPE order = ORDER_TYPE.Strong) { UnsupportedWarning(); }

        /// <summary>
        /// キューブからあらかじめ用意された効果音を再生します
        /// https://toio.github.io/toio-spec/docs/ble_sound#効果音の再生
        /// </summary>
        /// <param name="soundId">サウンドID</param>
        /// <param name="volume">音量</param>
        /// <param name="order">命令の優先度</param>
        public virtual void PlayPresetSound(int soundId, int volume = 255, ORDER_TYPE order = ORDER_TYPE.Strong) { UnsupportedWarning(); }

        /// <summary>
        /// キューブから任意の音を再生します
        /// https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生
        /// </summary>
        /// <param name="repeatCount">繰り返し回数</param>
        /// <param name="operations">命令配列</param>
        /// <param name="order">命令の優先度</param>
        public virtual void PlaySound(int repeatCount, SoundOperation[] operations, ORDER_TYPE order = ORDER_TYPE.Strong) { UnsupportedWarning(); }
        /// <summary>
        /// キューブから任意の音を再生します
        /// https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生
        /// </summary>
        /// <param name="buff">命令プロトコル</param>
        /// <param name="order">命令の優先度</param>
        public virtual void PlaySound(byte[] buff, ORDER_TYPE order = ORDER_TYPE.Strong) { UnsupportedWarning(); }

        /// <summary>
        /// キューブの音再生を停止します
        /// https://toio.github.io/toio-spec/docs/ble_sound#再生の停止
        /// </summary>
        /// <param name="order">命令の優先度</param>
        public virtual void StopSound(ORDER_TYPE order = ORDER_TYPE.Strong) { UnsupportedWarning(); }

        /// <summary>
        /// キューブの水平検出のしきい値を設定します
        /// https://toio.github.io/toio-spec/docs/ble_configuration#水平検出のしきい値設定
        /// </summary>
        /// <param name="angle">傾き検知の閾値</param>
        /// <param name="order">命令の優先度</param>
        public virtual void ConfigSlopeThreshold(int angle, ORDER_TYPE order = ORDER_TYPE.Strong) { UnsupportedWarning(); }

        /// <summary>
        /// キューブの衝突検出のしきい値を設定します
        /// https://toio.github.io/toio-spec/docs/ble_configuration#衝突検出のしきい値設定
        /// </summary>
        /// <param name="level">衝突検知の閾値</param>
        /// <param name="order">命令の優先度</param>
        public virtual void ConfigCollisionThreshold(int level, ORDER_TYPE order = ORDER_TYPE.Strong) { UnsupportedWarning(); }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      コールバック
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        // 非対応コールバック
        private UnsupportingCallbackProvider unsupportingCallback;
        // ボタンコールバック
        public virtual CallbackProvider buttonCallback { get { return this.unsupportingCallback; } }
        // 傾きコールバック
        public virtual CallbackProvider slopeCallback { get { return this.unsupportingCallback; } }
        // 衝突コールバック
        public virtual CallbackProvider collisionCallback { get { return this.unsupportingCallback; } }
        // 座標角度コールバック
        public virtual CallbackProvider idCallback { get { return this.unsupportingCallback; } }
        // StandardID コールバック
        public virtual CallbackProvider standardIdCallback { get { return this.unsupportingCallback; } }
        // ID Missed コールバック
        public virtual CallbackProvider idMissedCallback { get { return this.unsupportingCallback; } }
        // StandardID Missed コールバック
        public virtual CallbackProvider standardIdMissedCallback { get { return this.unsupportingCallback; } }

        public Cube()
        {
            this.unsupportingCallback = new UnsupportingCallbackProvider(this);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      内部クラス
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        // 発音ごとの設定構造体
        public struct SoundOperation
        {
            public Int16 durationMs; // ミリ秒
            public byte volume;      // 音量(0~255)
            public byte note_number; // 音符(0~128)

            public SoundOperation(Int16 durationMs = 0, byte volume = 0, byte note_number = 0)
            {
                this.durationMs = durationMs;
                this.volume = volume;
                this.note_number = note_number;
            }

            public SoundOperation(Int16 durationMs = 0, byte volume = 0, NOTE_NUMBER note_number = 0)
            {
                this.durationMs = durationMs;
                this.volume = volume;
                this.note_number = (byte)note_number;
            }
        }

        // 発光ごとの設定構造体
        public struct LightOperation
        {
            public Int16 durationMs; // ミリ秒
            public byte red;         // 赤色の強さ
            public byte green;       // 緑色の強さ
            public byte blue;        // 青色の強さ

            public LightOperation(Int16 durationMs = 0, byte red = 0, byte green = 0, byte blue = 0)
            {
                this.durationMs = durationMs;
                this.red = red;
                this.green = green;
                this.blue = blue;
            }
        }


        public enum PoseType
        {
            up=1,
            down=2,
            forward=3,
            backward=4,
            right=5,
            left=6
        };

        public enum NOTE_NUMBER : byte
        {
            C0 = 0,
            CS0 = 1,
            D0 = 2,
            DS0 = 3,
            E0 = 4,
            F0 = 5,
            FS0 = 6,
            G0 = 7,
            GS0 = 8,
            A0 = 9,
            AS0 = 10,
            B0 = 11,
            C1 = 12,
            CS1 = 13,
            D1 = 14,
            DS1 = 15,
            E1 = 16,
            F1 = 17,
            FS1 = 18,
            G1 = 19,
            GS1 = 20,
            A1 = 21,
            AS1 = 22,
            B1 = 23,
            C2 = 24,
            CS2 = 25,
            D2 = 26,
            DS2 = 27,
            E2 = 28,
            F2 = 29,
            FS2 = 30,
            G2 = 31,
            GS2 = 32,
            A2 = 33,
            AS2 = 34,
            B2 = 35,
            C3 = 36,
            CS3 = 37,
            D3 = 38,
            DS3 = 39,
            E3 = 40,
            F3 = 41,
            FS3 = 42,
            G3 = 43,
            GS3 = 44,
            A3 = 45,
            AS3 = 46,
            B3 = 47,
            C4 = 48,
            CS4 = 49,
            D4 = 50,
            DS4 = 51,
            E4 = 52,
            F4 = 53,
            FS4 = 54,
            G4 = 55,
            GS4 = 56,
            A4 = 57,
            AS4 = 58,
            B4 = 59,
            C5 = 60,
            CS5 = 61,
            D5 = 62,
            DS5 = 63,
            E5 = 64,
            F5 = 65,
            FS5 = 66,
            G5 = 67,
            GS5 = 68,
            A5 = 69,
            AS5 = 70,
            B5 = 71,
            C6 = 72,
            CS6 = 73,
            D6 = 74,
            DS6 = 75,
            E6 = 76,
            F6 = 77,
            FS6 = 78,
            G6 = 79,
            GS6 = 80,
            A6 = 81,
            AS6 = 82,
            B6 = 83,
            C7 = 84,
            CS7 = 85,
            D7 = 86,
            DS7 = 87,
            E7 = 88,
            F7 = 89,
            FS7 = 90,
            G7 = 91,
            GS7 = 92,
            A7 = 93,
            AS7 = 94,
            B7 = 95,
            C8 = 96,
            CS8 = 97,
            D8 = 98,
            DS8 = 99,
            E8 = 100,
            F8 = 101,
            FS8 = 102,
            G8 = 103,
            GS8 = 104,
            A8 = 105,
            AS8 = 106,
            B8 = 107,
            C9 = 108,
            CS9 = 109,
            D9 = 110,
            DS9 = 111,
            E9 = 112,
            F9 = 113,
            FS9 = 114,
            G9 = 115,
            GS9 = 116,
            A9 = 117,
            AS9 = 118,
            B9 = 119,
            C10 = 120,
            CS10 = 121,
            D10 = 122,
            DS10 = 123,
            E10 = 124,
            F10 = 125,
            FS10 = 126,
            G10 = 127,
            NO_SOUND = 128
        }

        public enum ORDER_TYPE : int
        {
            Strong,
            Weak
        }

        public class CallbackProvider
        {
            protected Dictionary<string, Action<Cube>> listenerTable = new Dictionary<string, Action<Cube>>();
            protected List<Action<Cube>> listenerList = new List<Action<Cube>>();

            public virtual void AddListener(string key, Action<Cube> listener)
            {
                this.listenerTable[key] = listener;
                this.listenerList.Add(listener);
            }
            public virtual void RemoveListener(string key)
            {
                if (this.listenerTable.ContainsKey(key))
                {
                    this.listenerList.Remove(this.listenerTable[key]);
                    this.listenerTable.Remove(key);
                }
            }
            public virtual void ClearListener()
            {
                listenerTable.Clear();
                listenerList.Clear();
            }
            public virtual void Notify(Cube target)
            {
                foreach (var listener in this.listenerList)
                {
                    listener.Invoke(target);
                }
            }
        }

        public class UnsupportingCallbackProvider : CallbackProvider
        {
            private Cube owner;
            public UnsupportingCallbackProvider(Cube owner)
            {
                this.owner = owner;
            }
            public override void AddListener(string key, Action<Cube> listener)
            {
                owner.UnsupportedWarning();
            }
            public override void RemoveListener(string key)
            {
                owner.UnsupportedWarning();
            }
            public override void ClearListener()
            {
                owner.UnsupportedWarning();
            }
            public override void Notify(Cube target)
            {
                owner.UnsupportedWarning();
            }
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      実装関数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected void UnsupportedWarning()
        {
            Debug.LogWarningFormat("非対応関数が呼ばれました, 実行にはファームウェアの更新が必要です.\n現在のバージョン={0}\nバージョン情報URL={1}", this.version, "https://toio.github.io/toio-spec/versions");
        }
    }
}