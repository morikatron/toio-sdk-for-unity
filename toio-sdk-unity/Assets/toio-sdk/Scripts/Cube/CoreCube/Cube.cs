using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
        // Complete local name
        public abstract string localName { get; }
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
        // コアキューブのモーター指令のデッドゾーン
        public abstract int deadzone { get; }

        // ver2.1.0
        // コアキューブのダブルタップ状態
        public virtual bool isDoubleTap {
            get{NotSupportedWarning(); return default;}
            protected set{NotSupportedWarning();}}
        // コアキューブの姿勢状態
        public virtual PoseType pose {
            get{NotSupportedWarning(); return default;}
            protected set{NotSupportedWarning();}}

        // ver2.2.0
        // コアキューブのシェイク状態
        public virtual int shakeLevel {
            get{NotSupportedWarning(); return default;}
            protected set{NotSupportedWarning();}}
        // コアキューブのモーター ID 1（左）の速度
        public virtual int leftSpeed {
            get{NotSupportedWarning(); return default;}
            protected set{NotSupportedWarning();}}
        // コアキューブのモーター ID 2（右）の速度
        public virtual int rightSpeed {
            get{NotSupportedWarning(); return default;}
            protected set{NotSupportedWarning();}}
        // コアキューブの磁石状態
        public virtual MagnetState magnetState {
            get{NotSupportedWarning(); return default;}
            protected set{NotSupportedWarning();}}

        // ver2.3.0
        // コアキューブの磁力
        public virtual Vector3 magneticForce {
            get{NotSupportedWarning(); return default;}
            protected set{NotSupportedWarning();}}

        // コアキューブのオイラー
        public virtual Vector3 eulers {
            get{NotSupportedWarning(); return default;}
            protected set{NotSupportedWarning();}}
        // コアキューブのクォータニオン
        public virtual Quaternion quaternion {
            get{NotSupportedWarning(); return default;}
            protected set{NotSupportedWarning();}}


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
        public virtual void Move(int left, int right, int durationMs, ORDER_TYPE order = ORDER_TYPE.Weak) { NotSupportedWarning(); }

        /// <summary>
        /// キューブ底面についている LED を制御します
        /// https://toio.github.io/toio-spec/docs/ble_light#点灯-消灯
        /// </summary>
        /// <param name="red">赤色の強さ</param>
        /// <param name="green">緑色の強さ</param>
        /// <param name="blue">青色の強さ</param>
        /// <param name="durationMs">持続時間(ミリ秒)</param>
        /// <param name="order">命令の優先度</param>
        public virtual void TurnLedOn(int red, int green, int blue, int durationMs, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// キューブ底面についている LED を連続的に制御します
        /// https://toio.github.io/toio-spec/docs/ble_light#連続的な点灯-消灯
        /// </summary>
        /// <param name="repeatCount">繰り返し回数</param>
        /// <param name="operations">命令配列</param>
        /// <param name="order">命令の優先度</param>
        public virtual void TurnOnLightWithScenario(int repeatCount, Cube.LightOperation[] operations, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// キューブ底面についている LED を消灯させます
        /// https://toio.github.io/toio-spec/docs/ble_light#全てのランプを消灯
        /// </summary>
        /// <param name="order">命令の優先度</param>
        public virtual void TurnLedOff(ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// キューブからあらかじめ用意された効果音を再生します
        /// https://toio.github.io/toio-spec/docs/ble_sound#効果音の再生
        /// </summary>
        /// <param name="soundId">サウンドID</param>
        /// <param name="volume">音量</param>
        /// <param name="order">命令の優先度</param>
        public virtual void PlayPresetSound(int soundId, int volume = 255, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// キューブから任意の音を再生します
        /// https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生
        /// </summary>
        /// <param name="repeatCount">繰り返し回数</param>
        /// <param name="operations">命令配列</param>
        /// <param name="order">命令の優先度</param>
        public virtual void PlaySound(int repeatCount, SoundOperation[] operations, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// キューブから任意の音を再生します
        /// https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生
        /// </summary>
        /// <param name="buff">命令プロトコル</param>
        /// <param name="order">命令の優先度</param>
        public virtual void PlaySound(byte[] buff, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// キューブの音再生を停止します
        /// https://toio.github.io/toio-spec/docs/ble_sound#再生の停止
        /// </summary>
        /// <param name="order">命令の優先度</param>
        public virtual void StopSound(ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// キューブの水平検出のしきい値を設定します
        /// https://toio.github.io/toio-spec/docs/ble_configuration#水平検出のしきい値設定
        /// </summary>
        /// <param name="angle">傾き検知の閾値</param>
        /// <param name="order">命令の優先度</param>
        public virtual void ConfigSlopeThreshold(int angle, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// キューブの衝突検出のしきい値を設定します
        /// https://toio.github.io/toio-spec/docs/ble_configuration#衝突検出のしきい値設定
        /// </summary>
        /// <param name="level">衝突検知の閾値</param>
        /// <param name="order">命令の優先度</param>
        public virtual void ConfigCollisionThreshold(int level, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// キューブのダブルタップ検出の時間間隔を設定します
        /// https://toio.github.io/toio-spec/docs/ble_configuration#ダブルタップ検出の時間間隔の設定
        /// </summary>
        /// <param name="interval">ダブルタップ検出の時間間隔</param>
        /// <param name="order">命令の優先度</param>
        public virtual void ConfigDoubleTapInterval(int interval, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// キューブのモーターを目標指定付き制御します
        /// https://toio.github.io/toio-spec/docs/ble_motor#目標指定付きモーター制御
        /// </summary>
        /// <param name="targetX">目標地点のX座標値</param>
        /// <param name="targetY">目標地点のY座標値</param>
        /// <param name="targetAngle">目標地点でのキューブの角度Θ</param>
        /// <param name="configID">制御識別値</param>
        /// <param name="timeOut">タイムアウト時間(秒)</param>
        /// <param name="targetMoveType">移動タイプ</param>
        /// <param name="maxSpd">モーターの最大速度指示値</param>
        /// <param name="targetSpeedType">モーターの速度変化タイプ</param>
        /// <param name="targetRotationType">回転タイプ</param>
        /// <param name="order">命令の優先度</param>
        public virtual void TargetMove(
            int targetX,
            int targetY,
            int targetAngle,
            int configID = 0,
            int timeOut = 0,
            TargetMoveType targetMoveType = TargetMoveType.RotatingMove,
            int maxSpd = 80,
            TargetSpeedType targetSpeedType = TargetSpeedType.UniformSpeed,
            TargetRotationType targetRotationType = TargetRotationType.AbsoluteLeastAngle,
            ORDER_TYPE order = ORDER_TYPE.Strong
        ) { NotSupportedWarning(); }
        /*
        /// <summary>
        /// キューブのモーターを複数目標指定付き制御します
        /// https://toio.github.io/toio-spec/docs/ble_motor#複数目標指定付きモーター制御
        /// </summary>
        /// <param name="targetXList">目標地点のX座標値の集合</param>
        /// <param name="targetYList">目標地点のY座標値の集合</param>
        /// <param name="targetAngleList">目標地点でのキューブの角度Θの集合</param>
        /// <param name="multiRotationTypeList">回転タイプの集合</param>
        /// <param name="configID">制御識別値</param>
        /// <param name="timeOut">タイムアウト時間(秒)</param>
        /// <param name="targetMoveType">移動タイプ</param>
        /// <param name="maxSpd">モーターの最大速度指示値</param>
        /// <param name="targetSpeedType">モーターの速度変化タイプ</param>
        /// <param name="multiWriteType">書き込み操作の追加設定</param>
        /// <param name="order">命令の優先度</param>
        public virtual void MultiTargetMove(
            int[] targetXList,
            int[] targetYList,
            int[] targetAngleList,
            TargetRotationType[] multiRotationTypeList = null,
            int configID = 0,
            int timeOut = 0,
            TargetMoveType targetMoveType = TargetMoveType.RotatingMove,
            int maxSpd = 80,
            TargetSpeedType targetSpeedType = TargetSpeedType.UniformSpeed,
            MultiWriteType multiWriteType = MultiWriteType.Write,
            ORDER_TYPE order = ORDER_TYPE.Strong
        ){ NotSupportedWarning(); }
        */
        /// <summary>
        /// キューブの加速度指定付きモーターを制御します
        /// https://toio.github.io/toio-spec/docs/ble_motor#加速度指定付きモーター制御
        /// </summary>
        /// <param name="targetSpeed">キューブの並進速度</param>
        /// <param name="acceleration">キューブの加速度、100msごとの速度の増加分</param>
        /// <param name="rotationSpeed">キューブの向きの回転速度[度/秒]</param>
        /// <param name="accPriorityType">回転や並進の優先指定</param>
        /// <param name="controlTime">制御時間[10ms]</param>
        /// <param name="order">命令の優先度</param>
        public virtual void AccelerationMove(
            int targetSpeed,
            int acceleration,
            int rotationSpeed = 0,
            AccPriorityType accPriorityType = AccPriorityType.Translation,
            int controlTime = 0,
            ORDER_TYPE order = ORDER_TYPE.Strong
        ){ NotSupportedWarning(); }

        /// <summary>
        /// キューブのモーター速度情報の取得の有効化・無効化を設定します
        /// https://toio.github.io/toio-spec/docs/ble_configuration#モーターの速度情報の取得の設定
        /// </summary>
        /// <param name="valid">有効無効フラグ</param>
        /// <param name="timeOutSec">タイムアウト(秒)</param>
        /// <param name="callback">終了コールバック(設定成功フラグ, キューブ)</param>
        /// <param name="order">命令の優先度</param>
        public virtual UniTask ConfigMotorRead(bool valid, float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); return UniTask.CompletedTask; }

        /// <summary>
        /// 読み取りセンサーの Position ID および Standard ID の通知頻度を設定します。「最小通知間隔」と「通知条件」の両方を満たした場合に通知が行われます。
        /// https://toio.github.io/toio-spec/docs/ble_configuration#読み取りセンサーの-id-通知設定
        /// </summary>
        /// <param name="intervalMs">最小通知間隔(ミリ秒) 精度10ms</param>
        /// <param name="notificationType">通知条件</param>
        /// <param name="timeOutSec">タイムアウト(秒)</param>
        /// <param name="callback">終了コールバック(設定成功フラグ, キューブ)</param>
        /// <param name="order">命令の優先度</param>
        public virtual UniTask ConfigIDNotification(int intervalMs, IDNotificationType notificationType = IDNotificationType.Balanced,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); return UniTask.CompletedTask; }

        /// <summary>
        /// 読み取りセンサーの Position ID missed および Standard ID missed の通知感度を設定します。
        /// https://toio.github.io/toio-spec/docs/ble_configuration#読み取りセンサーの-id-missed-通知設定
        /// </summary>
        /// <param name="sensitivityMs">通知感度(ミリ秒) 精度10ms</param>
        /// <param name="timeOutSec">タイムアウト(秒)</param>
        /// <param name="callback">終了コールバック(設定成功フラグ, キューブ)</param>
        /// <param name="order">命令の優先度</param>
        public virtual UniTask ConfigIDMissedNotification(int sensitivityMs,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); return UniTask.CompletedTask; }

        /// <summary>
        /// キューブの磁気センサーの機能のモードを設定します。デフォルトでは無効化されています。(v2.2.0から対応)
        /// https://toio.github.io/toio-spec/docs/ble_configuration#磁気センサーの設定
        /// </summary>
        /// <param name="mode">モード</param>
        /// <param name="timeOutSec">タイムアウト(秒)</param>
        /// <param name="callback">終了コールバック(設定成功フラグ, キューブ)</param>
        /// <param name="order">命令の優先度</param>
        public virtual UniTask ConfigMagneticSensor(MagneticMode mode, float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); return UniTask.CompletedTask; }

        /// <summary>
        /// キューブの磁気センサーの機能のモードを設定します。デフォルトでは無効化されています。(v2.3.0から対応)
        /// https://toio.github.io/toio-spec/docs/ble_configuration#磁気センサーの設定
        /// </summary>
        /// <param name="mode">モード</param>
        /// <param name="intervalMs">通知間隔(ミリ秒) 精度20ms (v2.3.0以上対応)</param>
        /// <param name="notificationType">通知条件 (v2.3.0以上対応)</param>
        /// <param name="timeOutSec">タイムアウト(秒)</param>
        /// <param name="callback">終了コールバック(設定成功フラグ, キューブ)</param>
        /// <param name="order">命令の優先度</param>
        public virtual UniTask ConfigMagneticSensor(MagneticMode mode, int intervalMs, MagneticNotificationType notificationType,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); return UniTask.CompletedTask; }

        /// <summary>
        /// キューブの姿勢角検出機能の有効化・無効化を設定します。デフォルトでは無効化されています。(v2.3.0から対応)
        /// https://toio.github.io/toio-spec/docs/ble_configuration#姿勢角検出の設定
        /// </summary>
        /// <param name="format">通知内容の種類</param>
        /// <param name="intervalMs">通知間隔(ミリ秒) 精度10ms</param>
        /// <param name="notificationType">通知条件</param>
        /// <param name="timeOutSec">タイムアウト(秒)</param>
        /// <param name="callback">終了コールバック(設定成功フラグ, キューブ)</param>
        /// <param name="order">命令の優先度</param>
        public virtual UniTask ConfigAttitudeSensor(AttitudeFormat format, int intervalMs, AttitudeNotificationType notificationType,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); return UniTask.CompletedTask; }

        /// <summary>
        /// モーションセンサー情報を要求します
        /// https://toio.github.io/toio-spec/docs/ble_sensor#モーション検出情報の要求
        /// </summary>
        /// <param name="order">命令の優先度</param>
        public virtual void RequestMotionSensor(ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// (Deprecated. RequestMotionSensor を使ってください) モーションセンサー情報を要求します
        /// https://toio.github.io/toio-spec/docs/ble_sensor#モーション検出情報の要求
        /// </summary>
        /// <param name="order">命令の優先度</param>
        [Obsolete("RequestSensor is deprecated. Use RequestMotionSensor instead.", false)]
        public void RequestSensor(ORDER_TYPE order = ORDER_TYPE.Strong) { RequestMotionSensor(order); }

        /// <summary>
        /// 磁気センサー情報を要求します
        /// https://toio.github.io/toio-spec/docs/ble_magnetic_sensor#磁気センサー情報の要求
        /// </summary>
        /// <param name="order">命令の優先度</param>
        public virtual void RequestMagneticSensor(ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }

        /// <summary>
        /// 姿勢角検出情報を要求します
        /// https://toio.github.io/toio-spec/docs/ble_high_precision_tilt_sensor#姿勢角検出の要求
        /// </summary>
        /// <param name="format">通知内容の種類（オイラーかクォータニオンか）</param>
        /// <param name="order">命令の優先度</param>
        public virtual void RequestAttitudeSensor(AttitudeFormat format, ORDER_TYPE order = ORDER_TYPE.Strong) { NotSupportedWarning(); }


        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      コールバック
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        // ボタンコールバック
        public virtual CallbackProvider<Cube> buttonCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }
        // 傾きコールバック
        public virtual CallbackProvider<Cube> slopeCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }
        // 衝突コールバック
        public virtual CallbackProvider<Cube> collisionCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }
        // 座標角度コールバック
        public virtual CallbackProvider<Cube> idCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }
        // StandardID コールバック
        public virtual CallbackProvider<Cube> standardIdCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }
        // ID Missed コールバック
        public virtual CallbackProvider<Cube> idMissedCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }
        // StandardID Missed コールバック
        public virtual CallbackProvider<Cube> standardIdMissedCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }

        // ver2.1.0
        // Double Tap コールバック
        public virtual CallbackProvider<Cube> doubleTapCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }
        // 姿勢検出コールバック
        public virtual CallbackProvider<Cube> poseCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }
        // 目標指定付きモーター制御の応答コールバック
        public virtual CallbackProvider<Cube, int, TargetMoveRespondType> targetMoveCallback { get { return CallbackProvider<Cube, int, TargetMoveRespondType>.NotSupported.Get(this); } }
        // 複数目標指定付きモーター制御の応答コールバック
        // public virtual CallbackProvider<Cube, int, TargetMoveRespondType> multiTargetMoveCallback { get { return CallbackProvider<Cube, int, TargetMoveRespondType>.NotSupported.Get(this); } }

        // ver2.2.0
        // シェイクコールバック
        public virtual CallbackProvider<Cube> shakeCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }
        // モータースピードコールバック
        public virtual CallbackProvider<Cube> motorSpeedCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }
        // 磁石状態コールバック
        public virtual CallbackProvider<Cube> magnetStateCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }

        // ver2.3.0
        // 磁力検出コールバック
        public virtual CallbackProvider<Cube> magneticForceCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }

        // 姿勢角検出コールバック
        public virtual CallbackProvider<Cube> attitudeCallback { get { return CallbackProvider<Cube>.NotSupported.Get(this); } }


        public Cube()
        {
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      内部クラス
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        // 発音ごとの設定構造体
        public struct SoundOperation
        {
            public int durationMs;   // ミリ秒
            public byte volume;      // 音量(0~255)
            public byte note_number; // 音符(0~128)

            public SoundOperation(int durationMs = 0, byte volume = 0, byte note_number = 0)
            {
                this.durationMs = durationMs;
                this.volume = volume;
                this.note_number = note_number;
            }

            public SoundOperation(int durationMs = 0, byte volume = 0, NOTE_NUMBER note_number = 0)
            {
                this.durationMs = durationMs;
                this.volume = volume;
                this.note_number = (byte)note_number;
            }
        }

        // 発光ごとの設定構造体
        public struct LightOperation
        {
            public int durationMs;   // ミリ秒
            public byte red;         // 赤色の強さ
            public byte green;       // 緑色の強さ
            public byte blue;        // 青色の強さ

            public LightOperation(int durationMs = 0, byte red = 0, byte green = 0, byte blue = 0)
            {
                this.durationMs = durationMs;
                this.red = red;
                this.green = green;
                this.blue = blue;
            }
        }

        // 目標指定付き制御のパラメータ種類
        public enum TargetMoveType: byte
        {
            // https://toio.github.io/toio-spec/docs/ble_motor#移動タイプ
            RotatingMove=0,       // 回転しながら移動
            RoundForwardMove=1,   // 回転しながら移動（後退なし）
            RoundBeforeMove=2     // 回転してから移動
        };

        public enum TargetSpeedType: byte
        {
            // https://toio.github.io/toio-spec/docs/ble_motor#モーターの速度変化タイプ
            UniformSpeed=0,   // 速度一定
            Acceleration=1,   // 目標地点まで徐々に加速
            Deceleration=2,   // 目標地点まで徐々に減速
            VariableSpeed=3   // 中間地点まで徐々に加速し、そこから目標地点まで減速
        };

        public enum TargetRotationType: byte
        {
            // https://toio.github.io/toio-spec/docs/ble_motor#目標地点でのキューブの角度-θ
            AbsoluteLeastAngle=0,         // 絶対角度 回転量が少ない方向
            AbsoluteClockwise=1,          // 絶対角度 正方向(時計回り)
            AbsoluteCounterClockwise=2,   // 絶対角度 負方向(反時計回り)
            RelativeClockwise=3,          // 相対角度 正方向(時計回り)
            RelativeCounterClockwise=4,   // 相対角度 負方向(反時計回り)
            NotRotate=5,                  // 回転しない
            Original=6                    // 書き込み操作時と同じ 回転量が少ない方向
        };

        public enum MultiWriteType: byte
        {
            // https://toio.github.io/toio-spec/docs/ble_motor#書き込み操作の追加設定
            Write=0,        // 上書き
            Add=1,          // 追加
        };

        // 加速度指定付き制御のパラメータ種類
        public enum AccPriorityType: byte
        {
            // https://toio.github.io/toio-spec/docs/ble_motor#優先指定
            Translation=0,      // 並進速度を優先し、回転速度を調整します
            Rotation=1,         // 回転速度を優先し、並進速度を調整します
        };

        // 制御の応答
        public enum TargetMoveRespondType: byte
        {
            // https://toio.github.io/toio-spec/docs/2.1.0/ble_motor#応答内容-1
            Normal=0,           // 目標に到達した時
            Timeout=1,          // 指定したタイムアウト時間を経過した時
            ToioIDmissed=2,     // toio ID がない場所にキューブが置かれた時
            ParameterError=3,   // 座標 X, 座標 Y, 角度の全てが現在と同じだった時
            PowerOffError=4,    // 電源を切られた時
            OtherWrite=5,       // 複数目標指定付きモーター制御以外のモーター制御が書き込まれた時
            NonSupport=6,       // 指定したモーターの最大速度指示値が8未満の時
            AddRefused=7        // 書き込み操作の追加ができない時
        };

        // 姿勢
        public enum PoseType: byte
        {
            Up=1,
            Down=2,
            Front=3,
            Back=4,
            Right=5,
            Left=6
        };

        // ID 通知条件
        public enum IDNotificationType: byte
        {
            Always = 0,
            OnChanged = 1,
            Balanced = 0xff
        }

        // 磁気センサーの設定 https://toio.github.io/toio-spec/docs/ble_configuration#_磁気センサーの設定_
        public enum MagneticMode: byte
        {
            Off = 0, MagnetState = 1, MagneticForce = 2
        }

        // 磁石の状態 https://toio.github.io/toio-spec/docs/2.2.0/hardware_magnet#磁石のレイアウト仕様
        public enum MagnetState: byte
        {
            None = 0, // 未装着
            S_Center = 1, N_Center = 2,
            S_Right = 3, N_Right = 4,
            S_Left = 5, N_Left = 6
        }

        // 磁気センサーの設定 https://toio.github.io/toio-spec/docs/ble_configuration#通知条件-1
        public enum MagneticNotificationType: byte
        {
            Always = 0, OnChanged = 1
        }

        // 姿勢角検出の設定 https://toio.github.io/toio-spec/docs/ble_configuration#通知条件-2
        public enum AttitudeFormat: byte
        {
            Eulers = 1, Quaternion = 2
        }

        // 姿勢角検出の設定 https://toio.github.io/toio-spec/docs/ble_configuration#通知条件-2
        public enum AttitudeNotificationType: byte
        {
            Always = 0, OnChanged = 1
        }

        // 効果音
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

        // 命令
        public enum ORDER_TYPE : int
        {
            Strong,
            Weak
        }

        public class CallbackProvider<T1>
        {
            public class NotSupported : CallbackProvider<T1>
            {
                public static Dictionary<string, NotSupported> versionTable = new Dictionary<string, NotSupported>();
                public static NotSupported Get(Cube cube)
                {
                    var version = cube.version;
                    if (!versionTable.ContainsKey(version))
                    {
                        version = string.Copy(version);
                        versionTable.Add(version, new NotSupported(version));
                    }
                    return versionTable[version];
                }
                private string version;
                public NotSupported(string version) { this.version = version; }
                public override void AddListener(string key, Action<T1> listener) { NotSupportedWarning(this.version); }
                public override void RemoveListener(string key) { NotSupportedWarning(this.version); }
                public override void ClearListener() { NotSupportedWarning(this.version); }
                public override void Notify(T1 p1) { NotSupportedWarning(this.version); }
            }

            protected Dictionary<string, Action<T1>> listenerTable = new Dictionary<string, Action<T1>>();
            protected List<Action<T1>> listenerList = new List<Action<T1>>();

            public virtual void AddListener(string key, Action<T1> listener)
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
                this.listenerTable.Clear();
                this.listenerList.Clear();
            }
            public virtual void Notify(T1 p1)
            {
                for (int i = this.listenerList.Count-1; i >= 0; i--)
                {
                    this.listenerList[i].Invoke(p1);
                }
            }
        }

        public class CallbackProvider<T1, T2>
        {
            public class NotSupported : CallbackProvider<T1, T2>
            {
                public static Dictionary<string, NotSupported> versionTable = new Dictionary<string, NotSupported>();
                public static NotSupported Get(Cube cube)
                {
                    var version = cube.version;
                    if (!versionTable.ContainsKey(version))
                    {
                        version = string.Copy(version);
                        versionTable.Add(version, new NotSupported(version));
                    }
                    return versionTable[version];
                }
                private string version;
                public NotSupported(string version) { this.version = version; }
                public override void AddListener(string key, Action<T1, T2> listener) { NotSupportedWarning(this.version); }
                public override void RemoveListener(string key) { NotSupportedWarning(this.version); }
                public override void ClearListener() { NotSupportedWarning(this.version); }
                public override void Notify(T1 p1, T2 p2) { NotSupportedWarning(this.version); }
            }

            protected Dictionary<string, Action<T1, T2>> listenerTable = new Dictionary<string, Action<T1, T2>>();
            protected List<Action<T1, T2>> listenerList = new List<Action<T1, T2>>();

            public virtual void AddListener(string key, Action<T1, T2> listener)
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
                this.listenerTable.Clear();
                this.listenerList.Clear();
            }
            public virtual void Notify(T1 p1, T2 p2)
            {
                for (int i = this.listenerList.Count-1; i >= 0; i--)
                {
                    this.listenerList[i].Invoke(p1, p2);
                }
            }
        }

        public class CallbackProvider<T1, T2, T3>
        {
            public class NotSupported : CallbackProvider<T1, T2, T3>
            {
                public static Dictionary<string, NotSupported> versionTable = new Dictionary<string, NotSupported>();
                public static NotSupported Get(Cube cube)
                {
                    var version = cube.version;
                    if (!versionTable.ContainsKey(version))
                    {
                        version = string.Copy(version);
                        versionTable.Add(version, new NotSupported(version));
                    }
                    return versionTable[version];
                }
                private string version;
                public NotSupported(string version) { this.version = version; }
                public override void AddListener(string key, Action<T1, T2, T3> listener) { NotSupportedWarning(this.version); }
                public override void RemoveListener(string key) { NotSupportedWarning(this.version); }
                public override void ClearListener() { NotSupportedWarning(this.version); }
                public override void Notify(T1 p1, T2 p2, T3 p3) { NotSupportedWarning(this.version); }
            }

            protected Dictionary<string, Action<T1, T2, T3>> listenerTable = new Dictionary<string, Action<T1, T2, T3>>();
            protected List<Action<T1, T2, T3>> listenerList = new List<Action<T1, T2, T3>>();

            public virtual void AddListener(string key, Action<T1, T2, T3> listener)
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
                this.listenerTable.Clear();
                this.listenerList.Clear();
            }
            public virtual void Notify(T1 p1, T2 p2, T3 p3)
            {
                for (int i = this.listenerList.Count-1; i >= 0; i--)
                {
                    this.listenerList[i].Invoke(p1, p2, p3);
                }
            }
        }

        public class CallbackProvider<T1, T2, T3, T4>
        {
            public class NotSupported : CallbackProvider<T1, T2, T3, T4>
            {
                public static Dictionary<string, NotSupported> versionTable = new Dictionary<string, NotSupported>();
                public static NotSupported Get(Cube cube)
                {
                    var version = cube.version;
                    if (!versionTable.ContainsKey(version))
                    {
                        version = string.Copy(version);
                        versionTable.Add(version, new NotSupported(version));
                    }
                    return versionTable[version];
                }
                private string version;
                public NotSupported(string version) { this.version = version; }
                public override void AddListener(string key, Action<T1, T2, T3, T4> listener) { NotSupportedWarning(this.version); }
                public override void RemoveListener(string key) { NotSupportedWarning(this.version); }
                public override void ClearListener() { NotSupportedWarning(this.version); }
                public override void Notify(T1 p1, T2 p2, T3 p3, T4 p4) { NotSupportedWarning(this.version); }
            }

            protected Dictionary<string, Action<T1, T2, T3, T4>> listenerTable = new Dictionary<string, Action<T1, T2, T3, T4>>();
            protected List<Action<T1, T2, T3, T4>> listenerList = new List<Action<T1, T2, T3, T4>>();

            public virtual void AddListener(string key, Action<T1, T2, T3, T4> listener)
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
                this.listenerTable.Clear();
                this.listenerList.Clear();
            }
            public virtual void Notify(T1 p1, T2 p2, T3 p3, T4 p4)
            {
                for (int i = this.listenerList.Count-1; i >= 0; i--)
                {
                    this.listenerList[i].Invoke(p1, p2, p3, p4);
                }
            }
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      実装関数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected void NotSupportedWarning()
        {
            NotSupportedWarning(this.version);
        }

        protected static void NotSupportedWarning(string version)
        {
            Debug.LogWarningFormat("非対応関数が呼ばれました, 実行にはファームウェアの更新が必要です.\n現在のバージョン={0}\nバージョン情報URL={1}", version, "https://toio.github.io/toio-spec/versions");
        }

        protected void NotImplementedWarning()
        {
            Debug.LogWarning("実装が存在しない機能が呼ばれました.");
        }
    }
}