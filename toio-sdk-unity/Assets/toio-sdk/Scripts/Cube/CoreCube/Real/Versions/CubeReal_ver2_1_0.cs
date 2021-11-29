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
        // public override CallbackProvider<Cube, int, TargetMoveRespondType> multiTargetMoveCallback { get { return this._multiTargetMoveCallback; } }

        public CubeReal_ver2_1_0(BLEPeripheralInterface peripheral) : base(peripheral)
        {
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < send >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        /// <summary>
        /// キューブから任意の音を再生します
        /// https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生
        /// </summary>
        /// <param name="repeatCount">繰り返し回数</param>
        /// <param name="operations">命令配列</param>
        /// <param name="order">命令の優先度</param>
        public override void PlaySound(int repeatCount, SoundOperation[] operations, ORDER_TYPE order)
        {
#if !RELEASE
            // v2.0.0に限り58以下
            // v2.1.0以降は59以下
            if (59 < operations.Length)
            {
                Debug.LogErrorFormat("[Cube.playSound]最大メロディ数を超えました. operations.Length={0}", operations.Length);
            }
#endif
            if (!this.isConnected) { return; }

            repeatCount = Mathf.Clamp(repeatCount, 0, 255);
            var operation_length = Mathf.Clamp(operations.Length, 0, 58);

            byte[] buff = new byte[3 + operation_length * 3];
            buff[0] = 3;
            buff[1] = BitConverter.GetBytes(repeatCount)[0];
            buff[2] = BitConverter.GetBytes(operation_length)[0];

            for (int i = 0; i < operation_length; i++)
            {
                buff[3 + 3 * i] = BitConverter.GetBytes(Mathf.Clamp(operations[i].durationMs / 10, 1, 255))[0];
                buff[4 + 3 * i] = BitConverter.GetBytes(operations[i].note_number)[0];
                buff[5 + 3 * i] = BitConverter.GetBytes(operations[i].volume)[0];
            }

            this.Request(CHARACTERISTIC_SOUND, buff, true, order, "playSound", repeatCount, operations);
        }

        /// <summary>
        /// キューブから任意の音を再生します
        /// https://toio.github.io/toio-spec/docs/ble_sound#midi-note-number-の再生
        /// </summary>
        /// <param name="buff">命令プロトコル</param>
        /// <param name="order">命令の優先度</param>
        public override void PlaySound(byte[] buff, ORDER_TYPE order)
        {
#if !RELEASE
            // v2.0.0に限り58以下
            // v2.1.0以降は59以下
            if (59 < buff[2])
            {
                Debug.LogErrorFormat("[Cube.playSound]最大メロディ数を超えました. Length={0}", buff[2]);
            }
#endif
            if (!this.isConnected) { return; }

            this.Request(CHARACTERISTIC_SOUND, buff, true, order, "playSound");
        }

        // キューブのモーターを目標指定付き制御します
        public override void TargetMove(
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
        ){
            if (!this.isConnected) { return; }
            #if !RELEASE
                if (65534 < targetX || (targetX < 0 && targetX != -1)){Debug.LogErrorFormat("[Cube.TargetMove]X座標範囲を超えました. targetX={0}", targetX);}
                if (65534 < targetY || (targetY < 0 && targetY != -1)){Debug.LogErrorFormat("[Cube.TargetMove]Y座標範囲を超えました. targetY={0}", targetY);}
                if (8191 < targetAngle || targetAngle < 0){Debug.LogErrorFormat("[Cube.TargetMove]回転角度範囲を超えました. targetAngle={0}", targetAngle);}
                if (255 < configID || configID < 0){Debug.LogErrorFormat("[Cube.TargetMove]制御識別値範囲を超えました. configID={0}", configID);}
                if (255 < timeOut || timeOut < 0){Debug.LogErrorFormat("[Cube.TargetMove]制御時間範囲を超えました. timeOut={0}", timeOut);}
                if (this.maxSpd < maxSpd || maxSpd < 10){Debug.LogErrorFormat("[Cube.TargetMove]速度範囲を超えました. maxSpd={0}", maxSpd);}
            #endif

            targetX = targetX == -1 ? 65535 : Mathf.Clamp(targetX, 0, 65534);
            targetY = targetY == -1 ? 65535 : Mathf.Clamp(targetY, 0, 65534);
            targetAngle = Mathf.Clamp(targetAngle, 0, 8191);
            configID = Mathf.Clamp(configID, 0, 255);
            timeOut= Mathf.Clamp(timeOut, 0, 255);
            maxSpd = Mathf.Clamp(maxSpd, 10, this.maxSpd);

            byte[] buff = new byte[13];
            buff[0] = 3;
            buff[1] = (byte)configID;
            buff[2] = (byte)timeOut;
            buff[3] = (byte)targetMoveType;
            buff[4] = (byte)maxSpd;
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
        /*
        // キューブのモーターを複数目標指定付き制御します
        public override void MultiTargetMove(
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
        ){
            if (!this.isConnected) { return; }
            #if !RELEASE
                if (29 < targetXList.Length){Debug.LogErrorFormat("[Cube.MultiTargetMove]追加目標数29を超えました. targetXList.Length={0}", targetXList.Length);}
                if (255 < configID || configID < 0){Debug.LogErrorFormat("[Cube.MultiTargetMove]制御識別値範囲を超えました. configID={0}", configID);}
                if (255 < timeOut || timeOut < 0){Debug.LogErrorFormat("[Cube.MultiTargetMove]制御時間範囲を超えました. timeOut={0}", timeOut);}
                if (this.maxSpd < maxSpd || maxSpd < 10){Debug.LogErrorFormat("[Cube.MultiTargetMove]速度範囲を超えました. maxSpd={0}", maxSpd);}
            #endif

            multiRotationTypeList = multiRotationTypeList==null? new TargetRotationType[targetXList.Length] : multiRotationTypeList;

            configID = Mathf.Clamp(configID, 0, 255);
            timeOut= Mathf.Clamp(timeOut, 0, 255);
            maxSpd = Mathf.Clamp(maxSpd, 10, this.maxSpd);

            byte[] buff = new byte[targetXList.Length * 6 + 8];
            buff[0] = 4;
            buff[1] = (byte)configID;
            buff[2] = (byte)timeOut;
            buff[3] = (byte)targetMoveType;
            buff[4] = (byte)maxSpd;
            buff[5] = (byte)targetSpeedType;
            buff[6] = 0;
            buff[7] = (byte)multiWriteType;

            for (int i = 0; i < targetXList.Length; i++)
            {
                #if !RELEASE
                    if (28 < i){break;}
                    if (65534 < targetXList[i]){Debug.LogErrorFormat("[Cube.MultiTargetMove]X座標範囲を超えました. targetX={0}", targetXList[i]);}
                    if (65534 < targetYList[i]){Debug.LogErrorFormat("[Cube.MultiTargetMove]Y座標範囲を超えました. targetY={0}", targetYList[i]);}
                    if (8191 < targetAngleList[i]){Debug.LogErrorFormat("[Cube.MultiTargetMove]回転角度範囲を超えました. targetAngle={0}", targetAngleList[i]);}
                #endif
                var targetX = targetXList[i] == -1 ? 65535 : Mathf.Clamp(targetXList[i], 0, 65534);
                var targetY = targetYList[i] == -1 ? 65535 : Mathf.Clamp(targetYList[i], 0, 65534);
                var targetAngle = Mathf.Clamp(targetAngleList[i], 0, 8191);

                buff[i * 6 + 8] = (byte)(targetX & 0xFF);
                buff[i * 6 + 9] = (byte)((targetX >> 8) & 0xFF);
                buff[i * 6 + 10] = (byte)(targetY & 0xFF);
                buff[i * 6 + 11] = (byte)((targetY >> 8) & 0xFF);
                buff[i * 6 + 12] = (byte)(targetAngle & 0xFF);
                buff[i * 6 + 13] = (byte)((((int)multiRotationTypeList[i] & 0x0007) << 5 ) | ((targetAngle & 0x1FFF) >> 8));
            }
            this.Request(CHARACTERISTIC_MOTOR, buff, false, order, "MultiTargetMove",
                targetXList, targetYList, targetAngleList, multiRotationTypeList, configID, timeOut,
                targetMoveType, maxSpd, targetSpeedType, multiWriteType);
        }
        */
        // キューブの加速度指定付きモーターを制御します
        public override void AccelerationMove(
            int targetSpeed,
            int acceleration,
            int rotationSpeed = 0,
            AccPriorityType accPriorityType = AccPriorityType.Translation,
            int controlTime = 0,
            ORDER_TYPE order = ORDER_TYPE.Strong
        ){
            if (!this.isConnected) { return; }
            int accMoveType = targetSpeed > 0 ? 0 : 1;
            int accRotationType = rotationSpeed > 0 ? 0 : 1;

            targetSpeed = Math.Abs(targetSpeed);
            rotationSpeed = Math.Abs(rotationSpeed);

            #if !RELEASE
                if (this.maxSpd < targetSpeed || targetSpeed < deadzone){Debug.LogErrorFormat("[Cube.AccelerationMove]直線速度範囲を超えました. targetSpeed={0}", targetSpeed);}
                if (255 < acceleration || acceleration < 0){Debug.LogErrorFormat("[Cube.AccelerationMove]加速度範囲を超えました. acceleration={0}", acceleration);}
                if (65535 < rotationSpeed){Debug.LogErrorFormat("[Cube.AccelerationMove]回転速度範囲を超えました. rotationSpeed={0}", rotationSpeed);}
                if (255 < controlTime || controlTime < 0){Debug.LogErrorFormat("[Cube.AccelerationMove]制御時間範囲を超えました. controlTime={0}", controlTime);}
            #endif

            targetSpeed = Mathf.Clamp(targetSpeed, deadzone, this.maxSpd);
            acceleration = Mathf.Clamp(acceleration, 0, 255);
            rotationSpeed = Mathf.Clamp(rotationSpeed, 0, 65535);
            controlTime = Mathf.Clamp(controlTime, 0, 255);

            byte[] buff = new byte[9];
            buff[0] = 5;
            buff[1] = (byte)(targetSpeed & 0xFF);
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

            interval = Mathf.Clamp(interval, 0, 7);

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
                // this.multiTargetMoveCallback.Notify(this, data[1], (TargetMoveRespondType)data[2]);
            }
        }

        protected override void Recv_sensor(byte[] data)
        {
            base.Recv_sensor(data);
            int type = data[0];

            // Motion Sensor https://toio.github.io/toio-spec/docs/2.1.0/ble_sensor
            if (1 == type)
            {
                var _isDoubleTap = data[3] == 1 ? true : false;
                PoseType _pose = (PoseType)data[4];

                if (_isDoubleTap != this.isDoubleTap)
                {
                    this.isDoubleTap = _isDoubleTap;
                    if (_isDoubleTap)
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