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

        protected CallbackProvider<Cube> _doubleTapCallback = new CallbackProvider<Cube>();
        protected CallbackProvider<Cube> _poseCallback = new CallbackProvider<Cube>();
        protected CallbackProvider<Cube, int, TargetMoveRespondType> _targetMoveCallback = new CallbackProvider<Cube, int, TargetMoveRespondType>();
        protected CallbackProvider<Cube, int, TargetMoveRespondType> _multiTargetMoveCallback = new CallbackProvider<Cube, int, TargetMoveRespondType>();

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        public override bool isDoubleTap { get; protected set; }
        public override PoseType pose { get; protected set; }
        public override string version { get { return "2.1.0"; } }
        public override int maxSpd { get { return 115; } }
        public override int deadzone { get { return 8; } }

        // ダブルタップコールバック
        public override CallbackProvider<Cube> doubleTapCallback { get { return this._doubleTapCallback; } }
        // 姿勢コールバック
        public override CallbackProvider<Cube> poseCallback { get { return this._poseCallback; } }
        // 目標指定付きモーター制御の応答コールバック
        public override CallbackProvider<Cube, int, TargetMoveRespondType> targetMoveCallback { get { return this._targetMoveCallback; } }
        // 複数目標指定付きモーター制御の応答コールバック
        public override CallbackProvider<Cube, int, TargetMoveRespondType> multiTargetMoveCallback { get { return this._multiTargetMoveCallback; } }

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
        public override void TargetMove(
            int targetX,
            int targetY,
            int targetAngle,
            byte configID = 0,
            byte timeOut = 0,
            TargetMoveType targetMoveType = TargetMoveType.RotatingMove,
            byte maxSpd = 80,
            TargetSpeedType targetSpeedType = TargetSpeedType.UniformSpeed,
            TargetRotationType targetRotationType = TargetRotationType.AbsoluteLeastAngle,
            ORDER_TYPE order = ORDER_TYPE.Strong
        ){
            if (!this.isConnected) { return; }

            byte[] buff = new byte[13];
            buff[0] = 3;
            buff[1] = (byte)(configID & 0xFF);
            buff[2] = (byte)(timeOut & 0xFF);
            buff[3] = (byte)targetMoveType;
            buff[4] = (byte)Mathf.Clamp(maxSpd, deadzone, this.maxSpd);
            buff[5] = (byte)targetSpeedType;
            buff[6] = 0;
            buff[7] = (byte)(targetX & 0xFF);
            buff[8] = (byte)((targetX >> 8) & 0xFF);
            buff[9] = (byte)(targetY & 0xFF);
            buff[10] = (byte)((targetY >> 8) & 0xFF);
            buff[11] = (byte)(targetAngle & 0xFF);
            buff[12] = (byte)((((int)targetRotationType & 0x0007) << 5 ) | ((targetAngle & 0x1FFF) >> 8));

            this.Request(CHARACTERISTIC_MOTOR, buff, false, order, "TargetMove",
                targetX, targetY, targetAngle, configID, timeOut, targetMoveType, maxSpd, targetSpeedType, targetRotationType);
        }

        // キューブのモーターを複数目標指定付き制御します
        public override void MultiTargetMove(
            int[] targetXList,
            int[] targetYList,
            int[] targetAngleList,
            TargetRotationType[] multiRotationTypeList = null,
            byte configID = 0,
            byte timeOut = 0,
            TargetMoveType targetMoveType = TargetMoveType.RotatingMove,
            byte maxSpd = 80,
            TargetSpeedType targetSpeedType = TargetSpeedType.UniformSpeed,
            MultiWriteType multiWriteType = MultiWriteType.Write,
            ORDER_TYPE order = ORDER_TYPE.Strong
        ){
            if (!this.isConnected) { return; }

            multiRotationTypeList = multiRotationTypeList==null? new TargetRotationType[targetXList.Length] : multiRotationTypeList;

            byte[] buff = new byte[targetXList.Length * 6 + 8];
            buff[0] = 4;
            buff[1] = (byte)(configID & 0xFF);
            buff[2] = (byte)(timeOut & 0xFF);
            buff[3] = (byte)targetMoveType;
            buff[4] = (byte)Mathf.Clamp(maxSpd, deadzone, this.maxSpd);
            buff[5] = (byte)targetSpeedType;
            buff[6] = 0;
            buff[7] = (byte)multiWriteType;

            for (int i = 0; i < targetXList.Length; i++)
            {
                buff[i * 6 + 8] = (byte)(targetXList[i] & 0xFF);
                buff[i * 6 + 9] = (byte)((targetXList[i] >> 8) & 0xFF);
                buff[i * 6 + 10] = (byte)(targetYList[i] & 0xFF);
                buff[i * 6 + 11] = (byte)((targetYList[i] >> 8) & 0xFF);
                buff[i * 6 + 12] = (byte)(targetAngleList[i] & 0xFF);
                buff[i * 6 + 13] = (byte)((((int)multiRotationTypeList[i] & 0x0007) << 5 ) | ((targetAngleList[i] & 0x1FFF) >> 8));
            }
            this.Request(CHARACTERISTIC_MOTOR, buff, false, order, "MultiTargetMove",
                targetXList, targetYList, targetAngleList, multiRotationTypeList, configID, timeOut,
                targetMoveType, maxSpd, targetSpeedType, multiWriteType);
        }

        // キューブの加速度指定付きモーターを制御します
        public override void AccelerationMove(
            int targetSpeed,
            int acceleration,
            ushort rotationSpeed = 0,
            AccRotationType accRotationType = AccRotationType.Clockwise,
            AccMoveType accMoveType = AccMoveType.Forward,
            AccPriorityType accPriorityType = AccPriorityType.Translation,
            byte controlTime = 0,
            ORDER_TYPE order = ORDER_TYPE.Strong
        ){
            if (!this.isConnected) { return; }
            byte[] buff = new byte[9];
            buff[0] = 5;
            buff[1] = (byte)(Mathf.Clamp(targetSpeed, deadzone, maxSpd) & 0xFF);
            buff[2] = (byte)(acceleration & 0xFF);
            buff[3] = (byte)(rotationSpeed & 0xFF);
            buff[4] = (byte)((rotationSpeed >> 8) & 0xFF);
            buff[5] = (byte)accRotationType;
            buff[6] = (byte)accMoveType;
            buff[7] = (byte)accPriorityType;
            buff[8] = (byte)(controlTime & 0xFF);

            this.Request(CHARACTERISTIC_MOTOR, buff, false, order, "AccelerationMove",
                targetSpeed, acceleration, rotationSpeed, accRotationType, accMoveType, accPriorityType, controlTime);
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
                this.targetMoveCallback.Notify(this, data[1], (TargetMoveRespondType)data[2]);
            }
            // https://toio.github.io/toio-spec/docs/2.1.0/ble_motor#複数目標指定付きモーター制御の応答
            else if (0x84 == type)
            {
                this.multiTargetMoveCallback.Notify(this, data[1], (TargetMoveRespondType)data[2]);
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