using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class CubeReal_ver2_2_0 : CubeReal_ver2_1_0
    {

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      内部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected CallbackProvider<Cube> _shakeCallback = new CallbackProvider<Cube>();
        protected CallbackProvider<Cube> _motorSpeedCallback = new CallbackProvider<Cube>();
        protected CallbackProvider<Cube> _magnetStateCallback = new CallbackProvider<Cube>();
        protected bool motorReadValid = false;
        protected bool requestedMotorReadValid;
        protected MagneticMode magneticMode = MagneticMode.Off;
        protected MagneticMode requestedMagneticMode;
        private RequestInfo motorReadRequest = null;
        private RequestInfo idNotificationRequest = null;
        private RequestInfo idMissedNotificationRequest = null;
        protected RequestInfo magneticSensorRequest = null;
        private int _leftSpeed = -1;
        private int _rightSpeed = -1;
        protected MagnetState _magnetState = MagnetState.None;


        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        public override int shakeLevel { get; protected set; }
        public override MagnetState magnetState
        {
            get
            {
                if (null == this.magneticSensorRequest)
                {
                    Debug.Log("磁気センサーが有効化されていません. ConfigMagneticSensor関数を実行して有効化して下さい.");
                    return MagnetState.None;
                }
                else if (this.magneticMode != MagneticMode.MagnetState || !this.magneticSensorRequest.hasReceivedData)
                    return MagnetState.None;
                else
                    return this._magnetState;
            }
            protected set { this.NotImplementedWarning(); }
        }
        public override string version { get { return "2.2.0"; } }
        public override int leftSpeed
        {
            get
            {
                if (null == this.motorReadRequest)
                {
                    Debug.Log("モーター速度が有効化されていません. ConfigMotorRead関数を実行して有効化して下さい.");
                    return -1;
                }
                else if (!this.motorReadValid || !this.motorReadRequest.hasReceivedData) { return -1; }
                else { return this._leftSpeed; }
            }
            protected set { this.NotImplementedWarning(); }
        }
        public override int rightSpeed
        {
            get
            {
                if (null == this.motorReadRequest)
                {
                    Debug.Log("モーター速度が有効化されていません. ConfigMotorRead関数を実行して有効化して下さい.");
                    return -1;
                }
                else if (!this.motorReadValid || !this.motorReadRequest.hasReceivedData) { return -1; }
                else { return this._rightSpeed; }
            }
            protected set { this.NotImplementedWarning(); }
        }

        // シェイクコールバック
        public override CallbackProvider<Cube> shakeCallback { get { return this._shakeCallback; } }
        public override CallbackProvider<Cube> motorSpeedCallback { get { return this._motorSpeedCallback; } }
        public override CallbackProvider<Cube> magnetStateCallback { get { return this._magnetStateCallback; } }

        public CubeReal_ver2_2_0(BLEPeripheralInterface peripheral) : base(peripheral)
        {
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < send >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        /// <summary>
        /// キューブのモーター速度情報の取得の有効化・無効化を設定します
        /// https://toio.github.io/toio-spec/docs/ble_configuration#モーターの速度情報の取得の設定
        /// note:
        /// 設定リクエストの応答が時間差で返ってくるため、場合によってはリクエストと応答の実行順番が前後する可能性がある。
        /// 対策として、タイムアウトを長めに設定する事で呼び出し順番が前後する可能性を下げる事が出来る。そのためタイムアウトの最短時間に制限を加えている。
        /// </summary>
        /// <param name="valid">有効無効フラグ</param>
        /// <param name="timeOutSec">タイムアウト(秒)</param>
        /// <param name="callback">終了コールバック(設定成功フラグ, キューブ)</param>
        /// <param name="order">命令の優先度</param>
        public override async UniTask ConfigMotorRead(bool valid, float timeOutSec, Action<bool, Cube> callback, ORDER_TYPE order)
        {
            if (this.motorReadRequest == null) this.motorReadRequest = new RequestInfo(this);
            if (!this.isConnected || !this.isInitialized)
            {
                callback?.Invoke(false, this); return;
            }
#if !RELEASE
            const float minTimeOut = 0.5f;
            if (minTimeOut > timeOutSec)
            {
                Debug.LogWarningFormat("[CubeReal_ver2_2_0.ConfigMotorRead]誤作動を避けるため, タイムアウト時間は {0} 秒以上にして下さい.", minTimeOut);
            }
#endif
            bool availabe = await this.motorReadRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!availabe) return;

            this.requestedMotorReadValid = valid;

            this.motorReadRequest.request = (() =>
            {
                byte[] buff = new byte[3];
                buff[0] = 0x1c;
                buff[1] = 0;
                buff[2] = BitConverter.GetBytes(valid)[0];
                this.Request(CHARACTERISTIC_CONFIG, buff, true, order, "ConfigMotorRead", valid, timeOutSec, callback, order);
            });
            await this.motorReadRequest.Run();
        }

        /// <summary>
        /// 読み取りセンサーの Position ID および Standard ID の通知頻度を設定します。「最小通知間隔」と「通知条件」の両方を満たした場合に通知が行われます。
        /// https://toio.github.io/toio-spec/docs/ble_configuration#読み取りセンサーの-id-通知設定
        /// </summary>
        /// <param name="intervalMs">最小通知間隔(ミリ秒)</param>
        /// <param name="notificationType">通知条件</param>
        /// <param name="order">命令の優先度</param>
        public override async UniTask ConfigIDNotification(int intervalMs, IDNotificationType notificationType,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            if (this.idNotificationRequest == null) this.idNotificationRequest = new RequestInfo(this);
            if (!this.isConnected || !this.isInitialized)
            {
                callback?.Invoke(false, this); return;
            }
#if !RELEASE
            const float minTimeOut = 0.5f;
            if (minTimeOut > timeOutSec)
            {
                Debug.LogWarningFormat("[CubeReal_ver2_2_0.ConfigIDNotification]誤作動を避けるため, タイムアウト時間は {0} 秒以上にして下さい.", minTimeOut);
            }
#endif
            bool availabe = await this.idNotificationRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!availabe) return;

            this.idNotificationRequest.request = () =>
            {
                byte[] buff = new byte[4];
                buff[0] = 0x18;
                buff[1] = 0;
                buff[2] = BitConverter.GetBytes(Mathf.Clamp(intervalMs/10, 0, 255))[0];
                buff[3] = (byte)notificationType;
                this.Request(CHARACTERISTIC_CONFIG, buff, true, order, "ConfigIDNotification", intervalMs, notificationType, timeOutSec, callback, order);
            };
            await this.idNotificationRequest.Run();
        }


        /// <summary>
        /// 読み取りセンサーの Position ID missed および Standard ID missed の通知感度を設定します。
        /// https://toio.github.io/toio-spec/docs/ble_configuration#読み取りセンサーの-id-missed-通知設定
        /// </summary>
        /// <param name="sensitivityMs">通知感度(ミリ秒)</param>
        /// <param name="order">命令の優先度</param>
        public override async UniTask ConfigIDMissedNotification(int sensitivityMs,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            if (this.idMissedNotificationRequest == null) this.idMissedNotificationRequest = new RequestInfo(this);
            if (!this.isConnected || !this.isInitialized)
            {
                callback?.Invoke(false, this); return;
            }
#if !RELEASE
            const float minTimeOut = 0.5f;
            if (minTimeOut > timeOutSec)
            {
                Debug.LogWarningFormat("[CubeReal_ver2_2_0.ConfigIDMissedNotification]誤作動を避けるため, タイムアウト時間は {0} 秒以上にして下さい.", minTimeOut);
            }
#endif
            bool availabe = await this.idMissedNotificationRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!availabe) return;

            this.idMissedNotificationRequest.request = () =>
            {
                byte[] buff = new byte[3];
                buff[0] = 0x19;
                buff[1] = 0;
                buff[2] = BitConverter.GetBytes(Mathf.Clamp(sensitivityMs/10, 0, 255))[0];
                this.Request(CHARACTERISTIC_CONFIG, buff, true, order, "ConfigIDMissedNotification", sensitivityMs, timeOutSec, callback, order);
            };
            await this.idMissedNotificationRequest.Run();
        }

        /// <summary>
        /// キューブの磁気センサーの機能のモードを設定します。デフォルトでは無効化されています。
        /// https://toio.github.io/toio-spec/docs/ble_configuration#磁気センサーの設定
        /// </summary>
        /// <param name="mode">無効・磁石状態検出のいずれかを指定</param>
        /// <param name="timeOutSec">タイムアウト(秒)</param>
        /// <param name="callback">終了コールバック(設定成功フラグ, キューブ)</param>
        /// <param name="order">命令の優先度</param>
        public override async UniTask ConfigMagneticSensor(MagneticMode mode, float timeOutSec, Action<bool, Cube> callback, ORDER_TYPE order)
        {
            if (this.magneticSensorRequest == null) this.magneticSensorRequest = new RequestInfo(this);
            if (!this.isConnected || !this.isInitialized)
            {
                callback?.Invoke(false, this); return;
            }
#if !RELEASE
            const float minTimeOut = 0.5f;
            if (minTimeOut > timeOutSec)
            {
                Debug.LogWarningFormat("[CubeReal_ver2_2_0.ConfigMagneticSensor]誤作動を避けるため, タイムアウト時間は {0} 秒以上にして下さい.", minTimeOut);
            }
#endif
            // Assert mode
            if ((byte)mode > 1)
            {
                Debug.LogWarningFormat("Given MagneticSensorMode {0} is not supported by BLE Protocl v2.2.0", mode.ToString());
                return;
            }

            bool availabe = await this.magneticSensorRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!availabe) return;

            this.requestedMagneticMode = mode;

            this.magneticSensorRequest.request = () =>
            {
                byte[] buff = new byte[3];
                buff[0] = 0x1b;
                buff[1] = 0;
                buff[2] = (byte) mode;
                this.Request(CHARACTERISTIC_CONFIG, buff, true, order, "ConfigMagneticSensor", mode, timeOutSec, callback, order);
            };
            await this.magneticSensorRequest.Run();
        }

        /// <summary>
        /// モーションセンサー情報を要求します
        /// https://toio.github.io/toio-spec/docs/ble_sensor#書き込み操作
        /// </summary>
        /// <param name="order">命令の優先度</param>
        public override void RequestMotionSensor(ORDER_TYPE order)
        {
            if (!this.isConnected) { return; }

            byte[] buff = new byte[1];
            buff[0] = 0x81;

            this.Request(CHARACTERISTIC_SENSOR, buff, true, order, "RequestMotionSensor");
        }

        /// <summary>
        /// 磁気センサー情報を要求します
        /// https://toio.github.io/toio-spec/docs/ble_magnetic_sensor#磁気センサー情報の要求
        /// </summary>
        /// <param name="order">命令の優先度</param>
        public override void RequestMagneticSensor(ORDER_TYPE order)
        {
            if (!this.isConnected) { return; }

            byte[] buff = new byte[1];
            buff[0] = 0x82;

            this.Request(CHARACTERISTIC_SENSOR, buff, true, order, "RequestMagneticSensor");
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < subscribe >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        /// <summary>
        /// 自動通知機能の購読を開始する
        /// </summary>
        public override async UniTask Initialize(Dictionary<string, BLECharacteristicInterface> characteristicTable)
        {
            await base.Initialize(characteristicTable);
            isInitialized = false;
            this.characteristicTable[CHARACTERISTIC_MOTOR].StartNotifications(this.Recv_motor);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#endif
            this.characteristicTable[CHARACTERISTIC_CONFIG].StartNotifications(this.Recv_config);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#endif
            this.isInitialized = true;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < recv >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected override void Recv_sensor(byte[] data)
        {
            base.Recv_sensor(data);
            int type = data[0];

            // Motion Sensor https://toio.github.io/toio-spec/docs/2.2.0/ble_sensor
            if (1 == type)
            {
                var _shakeLevel = data[5];

                if (_shakeLevel != this.shakeLevel)
                {
                    this.shakeLevel = _shakeLevel;
                    this.shakeCallback.Notify(this);
                }
            }
            // Magnetic Sensor https://toio.github.io/toio-spec/docs/2.2.0/ble_magnetic_sensor
            else if (2 == type)
            {
                var _magnetState = (MagnetState) data[1];
                if (this.magneticSensorRequest != null)
                    this.magneticSensorRequest.hasReceivedData = true;

                this._magnetState = _magnetState;
                if (this.magneticMode == MagneticMode.MagnetState)
                    this.magnetStateCallback.Notify(this);
            }
        }

        //キューブのモーター速度情報を取得
        protected override void Recv_motor(byte[] data)
        {
            base.Recv_motor(data);

            // https://toio.github.io/toio-spec/docs/ble_motor
            int type = data[0];
            if (0xe0 == type)
            {
                if (this.motorReadRequest != null)
                    this.motorReadRequest.hasReceivedData = true;
                this._leftSpeed = data[1];
                this._rightSpeed = data[2];
                this.motorSpeedCallback.Notify(this);
            }
        }


        protected virtual void Recv_config(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/ble_configuration
            int type = data[0];
            if (0x9c == type)   // Motoer Read
            {
                this.motorReadRequest.hasConfigResponse = true;
                this.motorReadRequest.isConfigResponseSucceeded = (0x00 == data[2]);
                if (this.motorReadRequest.isConfigResponseSucceeded)
                    this.motorReadValid = this.requestedMotorReadValid;
                this.motorSpeedCallback.Notify(this);
            }

            else if (0x98 == type)  // ID Notification
            {
                this.idNotificationRequest.hasConfigResponse = true;
                this.idNotificationRequest.isConfigResponseSucceeded = (0x00 == data[2]);
            }

            else if (0x99 == type)  // ID Missed Notification
            {
                this.idMissedNotificationRequest.hasConfigResponse = true;
                this.idMissedNotificationRequest.isConfigResponseSucceeded = (0x00 == data[2]);
            }

            else if (0x9b == type)   // Mag
            {
                this.magneticSensorRequest.hasConfigResponse = true;
                this.magneticSensorRequest.isConfigResponseSucceeded = (0x00 == data[2]);
                if (this.magneticSensorRequest.isConfigResponseSucceeded)
                    this.magneticMode = this.requestedMagneticMode;
            }
        }
   }
}