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

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        public override bool isShake { get; protected set; }
        public override int leftSpeed { get; protected set; }
        public override int rightSpeed { get; protected set; }
        public override string version { get { return "2.2.0"; } }

        // シェイクコールバック
        public override CallbackProvider shakeCallback { get { return this._shakeCallback; } }
        public override CallbackProvider motorSpeedCallback { get { return this._motorSpeedCallback; } }

        public CubeReal_ver2_2_0(BLEPeripheralInterface peripheral, Dictionary<string, BLECharacteristicInterface> characteristicTable)
        : base(peripheral, characteristicTable)
        {
        }

        protected override void Recv_sensor(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/ble_sensor
            int type = data[0];
            if (1 == type)
            {
                var _isSloped = data[1] == 0 ? true : false;
                var _isCollisionDetected = data[2] == 1 ? true : false;
                var _isDoubleTap = data[3] == 1 ? true : false;
                PoseType _pose = (PoseType)data[4];
                var _isShake = data[5] == 1 ? true : false;

                if (_isSloped != this.isSloped)
                {
                    this.isSloped = _isSloped;
                    this.slopeCallback.Notify(this);
                }

                if (_isCollisionDetected != this.isCollisionDetected)
                {
                    this.isCollisionDetected = _isCollisionDetected;
                    this.collisionCallback.Notify(this);
                }

                if (_isDoubleTap != this.isDoubleTap)
                {
                    this.isDoubleTap = _isDoubleTap;
                    this.doubleTapCallback.Notify(this);
                }

                if (_pose != this.pose)
                {
                    this.pose = _pose;
                    this.poseCallback.Notify(this);
                }

                if (_isShake != this.isShake)
                {
                    this.isShake = _isShake;
                    this.shakeCallback.Notify(this);
                }
            }
        }


        // キューブのモーター速度情報の取得を有効化します
        protected void MotorReadValidation(bool valid)
        {
            if (!this.isConnected) { return; }
            byte[] buff = new byte[3];
            buff[0] = 0x1c;
            buff[1] = 0;
            buff[2] = BitConverter.GetBytes(valid)[0];
        }

        //キューブのモーター速度情報を取得
        protected void Recv_motor(byte[] data)
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

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < subscribe >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        /// <summary>
        /// 自動通知機能の購読を開始する
        /// </summary>

        public override async UniTask Initialize()
        {
            characteristicTable[CHARACTERISTIC_BATTERY].StartNotifications(this.Recv_battery);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#else
            await UniTask.Delay(1);
#endif
            characteristicTable[CHARACTERISTIC_ID].StartNotifications(this.Recv_Id);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#endif
            this.characteristicTable[CHARACTERISTIC_BUTTON].StartNotifications(this.Recv_button);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#endif
            this.characteristicTable[CHARACTERISTIC_SENSOR].StartNotifications(this.Recv_sensor);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#endif

        // モーターの速度情報の取得の有効化
            this.MotorReadValidation(true);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#endif

            this.characteristicTable[CHARACTERISTIC_MOTOR].StartNotifications(this.Recv_motor);
#if !UNITY_EDITOR && UNITY_ANDROID
            await UniTask.Delay(500);
#endif
        }
    }
}