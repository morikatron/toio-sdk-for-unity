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

        protected CallbackProvider _shakeCallback = new CallbackProvider();
        protected CallbackProvider _motorSpeedCallback = new CallbackProvider();
        private bool isInitialized = false;
        private bool isEnablingMotorSpeed = false;
        private bool isEnabledMotorSpeed = false;
        private int _leftSpeed = 0;
        private int _rightSpeed = 0;

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        public override bool isShake { get; protected set; }
        public override string version { get { return "2.2.0"; } }
        public override int leftSpeed
        {
            get
            {
                if (this.isEnabledMotorSpeed)
                    return this._leftSpeed;
                else if (this.isInitialized && !this.isEnablingMotorSpeed)
                    this.EnableMotorRead(true);
                return -1;
            }
            protected set { this._leftSpeed = value; }
        }
        public override int rightSpeed
        {
            get
            {
                if (this.isEnabledMotorSpeed)
                    return this._rightSpeed;
                else if (this.isInitialized && !this.isEnablingMotorSpeed)
                    this.EnableMotorRead(true);
                return -1;
            }
            protected set { this._rightSpeed = value; }
        }

        // シェイクコールバック
        public override CallbackProvider shakeCallback { get { return this._shakeCallback; } }
        public override CallbackProvider motorSpeedCallback { get { return this._motorSpeedCallback; } }

        public CubeReal_ver2_2_0(BLEPeripheralInterface peripheral, Dictionary<string, BLECharacteristicInterface> characteristicTable)
        : base(peripheral, characteristicTable)
        {
            this.motorSpeedCallback.onAddListener += (() =>
            {
                if (this.isInitialized && !this.isEnabledMotorSpeed && !this.isEnablingMotorSpeed)
                    this.EnableMotorRead(true);
            });
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < send >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        // キューブのモーター速度情報の取得を有効化します
        private void EnableMotorRead(bool valid)
        {
            if (!this.isConnected) { return; }
            this.isEnablingMotorSpeed = true;
            byte[] buff = new byte[3];
            buff[0] = 0x1c;
            buff[1] = 0;
            buff[2] = BitConverter.GetBytes(valid)[0];
            this.Request(CHARACTERISTIC_CONFIG, buff, true, ORDER_TYPE.Strong, "EnableMotorRead", valid);
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

        //キューブのモーター速度情報を取得
        protected virtual void Recv_motor(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/ble_motor
            int type = data[0];
            if (0xe0 == type)
            {
                this.leftSpeed = data[1];
                this.rightSpeed = data[2];
                this.motorSpeedCallback.Notify(this);
            }
        }

        protected virtual void Recv_config(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/ble_configuration
            int type = data[0];
            if (0x9c == type)
            {
                this.isEnablingMotorSpeed = false;
                this.isEnabledMotorSpeed = (0x00 == data[2]);
            }
        }
    }
}