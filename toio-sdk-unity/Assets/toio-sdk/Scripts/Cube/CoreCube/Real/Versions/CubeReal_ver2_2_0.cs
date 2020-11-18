using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class CubeReal_ver2_2_0 : CubeReal_ver2_1_0
    {
        private class MotorReadRequest
        {
            public bool valid;
            public float timeOutSec;
            public Action<bool, Cube> callback;
            public ORDER_TYPE order;
            public bool isRequesting = false;
            public bool hasMotorResponse = false;
            public bool hasConfigResponse = false;
            public bool isConfigResponseSucceeded = false;
            public bool wasTimeOut = false;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      内部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        private bool isInitialized = false;
        protected CallbackProvider _shakeCallback = new CallbackProvider();
        protected CallbackProvider _motorSpeedCallback = new CallbackProvider();
        private MotorReadRequest motorReadRequest = null;
        private int _leftSpeed = -1;
        private int _rightSpeed = -1;

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        public override bool isShake { get; protected set; }
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
                else if (!this.motorReadRequest.valid || !this.motorReadRequest.hasMotorResponse) { return -1; }
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
                else if (!this.motorReadRequest.valid || !this.motorReadRequest.hasMotorResponse) { return -1; }
                else { return this._rightSpeed; }
            }
            protected set { this.NotImplementedWarning(); }
        }

        // シェイクコールバック
        public override CallbackProvider shakeCallback { get { return this._shakeCallback; } }
        public override CallbackProvider motorSpeedCallback { get { return this._motorSpeedCallback; } }

        public CubeReal_ver2_2_0(BLEPeripheralInterface peripheral, Dictionary<string, BLECharacteristicInterface> characteristicTable)
        : base(peripheral, characteristicTable)
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
#if !RELEASE
            const float minTimeOut = 0.5f;
            if (minTimeOut > timeOutSec)
            {
                Debug.LogWarningFormat("[CubeReal_ver2_2_0.ConfigMotorRead]誤作動を避けるため, タイムアウト時間は {0} 秒以上にして下さい.", minTimeOut);
            }
#endif

            var startTime = Time.time;

            // 既に別の命令が実行されている場合は待機する
            while (null != this.motorReadRequest && this.motorReadRequest.isRequesting)
            {
                if ((startTime + timeOutSec) < Time.time)
                {
                    callback?.Invoke(false, this);
                    return;
                }
            }

            this.motorReadRequest = new MotorReadRequest();
            this.motorReadRequest.valid = valid;
            this.motorReadRequest.timeOutSec = timeOutSec;
            this.motorReadRequest.callback = callback;
            this.motorReadRequest.order = order;
            this.motorReadRequest.isRequesting = true;

            Action request = (() =>
            {
                byte[] buff = new byte[3];
                buff[0] = 0x1c;
                buff[1] = 0;
                buff[2] = BitConverter.GetBytes(valid)[0];
                this.Request(CHARACTERISTIC_CONFIG, buff, true, order, "ConfigMotorSpeed", valid, timeOutSec, callback, order);
            });

            while(!this.motorReadRequest.hasConfigResponse)
            {
                if ((startTime + timeOutSec) < Time.time)
                {
                    this.motorReadRequest.wasTimeOut = true;
                    break;
                }

                if (!this.isConnected || !this.isInitialized)
                {
                    await UniTask.Delay(50);
                    continue;
                }

                request();
                await UniTask.Delay(100);
            }
            this.motorReadRequest.isRequesting = false;
            callback?.Invoke(this.motorReadRequest.isConfigResponseSucceeded, this);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < subscribe >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        /// <summary>
        /// 自動通知機能の購読を開始する
        /// </summary>
        public override async UniTask Initialize()
        {
            await base.Initialize();
            this.characteristicTable[CHARACTERISTIC_MOTOR].StartNotifications(this.Recv_motor);
            this.characteristicTable[CHARACTERISTIC_CONFIG].StartNotifications(this.Recv_config);
            this.isInitialized = true;
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < recv >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected override void Recv_sensor(byte[] data)
        {
            base.Recv_sensor(data);

            // https://toio.github.io/toio-spec/docs/ble_sensor
            int type = data[0];
            if (1 == type)
            {
                var _isShake = data[5] == 1 ? true : false;

                if (_isShake != this.isShake)
                {
                    this.isShake = _isShake;
                    this.shakeCallback.Notify(this);
                }
            }
        }

        protected virtual void Recv_motor(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/ble_motor
            int type = data[0];
            if (0xe0 == type)
            {
                this.motorReadRequest.hasMotorResponse = true;
                this._leftSpeed = data[1];
                this._rightSpeed = data[2];
                this.motorSpeedCallback.Notify(this);
            }
        }


        protected virtual void Recv_config(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/ble_configuration
            int type = data[0];
            if (0x9c == type)
            {
                this.motorReadRequest.hasConfigResponse = true;
                this.motorReadRequest.isConfigResponseSucceeded = (0x00 == data[2]);
            }
        }
   }
}