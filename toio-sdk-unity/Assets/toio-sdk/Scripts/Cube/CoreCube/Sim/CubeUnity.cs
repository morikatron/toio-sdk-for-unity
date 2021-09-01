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
        public UnityPeripheral peripheral { get; private set; }

        public CubeUnity(UnityPeripheral peripheral)
        {
            this.peripheral = peripheral;
            this.gameObject = peripheral.obj;

            id = gameObject.GetInstanceID().ToString();
            simulator = gameObject.GetComponent<CubeSimulator>();
        }
        public bool Initialize()
        {
            if (isConnected)
            {
                simulator.StartNotification_Button(this.Recv_Button);
                simulator.StartNotification_StandardID(this.Recv_StandardId);
                simulator.StartNotification_PositionID(this.Recv_PositionId);
                simulator.StartNotification_StandardIDMissed(this.Recv_StandardIdMissed);
                simulator.StartNotification_PositionIDMissed(this.Recv_PositionIdMissed);

                simulator.StartNotification_MotionSensor(this.Recv_MotionSensor);

                simulator.StartNotification_TargetMove(this.Recv_TargetMove);
                simulator.StartNotification_MultiTargetMove(this.Recv_MultiTargetMove);

                simulator.StartNotification_MotorSpeed(this.Recv_MotorSpeed);
                simulator.StartNotification_MagnetState(this.Recv_MagnetState);
                simulator.StartNotification_MagneticForce(this.Recv_MagneticForce);
                simulator.StartNotification_Attitude(this.Recv_AttitudeEulers, this.Recv_AttitudeQuaternion);

                simulator.StartNotification_ConfigMotorRead(this.Recv_ConfigMotorRead);
                simulator.StartNotification_ConfigIDNotification(this.Recv_ConfigIDNotification);
                simulator.StartNotification_ConfigIDMissedNotification(this.Recv_ConfigIDMissedNotification);
                simulator.StartNotification_ConfigMagneticSensor(this.Recv_ConfigMagneticSensor);
                simulator.StartNotification_ConfigAttitudeSensor(this.Recv_ConfigAttitudeSensor);

                return true;
            }
            return false;
        }


        #region ============ Public Properties ============

        public override string version { get {
                if (simulator.version == CubeSimulator.Version.v2_0_0) return "2.0.0";
                else if (simulator.version == CubeSimulator.Version.v2_1_0) return "2.1.0";
                else if (simulator.version == CubeSimulator.Version.v2_2_0) return "2.2.0";
                else if (simulator.version == CubeSimulator.Version.v2_3_0) return "2.3.0";
                return "2.3.0";
        } }
        public override string id { get; protected set; }
        public override string addr { get { return id; } }
        public override bool isConnected { get { return simulator.isConnected; } }
        public override string localName { get { return this.simulator.gameObject.name; } }
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
        protected int _shakeLevel = 0;
        public override int shakeLevel {
            get {
                if (this.simulator.version>=CubeSimulator.Version.v2_2_0) return _shakeLevel;
                NotSupportedWarning(); return default;
            }
            protected set { _shakeLevel = value; } }

        protected int _leftSpeed = -1;
        protected int _rightSpeed = -1;
        public override int leftSpeed {
            get {
                if (this.simulator.version < CubeSimulator.Version.v2_2_0) { NotSupportedWarning(); return -1; }
                if (this.configMotorReadRequest == null)
                {
                    Debug.Log("モーター速度が有効化されていません. ConfigMotorRead関数を実行して有効化して下さい.");
                    return -1;
                }
                if (!this.motorReadValid || !this.configMotorReadRequest.hasReceivedData) return -1;
                return this._leftSpeed;
            }
            protected set { this._leftSpeed = value; }
        }
        public override int rightSpeed {
            get {
                if (this.simulator.version < CubeSimulator.Version.v2_2_0) { NotSupportedWarning(); return -1; }
                if (this.configMotorReadRequest == null)
                {
                    Debug.Log("モーター速度が有効化されていません. ConfigMotorRead関数を実行して有効化して下さい.");
                    return -1;
                }
                if (!this.motorReadValid || !this.configMotorReadRequest.hasReceivedData) return -1;
                return this._rightSpeed;
            }
            protected set { this._rightSpeed = value; }
        }
        protected Cube.MagnetState _magnetState = MagnetState.None;
        public override Cube.MagnetState magnetState {
            get {
                if (this.simulator.version < CubeSimulator.Version.v2_2_0) { NotSupportedWarning(); return MagnetState.None; }
                if (this.configMagneticSensorRequest == null)
                {
                    Debug.Log("磁気センサーが有効化されていません. ConfigMagneticSensor関数を実行して有効化して下さい.");
                    return MagnetState.None;
                }
                if (this.magneticMode!=MagneticMode.MagnetState || !this.configMotorReadRequest.hasReceivedData)
                    return MagnetState.None;
                return this._magnetState;
            }
        }

        // ver2.3.0
        protected Vector3 _magneticForce = Vector3.zero;
        public override Vector3 magneticForce {
            get {
                if (this.simulator.version < CubeSimulator.Version.v2_3_0) { NotSupportedWarning(); return Vector3.zero; }
                if (this.configMagneticSensorRequest == null)
                {
                    Debug.Log("磁気センサーが有効化されていません. ConfigMagneticSensor関数を実行して有効化して下さい.");
                    return Vector3.zero;
                }
                if (this.magneticMode!=MagneticMode.MagneticForce || !this.configMotorReadRequest.hasReceivedData)
                    return Vector3.zero;
                return this._magneticForce;
            }
        }

        protected Vector3 _eulers = Vector3.zero;
        public override Vector3 eulers {
            get {
                if (this.simulator.version < CubeSimulator.Version.v2_3_0) { NotSupportedWarning(); return Vector3.zero; }
                return this._eulers;
            }
        }
        protected Quaternion _quaternion = Quaternion.identity;
        public override Quaternion quaternion {
            get {
                if (this.simulator.version < CubeSimulator.Version.v2_3_0) { NotSupportedWarning(); return Quaternion.identity; }
                return this._quaternion;
            }
        }

        // -------- Callbacks to user --------

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
        // protected CallbackProvider<Cube, int, TargetMoveRespondType> _multiTargetMoveCallback = new CallbackProvider<Cube, int, TargetMoveRespondType>();
        // public override CallbackProvider<Cube, int, TargetMoveRespondType> multiTargetMoveCallback { get {
        //     if (simulator.version>=CubeSimulator.Version.v2_1_0) return this._multiTargetMoveCallback;
        //     else return CallbackProvider<Cube, int, TargetMoveRespondType>.NotSupported.Get(this); } }

        // 2.2.0
        protected CallbackProvider<Cube> _shakeCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> shakeCallback { get {
            if (simulator.version>=CubeSimulator.Version.v2_2_0) return this._shakeCallback;
            else return CallbackProvider<Cube>.NotSupported.Get(this); } }
        protected CallbackProvider<Cube> _motorSpeedCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> motorSpeedCallback { get {
            if (simulator.version>=CubeSimulator.Version.v2_2_0) return this._motorSpeedCallback;
            else return CallbackProvider<Cube>.NotSupported.Get(this); } }
        protected CallbackProvider<Cube> _magnetStateCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> magnetStateCallback { get {
            if (simulator.version>=CubeSimulator.Version.v2_2_0) return this._magnetStateCallback;
            else return CallbackProvider<Cube>.NotSupported.Get(this); } }

        // 2.3.0
        protected CallbackProvider<Cube> _magneticForceCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> magneticForceCallback { get {
            if (simulator.version>=CubeSimulator.Version.v2_3_0) return this._magneticForceCallback;
            else return CallbackProvider<Cube>.NotSupported.Get(this); } }

        protected CallbackProvider<Cube> _attitudeCallback = new CallbackProvider<Cube>();
        public override CallbackProvider<Cube> attitudeCallback { get {
            if (simulator.version>=CubeSimulator.Version.v2_3_0) return this._attitudeCallback;
            else return CallbackProvider<Cube>.NotSupported.Get(this); } }

        #endregion



        #region ============ Internal Vars ============

        protected bool motorReadValid = false;
        protected bool requestedMotorReadValid;
        protected MagneticMode magneticMode = MagneticMode.Off;
        protected MagneticMode requestedMagneticMode;


        // -------- Requests to CubeSimulator --------

        private RequestInfo configMotorReadRequest = null;
        private RequestInfo configIDNotificationRequest = null;
        private RequestInfo configIDMissedNotificationRequest = null;
        private RequestInfo configMagneticSensorRequest = null;
        private RequestInfo configAttitudeSensorRequest = null;

        #endregion



        #region ============ Callbacks from CubeSimulator ============

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

        // ---------- Motion Sensor Receiver ----------
        private void Recv_MotionSensor(object[] sensors)
        {
            // To align with Specification, sensors[0] is set null.
            // https://toio.github.io/toio-spec/docs/ble_sensor#モーションセンサー情報の取得
            for (int i=1; i<sensors.Length; i++)
            {
                if (i==1)       // slopped
                    this.Recv_Sloped( (bool)sensors[i] );
                else if (i==2)  // collision
                    this.Recv_CollisionDetected( (bool)sensors[i] );
                else if (i==3)  // double tap
                    this.Recv_DoubleTap( (bool)sensors[i] );
                else if (i==4)  // pose
                    this.Recv_Pose( (Cube.PoseType)sensors[i] );
                else if (i==5)  // shake
                    this.Recv_Shake( (int)sensors[i] );
            }
        }
        private void Recv_Sloped(bool sloped)
        {
            if (this.isSloped != sloped)
            {
                this.isSloped = sloped;
                this.slopeCallback.Notify(this);
            }
        }
        private void Recv_CollisionDetected(bool collisionDetected)
        {
            if (this.isCollisionDetected != collisionDetected)
            {
                this.isCollisionDetected = collisionDetected;
                if (collisionDetected)
                    this.collisionCallback.Notify(this);
            }
        }
        private void Recv_DoubleTap(bool doubleTap)
        {
            if (this.isDoubleTap != doubleTap)
            {
                this.isDoubleTap = doubleTap;
                if (doubleTap)
                    this.doubleTapCallback.Notify(this);
            }
        }
        private void Recv_Pose(PoseType posed)
        {
            if (this.pose != posed)
            {
                this.pose = posed;
                this.poseCallback.Notify(this);
            }
        }
        private void Recv_Shake(int shakeLevel)
        {
            if (this.shakeLevel != shakeLevel)
            {
                this.shakeLevel = shakeLevel;
                this.shakeCallback.Notify(this);
            }
        }

        // ---------- Motor Response Receiver ----------

        private void Recv_TargetMove(int configID, TargetMoveRespondType response)
        {
            this.targetMoveCallback.Notify(this, configID, response);
        }
        private void Recv_MultiTargetMove(int configID, TargetMoveRespondType response)
        {
            // this.multiTargetMoveCallback.Notify(this, configID, response);
        }

        private void Recv_MotorSpeed(int left, int right)
        {
            this.configMotorReadRequest.hasReceivedData = true;
            this.leftSpeed = left;
            this.rightSpeed = right;
            this.motorSpeedCallback.Notify(this);
        }

        // ---------- Magnetic Receiver ----------

        private void Recv_MagnetState(Cube.MagnetState state)
        {
            this.configMotorReadRequest.hasReceivedData = true;
            this._magnetState = state;
            this.magnetStateCallback.Notify(this);
        }

        private void Recv_MagneticForce(Vector3 force)
        {
            this.configMotorReadRequest.hasReceivedData = true;
            this._magneticForce = force;
            this.magneticForceCallback.Notify(this);
        }

        // ---------- Attitude Receiver ----------

        private void Recv_AttitudeEulers(Vector3 eulers)
        {
            this.configAttitudeSensorRequest.hasReceivedData = true;
            this._eulers = eulers;
            this.attitudeCallback.Notify(this);
        }
        private void Recv_AttitudeQuaternion(Quaternion quat)
        {
            this.configAttitudeSensorRequest.hasReceivedData = true;
            this._quaternion = quat;
            this.attitudeCallback.Notify(this);
        }

        // ---------- Config Response Receiver ----------

        protected void Recv_ConfigMotorRead(bool success)
        {
            this.configMotorReadRequest.hasConfigResponse = true;
            this.configMotorReadRequest.isConfigResponseSucceeded = true;
            this.motorReadValid = this.requestedMotorReadValid;
            this.motorSpeedCallback.Notify(this);
        }

        protected void Recv_ConfigIDNotification(bool success)
        {
            this.configIDNotificationRequest.hasConfigResponse = true;
            this.configIDNotificationRequest.isConfigResponseSucceeded = true;
        }

        protected void Recv_ConfigIDMissedNotification(bool success)
        {
            this.configIDMissedNotificationRequest.hasConfigResponse = true;
            this.configIDMissedNotificationRequest.isConfigResponseSucceeded = true;
        }

        protected void Recv_ConfigMagneticSensor(bool success)
        {
            this.configMagneticSensorRequest.hasConfigResponse = true;
            this.configMagneticSensorRequest.isConfigResponseSucceeded = true;
            this.magneticMode = this.requestedMagneticMode;
        }

        protected void Recv_ConfigAttitudeSensor(bool success)
        {
            this.configAttitudeSensorRequest.hasConfigResponse = true;
            this.configAttitudeSensorRequest.isConfigResponseSucceeded = true;
        }

        #endregion



        #region ============ COMMAND API ============

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
            if (this.configMotorReadRequest == null) this.configMotorReadRequest = new RequestInfo(this);

            bool available = await this.configMotorReadRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!available) return;

            this.requestedMotorReadValid = valid;

            this.configMotorReadRequest.request = () =>
            {
#if RELEASE
                CubeOrderBalancer.Instance.AddOrder(this, () => simulator.ConfigMotorRead(valid), order);
#else
                CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.ConfigMotorRead(valid), order, "ConfigMotorRead", valid);
#endif
            };
            await this.configMotorReadRequest.Run();
        }

        public override async UniTask ConfigIDNotification(int interval, IDNotificationType notificationType = IDNotificationType.Balanced,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            if (this.configIDNotificationRequest == null) this.configIDNotificationRequest = new RequestInfo(this);

            bool available = await this.configIDNotificationRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!available) return;

            this.configIDNotificationRequest.request = () =>
            {
#if RELEASE
                CubeOrderBalancer.Instance.AddOrder(this, () => simulator.ConfigIDNotification(interval, notificationType), order);
#else
                CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.ConfigIDNotification(interval, notificationType), order, "ConfigIDNotification", interval, notificationType);
#endif
            };
            await this.configIDNotificationRequest.Run();
        }

        public override async UniTask ConfigIDMissedNotification(int sensitivity,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            if (this.configIDMissedNotificationRequest == null) this.configIDMissedNotificationRequest = new RequestInfo(this);

            bool available = await this.configIDMissedNotificationRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!available) return;

            this.configIDMissedNotificationRequest.request = () =>
            {
#if RELEASE
                CubeOrderBalancer.Instance.AddOrder(this, () => simulator.ConfigIDMissedNotification(sensitivity), order);
#else
                CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.ConfigIDMissedNotification(sensitivity), order, "ConfigIDMissedNotification", sensitivity);
#endif
            };

            await this.configIDMissedNotificationRequest.Run();
        }

        public override async UniTask ConfigMagneticSensor(MagneticMode mode, float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            if (this.configMagneticSensorRequest == null) this.configMagneticSensorRequest = new RequestInfo(this);

            bool available = await this.configMagneticSensorRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!available) return;

            this.requestedMagneticMode = mode;

            this.configMagneticSensorRequest.request = () =>
            {
#if RELEASE
                CubeOrderBalancer.Instance.AddOrder(this, () => simulator.ConfigMagneticSensor(mode), order);
#else
                CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.ConfigMagneticSensor(mode), order, "ConfigMagneticSensor", mode);
#endif
            };

            await this.configMagneticSensorRequest.Run();
        }

        public override void RequestMotionSensor(ORDER_TYPE order = ORDER_TYPE.Strong)
        {
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.RequestMotionSensor(), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.RequestMotionSensor(), order, "RequestMotionSensor");
#endif
        }

        public override void RequestMagneticSensor(ORDER_TYPE order = ORDER_TYPE.Strong)
        {
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.RequestMagneticSensor(), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.RequestMagneticSensor(), order, "RequestMagneticSensor");
#endif
        }

        // -------- ver2.3.0 --------
        public override async UniTask ConfigMagneticSensor(MagneticMode mode, int interval, MagneticNotificationType notificationType,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            if (this.configMagneticSensorRequest == null) this.configMagneticSensorRequest = new RequestInfo(this);

            bool available = await this.configMagneticSensorRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!available) return;

            this.requestedMagneticMode = mode;

            this.configMagneticSensorRequest.request = () =>
            {
#if RELEASE
                CubeOrderBalancer.Instance.AddOrder(this, () => simulator.ConfigMagneticSensor(mode, interval, notificationType), order);
#else
                CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.ConfigMagneticSensor(mode, interval, notificationType), order, "ConfigMagneticSensor", mode, interval, notificationType);
#endif
            };

            await this.configMagneticSensorRequest.Run();
        }

        public override async UniTask ConfigAttitudeSensor(AttitudeFormat format, int interval, AttitudeNotificationType notificationType,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            if (this.configAttitudeSensorRequest == null) this.configAttitudeSensorRequest = new RequestInfo(this);

            bool available = await this.configAttitudeSensorRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!available) return;

            this.configAttitudeSensorRequest.request = () =>
            {
#if RELEASE
                CubeOrderBalancer.Instance.AddOrder(this, () => simulator.ConfigAttitudeSensor(format, interval, notificationType), order);
#else
                CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.ConfigAttitudeSensor(format, interval, notificationType), order, "ConfigAttitudeSensor", format, interval, notificationType);
#endif
            };

            await this.configAttitudeSensorRequest.Run();
        }

        public override void RequestAttitudeSensor(AttitudeFormat format, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.RequestAttitudeSensor(format), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.RequestAttitudeSensor(format), order, "RequestAttitudeSensor", format);
#endif
        }


        #endregion


    }
}