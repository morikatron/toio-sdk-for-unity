using System;
using UnityEngine;
using toio.Simulator;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class CubeUnity : Cube
    {
        GameObject gameObject;
        CubeSimulator simulator;
        public string objName { get { return this.simulator.gameObject.name; } }

        public CubeUnity(GameObject gameObject)
        {
            this.gameObject = gameObject;
            id = gameObject.GetInstanceID().ToString();
            simulator = gameObject.GetComponent<CubeSimulator>();

        }
        public bool Init()
        {
            if (isConnected)
            {
                simulator.StartNotification_Button(this.Recv_Button);
                simulator.StartNotification_StandardID(this.Recv_StandardId);
                simulator.StartNotification_PositionID(this.Recv_PositionId);
                simulator.StartNotification_StandardIDMissed(this.Recv_StandardIdMissed);
                simulator.StartNotification_PositionIDMissed(this.Recv_PositionIdMissed);

                simulator.StartNotification_Sloped(this.Recv_Sloped);
                simulator.StartNotification_CollisionDetected(this.Recv_CollisionDetected);

                simulator.StartNotification_DoubleTap(this.Recv_DoubleTap);
                simulator.StartNotification_Pose(this.Recv_Pose);
                simulator.StartNotification_TargetMove(this.Recv_TargetMove);
                simulator.StartNotification_MultiTargetMove(this.Recv_MultiTargetMove);

                simulator.StartNotification_Shake(this.Recv_Shake);
                simulator.StartNotification_MotorSpeed(this.Recv_MotorSpeed);
                simulator.StartNotification_ConfigMotorRead(this.Recv_ConfigMotorRead);

                return true;
            }
            return false;
        }

        /////////////// PROPERTY ///////////////

        public override string version { get {
                if (simulator.version == CubeSimulator.Version.v2_0_0) return "2.0.0";
                else if (simulator.version == CubeSimulator.Version.v2_1_0) return "2.1.0";
                else if (simulator.version == CubeSimulator.Version.v2_2_0) return "2.2.0";
                return "2.2.0";
        } }
        public override string id { get; protected set; }
        public override string addr { get { return id; } }
        public override bool isConnected { get { return simulator.ready; } }
        public override int battery { get { return 100; } protected set { } }

        public override int x { get; protected set; }
        public override int y { get; protected set; }
        public override Vector2 pos { get { return new Vector2(x, y); } }
        public override int angle { get; protected set; }
        public int sensorX { get; protected set; }
        public int sensorY { get; protected set; }
        public override Vector2 sensorPos { get { return new Vector2(sensorX, sensorY); } }
        public override int sensorAngle { get; protected set; }
        public override uint standardId { get; protected set; }
        public override bool isSloped { get; protected set; }
        public override bool isPressed { get; protected set; }
        public override bool isCollisionDetected { get; protected set; }
        public override bool isGrounded { get; protected set; }
        public override int maxSpd { get { return simulator.maxMotor; } }
        public override int deadzone { get { return simulator.deadzone; } }
        // ver2.1.0
        protected bool _isDoubleTap = false;
        public override bool isDoubleTap {
            get {
                if (this.simulator.version>=CubeSimulator.Version.v2_1_0) return _isDoubleTap;
                NotSupportedWarning(); return default;
            }
            protected set { _isDoubleTap = value; } }
        protected PoseType _pose;
        public override PoseType pose {
            get {
                if (this.simulator.version>=CubeSimulator.Version.v2_1_0) return _pose;
                NotSupportedWarning(); return default;
            }
            protected set { _pose = value; } }
        // ver2.2.0
        protected bool _isShake = false;
        public override bool isShake {
            get {
                if (this.simulator.version>=CubeSimulator.Version.v2_2_0) return _isShake;
                NotSupportedWarning(); return default;
            }
            protected set { _isShake = value; } }
        protected bool isEnabledMotorSpeed = false;
        protected bool isCalled_ConfigMotorRead = false;

        protected int _leftSpeed = -1;
        protected int _rightSpeed = -1;
        public override int leftSpeed {
            get {
                if (this.simulator.version < CubeSimulator.Version.v2_2_0) { NotSupportedWarning(); return -1; }
                if (this.isEnabledMotorSpeed) return this._leftSpeed;
                else if (!isCalled_ConfigMotorRead)
                    Debug.Log("モーター速度が有効化されていません. ConfigMotorRead関数を実行して有効化して下さい.");
                return -1;
            }
            protected set { this._leftSpeed = value; }
        }
        public override int rightSpeed {
            get {
                if (this.simulator.version < CubeSimulator.Version.v2_2_0) { NotSupportedWarning(); return -1; }
                if (this.isEnabledMotorSpeed) return this._rightSpeed;
                else if (!isCalled_ConfigMotorRead)
                    Debug.Log("モーター速度が有効化されていません. ConfigMotorRead関数を実行して有効化して下さい.");
                return -1;
            }
            protected set { this._rightSpeed = value; }
        }


        /////////////// Callbacks to user ///////////////
        protected CallbackProvider<Cube> _buttonCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> buttonCallback { get { return this._buttonCallback; } }
        protected CallbackProvider<Cube> _slopeCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> slopeCallback { get { return this._slopeCallback; } }
        protected CallbackProvider<Cube> _collisionCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> collisionCallback { get { return this._collisionCallback; } }
        protected CallbackProvider<Cube> _idCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> idCallback { get { return this._idCallback; } }
        protected CallbackProvider<Cube> _standardIdCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> standardIdCallback { get { return this._standardIdCallback; } }
        protected CallbackProvider<Cube> _idMissedCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> idMissedCallback { get { return this._idMissedCallback; } }
        protected CallbackProvider<Cube> _standardIdMissedCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> standardIdMissedCallback { get { return this._standardIdMissedCallback; } }
        // 2.1.0
        protected CallbackProvider<Cube> _doubleTapCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> doubleTapCallback { get {
            if (simulator.version>=CubeSimulator.Version.v2_1_0) return this._doubleTapCallback;
            else return CallbackProvider<Cube>.NotSupported.Get(this); } }
        protected CallbackProvider<Cube> _poseCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> poseCallback { get {
            if (simulator.version>=CubeSimulator.Version.v2_1_0) return this._poseCallback;
            else return CallbackProvider<Cube>.NotSupported.Get(this); } }
        protected CallbackProvider<Cube, int, TargetMoveRespondType> _targetMoveCallback = new CallbackProvider<Cube, int, TargetMoveRespondType>();
        public override CallbackProvider<Cube, int, TargetMoveRespondType> targetMoveCallback { get {
            if (simulator.version>=CubeSimulator.Version.v2_1_0) return this._targetMoveCallback;
            else return CallbackProvider<Cube, int, TargetMoveRespondType>.NotSupported.Get(this); } }
        protected CallbackProvider<Cube, int, TargetMoveRespondType> _multiTargetMoveCallback = new CallbackProvider<Cube, int, TargetMoveRespondType>();
        public override CallbackProvider<Cube, int, TargetMoveRespondType> multiTargetMoveCallback { get {
            if (simulator.version>=CubeSimulator.Version.v2_1_0) return this._multiTargetMoveCallback;
            else return CallbackProvider<Cube, int, TargetMoveRespondType>.NotSupported.Get(this); } }

        // 2.2.0
        protected CallbackProvider<Cube> _shakeCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> shakeCallback { get {
            if (simulator.version>=CubeSimulator.Version.v2_2_0) return this._shakeCallback;
            else return CallbackProvider<Cube>.NotSupported.Get(this); } }
        protected CallbackProvider<Cube> _motorSpeedCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> motorSpeedCallback { get {
            if (simulator.version>=CubeSimulator.Version.v2_2_0) return this._motorSpeedCallback;
            else return CallbackProvider<Cube>.NotSupported.Get(this); } }


        /////////////// Callbacks from CubeSimulator ///////////////
        private void Recv_Button(bool pressed)
        {
            isPressed = pressed;
            this.buttonCallback.Notify(this);
        }

        private void Recv_StandardId(uint standardId, int deg)
        {
            this.standardId = standardId;
            this.angle = deg;
            this.sensorAngle = deg;
            this.isGrounded = true;
            this.standardIdCallback.Notify(this);
        }

        protected void Recv_PositionId(int x, int y, int deg, int xSensor, int ySensor)
        {
            this.x = x;
            this.y = y;
            this.angle = deg;
            this.sensorX = xSensor;
            this.sensorY = ySensor;
            this.sensorAngle = deg;
            this.isGrounded = true;
            this.idCallback.Notify(this);
        }

        protected void Recv_PositionIdMissed()
        {
            this.isGrounded = false;
            this.idMissedCallback.Notify(this);
        }
        protected void Recv_StandardIdMissed()
        {
            this.isGrounded = false;
            this.standardIdMissedCallback.Notify(this);
        }

        private void Recv_Sloped(bool sloped)
        {
            this.isSloped = sloped;
            this.slopeCallback.Notify(this);
        }

        private void Recv_CollisionDetected(bool collisionDetected)
        {
            this.isCollisionDetected = collisionDetected;
            this.collisionCallback.Notify(this);
        }

        private void Recv_DoubleTap(bool doubleTap)
        {
            this.isDoubleTap = doubleTap;
            this.doubleTapCallback.Notify(this);
        }

        private void Recv_Pose(PoseType posed)
        {
            this.pose = posed;
            this.poseCallback.Notify(this);
        }

        private void Recv_TargetMove(int configID, TargetMoveRespondType response)
        {
            this.targetMoveCallback.Notify(this, configID, response);
        }
        private void Recv_MultiTargetMove(int configID, TargetMoveRespondType response)
        {
            // this.multiTargetMoveCallback.Notify(this, configID, response);
        }

        private void Recv_Shake(bool shake)
        {
            this.isShake = shake;
            this.shakeCallback.Notify(this);
        }

        private void Recv_MotorSpeed(int left, int right)
        {
            this.leftSpeed = left;
            this.rightSpeed = right;
            this.motorSpeedCallback.Notify(this);
        }

        protected void Recv_ConfigMotorRead(bool valid)
        {
            this.isEnabledMotorSpeed = valid;
            this.motorSpeedCallback.Notify(this);
        }


        ///////////////   COMMAND API  ///////////////

        // -------- ver2.0.0 --------
        public override void Move(int left, int right, int durationMs, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
            durationMs = Mathf.Clamp(durationMs/10, 0, 255)*10;
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.Move(left, right, durationMs), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.Move(left, right, durationMs), order, "move", left, right);
#endif
        }

        // Sound
        public override void PlaySound(int repeatCount, SoundOperation[] operations, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
            repeatCount = Mathf.Clamp(repeatCount, 0, 255);
            for (int i=0; i<operations.Length; i++)
            {
                operations[i].durationMs = Mathf.Clamp(operations[i].durationMs/10, 0, 255)*10;
            }

#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.PlaySound(repeatCount, operations), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.PlaySound(repeatCount, operations), order, "playSound", repeatCount);
#endif
        }

        public override void PlaySound(byte[] buff, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
            var repeat = buff[1];
            var length = buff[2];

            int start = 3;
            var data = new SoundOperation[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = new SoundOperation();
                data[i].durationMs = buff[start + i * 3] * 10;
                data[i].note_number = buff[start + i * 3 + 1];
                data[i].volume = buff[start + i * 3 + 2];
            }


#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.PlaySound(repeat, data), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.PlaySound(repeat, data), order, "playSound", repeat);
#endif
        }

        public override void PlayPresetSound(int soundId, int volume = 255, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.PlayPresetSound(soundId, volume), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.PlayPresetSound(soundId, volume), order, "playPresetSound", soundId);
#endif
        }

        public override void StopSound(ORDER_TYPE order = ORDER_TYPE.Weak)
        {
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.StopSound(), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.StopSound(), order, "stopSound");
#endif
        }

        // Light
        public override void TurnLedOff(ORDER_TYPE order = ORDER_TYPE.Weak)
        {
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.StopLight(), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.StopLight(), order, "turnLedOff");
#endif
        }

        public override void TurnLedOn(int red, int green, int blue, int durationMs, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
            durationMs = Mathf.Clamp(durationMs/10, 0, 255)*10;
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.SetLight(red, green, blue, duration), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.SetLight(red, green, blue, durationMs), order, "turnLedOn", red, green, blue, durationMs);
#endif
        }

        public override void TurnOnLightWithScenario(int repeatCount, Cube.LightOperation[] operations, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
            repeatCount = Mathf.Clamp(repeatCount, 0, 255);
            for (int i=0; i<operations.Length; i++)
            {
                operations[i].durationMs = Mathf.Clamp(operations[i].durationMs/10, 0, 255)*10;
            }
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.SetLights(repeatCount, operations), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.SetLights(repeatCount, operations), order, "turnOnLightWithScenario", repeatCount, operations);
#endif
        }

        // Config
        public override void ConfigSlopeThreshold(int angle, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            angle = Mathf.Clamp(angle, 0, 90);
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.SetSlopeThreshold(angle), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.ConfigSlopeThreshold(angle), order, "configSlopeThreshold", angle);
#endif
        }
        public override void ConfigCollisionThreshold(int level, ORDER_TYPE order = ORDER_TYPE.Strong) { NotImplementedWarning(); }

        // -------- ver2.1.0 --------
        public override void ConfigDoubleTapInterval(int interval, ORDER_TYPE order = ORDER_TYPE.Strong) { NotImplementedWarning(); }
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
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.TargetMove(targetX, targetY, targetAngle, configID, timeOut, targetMoveType, maxSpd, targetSpeedType, targetRotationType), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this,
                () => simulator.TargetMove(targetX, targetY, targetAngle, configID, timeOut, targetMoveType, maxSpd, targetSpeedType, targetRotationType),
                order, "targetMove", targetX, targetY, targetAngle, configID, timeOut, targetMoveType, maxSpd, targetSpeedType, targetRotationType);
#endif
        }
        /*
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
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.MultiTargetMove(targetXList, targetYList, targetAngleList, multiRotationTypeList, configID, timeOut, targetMoveType, maxSpd, targetSpeedType, multiWriteType), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this,
                () => simulator.MultiTargetMove(targetXList, targetYList, targetAngleList, multiRotationTypeList,
                    configID, timeOut, targetMoveType, maxSpd, targetSpeedType, multiWriteType),
                order, "multiTargetMove", targetXList, targetYList, targetAngleList, multiRotationTypeList, configID, timeOut, targetMoveType, maxSpd, targetSpeedType, multiWriteType);
#endif
        }
        */
        public override void AccelerationMove(
            int targetSpeed,
            int acceleration,
            int rotationSpeed = 0,
            AccPriorityType accPriorityType = AccPriorityType.Translation,
            int controlTime = 0,
            ORDER_TYPE order = ORDER_TYPE.Strong
        ){
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.AccelerationMove(targetSpeed, acceleration, rotationSpeed, accRotationType, accMoveType, accPriorityType, controlTime), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this,
                () => simulator.AccelerationMove(targetSpeed, acceleration, rotationSpeed, accPriorityType, controlTime),
                order, "accelerationMove", targetSpeed, acceleration, rotationSpeed, accPriorityType, controlTime);
#endif
        }

        // -------- ver2.2.0 --------
        public override async UniTask ConfigMotorRead(bool valid, float timeOutSec, Action<bool, Cube> callback, ORDER_TYPE order)
        {
            isCalled_ConfigMotorRead = true;
            this.simulator.ConfigMotorRead(valid);
            await UniTask.Delay(0);
            callback?.Invoke(true, this);
        }

    }
}