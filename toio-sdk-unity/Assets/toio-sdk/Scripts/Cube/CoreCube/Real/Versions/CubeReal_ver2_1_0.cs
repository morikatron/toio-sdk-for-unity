using System;
using System.Collections.Generic;
using UnityEngine;

namespace toio
{
    public class CubeReal_ver2_1_0 : CubeReal_ver2_0_0
    {
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      内部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected CallbackProvider _doubleTapCallback = new CallbackProvider();
        protected CallbackProvider _poseCallback = new CallbackProvider();
        protected CallbackProvider _targetMoveCallback = new CallbackProvider();
        protected CallbackProvider _multiTargetMoveCallback = new CallbackProvider();

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        public override bool isDoubleTap { get; protected set; }
        public override PoseType pose { get; protected set; }
        public override int motorConfigID {get;protected set;}
        public override TargetMoveRespondType motorRespond {get;protected set;}
        public override int multiConfigID {get;protected set;}
        public override TargetMoveRespondType multiRespond {get;protected set;}
        public override string version { get { return "2.1.0"; } }
        public override int maxSpd { get { return 115; } }
        public override int deadzone { get { return 8; } }

        // ダブルタップコールバック
        public override CallbackProvider doubleTapCallback { get { return this._doubleTapCallback; } }
        // 姿勢コールバック
        public override CallbackProvider poseCallback { get { return this._poseCallback; } }
        // 目標指定付きモーター制御の応答コールバック
        public override CallbackProvider targetMoveCallback { get { return this._targetMoveCallback; } }
        // 複数目標指定付きモーター制御の応答コールバック
        public override CallbackProvider multiTargetMoveCallback { get { return this._multiTargetMoveCallback; } }

        public CubeReal_ver2_1_0(BLEPeripheralInterface peripheral, Dictionary<string, BLECharacteristicInterface> characteristicTable)
        : base(peripheral, characteristicTable)
        {
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < send >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        // キューブのモーターを目標指定付き制御します
        public override void TargetMove(TargetMoveConfig config, ORDER_TYPE order)
        {
            if (!this.isConnected) { return; }

            byte[] buff = new byte[13];
            buff[0] = 3;
            buff[1] = (byte)(config.configID & 0xFF);
            buff[2] = (byte)(config.timeOut & 0xFF);
            buff[3] = (byte)config.targetMoveType;
            buff[4] = (byte)Mathf.Clamp(config.setMaxSpd, deadzone, maxSpd);
            buff[5] = (byte)config.targetSpeedType;
            buff[6] = 0;
            buff[7] = (byte)(config.targetX & 0xFF);
            buff[8] = (byte)((config.targetX >> 8) & 0xFF);
            buff[9] = (byte)(config.targetY & 0xFF);
            buff[10] = (byte)((config.targetY >> 8) & 0xFF);
            buff[11] = (byte)(config.targetAngle & 0xFF);
            buff[12] = (byte)((((int)config.targetRotationType & 0x0007) << 5 ) | ((config.targetAngle & 0x1FFF) >> 8));

            this.Request(CHARACTERISTIC_MOTOR, buff, false, order, "TargetMove", config);
        }

        // キューブのモーターを複数目標指定付き制御します
        public override void MultiTargetMove(MultiMoveConfig config, ORDER_TYPE order)
        {
            if (!this.isConnected) { return; }

            byte[] buff = new byte[config.targetXList.Length * 6 + 8];
            buff[0] = 4;
            buff[1] = (byte)(config.configID & 0xFF);
            buff[2] = (byte)(config.timeOut & 0xFF);
            buff[3] = (byte)config.targetMoveType;
            buff[4] = (byte)Mathf.Clamp(config.setMaxSpd, deadzone, maxSpd);
            buff[5] = (byte)config.targetSpeedType;
            buff[6] = 0;
            buff[7] = (byte)config.multiWriteType;

            for (int i = 0; i < config.targetXList.Length; i++)
            {
                buff[i * 6 + 8] = (byte)(config.targetXList[i] & 0xFF);
                buff[i * 6 + 9] = (byte)((config.targetXList[i] >> 8) & 0xFF);
                buff[i * 6 + 10] = (byte)(config.targetYList[i] & 0xFF);
                buff[i * 6 + 11] = (byte)((config.targetYList[i] >> 8) & 0xFF);
                buff[i * 6 + 12] = (byte)(config.targetAngleList[i] & 0xFF);
                buff[i * 6 + 13] = (byte)((((int)config.multiRotationTypeList[i] & 0x0007) << 5 ) | ((config.targetAngleList[i] & 0x1FFF) >> 8));
            }
            this.Request(CHARACTERISTIC_MOTOR, buff, false, order, "MultiTargetMove", config);
        }

        // キューブの加速度指定付きモーターを制御します
        public override void AccelerationMove(AccMoveConfig config, ORDER_TYPE order)
        {
            if (!this.isConnected) { return; }
            byte[] buff = new byte[9];
            buff[0] = 5;
            buff[1] = (byte)(Mathf.Clamp(config.targetSpeed, deadzone, maxSpd) & 0xFF);
            buff[2] = (byte)(config.Acceleration & 0xFF);
            buff[3] = (byte)(config.rotationSpeed & 0xFF);
            buff[4] = (byte)((config.rotationSpeed >> 8) & 0xFF);
            buff[5] = (byte)config.accRotationType;
            buff[6] = (byte)config.accMoveType;
            buff[7] = (byte)config.accSpeedPriorityType;
            buff[8] = (byte)(config.controlTime & 0xFF);

            this.Request(CHARACTERISTIC_MOTOR, buff, false, order, "AccelerationMove", config);
        }

        // キューブのダブルタップ検出の時間間隔を設定します
        public override void ConfigDoubleTapInterval(int interval, ORDER_TYPE order)
        {
            if (!this.isConnected) { return; }

            interval = Mathf.Clamp(interval, 1, 7);

            byte[] buff = new byte[3];
            buff[0] = 0x17;
            buff[1] = 0;
            buff[2] = BitConverter.GetBytes(interval)[0];

            this.Request(CHARACTERISTIC_CONFIG, buff, true, order, "ConfigDoubleTapinterval", interval);
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < recv >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        protected virtual void Recv_motor(byte[] data)
        {
            int type = data[0];
            // https://toio.github.io/toio-spec/docs/2.1.0/ble_motor#目標指定付きモーター制御の応答
            if (0x83 == type)
            {
                this.motorConfigID = data[1];
                this.motorRespond = (TargetMoveRespondType)data[2];
                this.targetMoveCallback.Notify(this);
            }
            // https://toio.github.io/toio-spec/docs/2.1.0/ble_motor#複数目標指定付きモーター制御の応答
            else if (0x84 == type)
            {
                this.multiConfigID = data[1];
                this.multiRespond = (TargetMoveRespondType)data[2];
                this.multiTargetMoveCallback.Notify(this);
            }
        }

        protected override void Recv_sensor(byte[] data)
        {
            base.Recv_sensor(data);

            // https://toio.github.io/toio-spec/docs/2.1.0/ble_sensor
            int type = data[0];
            if (1 == type)
            {
                var _isDoubleTap = data[3] == 1 ? true : false;
                PoseType _pose = (PoseType)data[4];

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
            }
        }
    }
}