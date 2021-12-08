using System;
using UnityEngine;


namespace toio.Simulator
{

    internal class CubeSimImpl_v2_2_0 : CubeSimImpl_v2_1_0
    {
        public  CubeSimImpl_v2_2_0(CubeSimulator cube) : base(cube){}


        // ============ ID Sensor ============
        // ------ Configs ------
        protected byte idNotificationInterval = 0;
        protected Cube.IDNotificationType idNotificationType = Cube.IDNotificationType.Balanced;
        protected byte idMissedNotificationSensitivity = 0;

        // ------ Temp Vars ------
        private float idNotificationLastTime = 0;
        private float idMissedNotificationMissTime = 0;
        private bool idMissedNotificationMissMat = false;
        private int x_sent = 0, y_sent = 0, deg_sent = 0;
        private uint standardID_sent = 0;

        // ------ Callbacks ------
        private Action<bool> configIDNotificationCallback = null;
        private Action<bool> configIDMissedNotificationCallback = null;

        public override void StartNotification_ConfigIDNotification(System.Action<bool> action)
        {
            this.configIDNotificationCallback = action;
        }
        public override void StartNotification_ConfigIDMissedNotification(System.Action<bool> action)
        {
            this.configIDMissedNotificationCallback = action;
        }

        public override void ConfigIDNotification(int intervalMs, Cube.IDNotificationType notificationType)
        {
            idNotificationInterval = (byte) Mathf.Clamp(intervalMs/10, 0, 255);
            idNotificationType = notificationType;
            this.configIDNotificationCallback?.Invoke(true);
        }

        public override void ConfigIDMissedNotification(int sensitivityMs)
        {
            idMissedNotificationSensitivity = (byte) Mathf.Clamp(sensitivityMs/10, 0, 255);
            this.configIDMissedNotificationCallback?.Invoke(true);
        }

        protected override void _SetXYDeg(int x, int y, int deg, int xSensor, int ySensor)
        {
            deg = (deg%360+360)%360;
            float now = Time.fixedTime;

            // Decide whether to notify
            bool isToNotify_Interval = now - idNotificationLastTime >= (float)idNotificationInterval/100;

            bool isToNotify_Type = false;
            if (Cube.IDNotificationType.Always == idNotificationType)
                isToNotify_Type = true;
            else if (Cube.IDNotificationType.OnChanged == idNotificationType)
                isToNotify_Type = Mathf.Abs(x-x_sent) >= 2 || Mathf.Abs(y-y_sent) >= 2 || (deg-deg_sent+360)%360 >=3;
            else if (Cube.IDNotificationType.Balanced == idNotificationType)
                isToNotify_Type = x_sent != x || y_sent != y || deg_sent != deg
                    || now - idNotificationLastTime >= 0.3f;
            isToNotify_Type = isToNotify_Type || !this.onMat;

            // Notify ID
            if (isToNotify_Interval && isToNotify_Type)
            {
                this.IDCallback?.Invoke(x, y, deg, xSensor, ySensor);
                idNotificationLastTime = now;
                this.x_sent = x; this.y_sent = y; this.deg_sent = deg;
            }

            // Update realtime vars
            this.x = x; this.y = y; this.deg = deg;
            this.xSensor = xSensor; this.ySensor = ySensor;
            this.onMat = true;
            this.onStandardID = false;
        }

        protected override void _SetStandardID(uint stdID, int deg)
        {
            deg = (deg%360+360)%360;
            float now = Time.fixedTime;

            // Decide whether to notify
            bool isToNotify_Interval = now - idNotificationLastTime >= (float)idNotificationInterval/100;

            bool isToNotify_Type = false;
            if (Cube.IDNotificationType.Always == idNotificationType)
                isToNotify_Type = true;
            else if (Cube.IDNotificationType.OnChanged == idNotificationType)
                isToNotify_Type = this.standardID != stdID || (deg-this.deg+360)%360 >=3;
            else if (Cube.IDNotificationType.Balanced == idNotificationType)
                isToNotify_Type = this.standardID != stdID || this.deg != deg
                    || now - idNotificationLastTime >= 0.3f;
            isToNotify_Type = isToNotify_Type || !this.onStandardID;

            // Notify ID
            if (isToNotify_Interval && isToNotify_Type)
            {
                this.standardIDCallback?.Invoke(stdID, deg);
                idNotificationLastTime = now;
                this.x_sent = x; this.y_sent = y; this.deg_sent = deg;
                this.standardID_sent = stdID;
            }

            // Update realtime vars
            this.standardID = stdID;
            this.deg = deg;
            this.onStandardID = true;
            this.onMat = false;
        }

        protected override void _SetOffGround()
        {
            float now = Time.fixedTime;

            // Missing
            if (this.onMat)
            {
                idMissedNotificationMissTime = now;
                idMissedNotificationMissMat = true;
            }
            else if (this.onStandardID)
            {
                idMissedNotificationMissTime = now;
                idMissedNotificationMissMat = false;
            }

            // Missed for idMissedNotificationSensitivity
            bool isToNotify = (now - idMissedNotificationMissTime >= (float)idMissedNotificationSensitivity/100) && idMissedNotificationMissTime > 0;
            if (isToNotify)
            {
                if (idMissedNotificationMissMat)
                    this.positionIDMissedCallback?.Invoke();
                else
                    this.standardIDMissedCallback?.Invoke();
                idMissedNotificationMissTime = 0;
            }

            // Update realtime vars
            this.onMat = false;
            this.onStandardID = false;
        }



        // ============ Magnetic Sensor ============
        // ------ Configs ------
        protected Cube.MagneticMode magneticMode = Cube.MagneticMode.Off;

        protected Action<bool> configMagneticSensorCallback = null;
        public override void StartNotification_ConfigMagneticSensor(Action<bool> action)
        {
            this.configMagneticSensorCallback = action;
        }
        public override void ConfigMagneticSensor(Cube.MagneticMode mode)
        {
            if (mode > Cube.MagneticMode.MagnetState) return;
            this.magneticMode = mode;
            this.configMagneticSensorCallback?.Invoke(true);
        }

        // ------ Configs ------
        public override void RequestMagneticSensor()
        {
            SimulateMagneticSensor();
            this.magnetStateCallback?.Invoke(this.magnetState);
        }

        // ------ Magnet State ------
        public override Cube.MagnetState magnetState {get; protected set; }
        protected Action<Cube.MagnetState> magnetStateCallback = null;

        public override void StartNotification_MagnetState(Action<Cube.MagnetState> action)
        {
            this.magnetStateCallback = action;
            if (this.magneticMode == Cube.MagneticMode.MagnetState)
                action?.Invoke(this.magnetState);
        }

        protected virtual void _SetMagnetState(Cube.MagnetState state)
        {
            if (state != this.magnetState)
            {
                this.magnetState = state;
                this.magnetStateCallback?.Invoke(state);
            }
        }
        protected virtual void SimulateMagnetState(Vector3 force)
        {
            if (this.magneticMode != Cube.MagneticMode.MagnetState)
            {
                this.magnetState = Cube.MagnetState.None;
                return;
            }

            var e = force.normalized;
            var m = force.magnitude;
            const float orientThreshold = 0.95f;
            Cube.MagnetState state = this.magnetState;

            if (m > 9000 && Vector3.Dot(e, Vector3.forward) > orientThreshold)
                state = Cube.MagnetState.N_Center;
            else if (m > 9000 && Vector3.Dot(e, Vector3.back) > orientThreshold)
                state = Cube.MagnetState.S_Center;
            else if (m > 6000 && Vector3.Dot(e, new Vector3(0, -1, 1).normalized) > orientThreshold)
                state = Cube.MagnetState.N_Right;
            else if (m > 6000 && Vector3.Dot(e, new Vector3(0, 1, 1).normalized) > orientThreshold)
                state = Cube.MagnetState.N_Left;
            else if (m > 6000 && Vector3.Dot(e, new Vector3(0, 1, -1).normalized) > orientThreshold)
                state = Cube.MagnetState.S_Right;
            else if (m > 6000 && Vector3.Dot(e, new Vector3(0, -1, -1).normalized) > orientThreshold)
                state = Cube.MagnetState.S_Left;
            else if (m < 200)
                state = Cube.MagnetState.None;

            _SetMagnetState(state);
        }
        protected virtual void SimulateMagneticSensor()
        {
            var force = cube._GetMagneticField();
            SimulateMagnetState(force);
        }
        protected virtual void ResetMagneticSensor()
        {
            this.magneticMode = Cube.MagneticMode.Off;
            this.magnetState = Cube.MagnetState.None;
            this.magnetStateCallback = null;
            this.configMagneticSensorCallback = null;
        }


        // ============ Motion Sensor ============

        protected override void InvokeMotionSensorCallback()
        {
            if (this.motionSensorCallback == null) return;
            object[] sensors = new object[6];
            sensors[0] = null;
            sensors[1] = (object)this._sloped;
            sensors[2] = (object)this._collisonDetected; this._collisonDetected = false;
            sensors[3] = (object)this._doubleTapped; this._doubleTapped = false;
            sensors[4] = (object)this._pose;
            sensors[5] = (object)this._shakeLevel;
            this.motionSensorCallback.Invoke(sensors);
        }

        // ---------- Shake -----------
        protected int _shakeLevel = 0;
        public override int shakeLevel
        {
            get {return this._shakeLevel;}
            internal set
            {
                if (this._shakeLevel != value){
                    this._shakeLevel = value;
                    this.InvokeMotionSensorCallback();
                }
            }
        }
        protected virtual void SimulateShake()
        {
            // Not Implemented
        }

        // ----------- Simulate -----------
        protected override void SimulateMotionSensor()
        {
            base.SimulateMotionSensor();

            SimulateShake();
        }

        protected override void ResetMotionSensor()
        {
            base.ResetMotionSensor();

            _shakeLevel = 0;
        }

        // ---------- Request Sensor -----------
        public override void RequestMotionSensor()
        {
            this.InvokeMotionSensorCallback();
        }



        // ============ Motor ============

        // ---------- Motor Speed -----------
        protected bool motorSpeedEnabled = false;
        public override int leftMotorSpeed {get; protected set;}
        public override int rightMotorSpeed {get; protected set;}
        protected System.Action<int, int> motorSpeedCallback = null;
        public override void StartNotification_MotorSpeed(System.Action<int, int> action)
        {
            this.motorSpeedCallback = action;
            if (this.motorSpeedEnabled)
                this.motorSpeedCallback?.Invoke(leftMotorSpeed, rightMotorSpeed);
        }

        protected void _SetMotorSpeed(int left, int right)
        {
            left = Mathf.Abs(left);
            right = Mathf.Abs(right);
            if (motorSpeedEnabled)
                if (this.leftMotorSpeed != left || this.rightMotorSpeed != right)
                    this.motorSpeedCallback?.Invoke(left, right);
            this.leftMotorSpeed = left;
            this.rightMotorSpeed = right;
        }

        // ----------- Simulate -----------
        protected void SimulateMotorSpeedSensor()
        {
            int left = Mathf.RoundToInt(cube.speedTireL/CubeSimulator.VMeterOverU);
            int right = Mathf.RoundToInt(cube.speedTireR/CubeSimulator.VMeterOverU);
            _SetMotorSpeed(left, right);
        }

        protected override void ResetMotor()
        {
            base.ResetMotor();

            configMotorReadCallback = null;
            motorSpeedEnabled = false;
            leftMotorSpeed = 0; rightMotorSpeed = 0;
        }

        // ---------- Config -----------
        protected System.Action<bool> configMotorReadCallback = null;
        public override void StartNotification_ConfigMotorRead(System.Action<bool> action)
        {
            this.configMotorReadCallback = action;
        }
        public override void ConfigMotorRead(bool enabled)
        {
            this.motorSpeedEnabled = enabled;
            this.configMotorReadCallback?.Invoke(true);
            if (enabled)
                this.motorSpeedCallback?.Invoke(leftMotorSpeed, rightMotorSpeed);
        }


        // ============ Simulate ============
        public override void Simulate()
        {
            SimulateIDSensor();
            SimulateMotionSensor();
            SimulateMotorSpeedSensor();
            SimulateMagneticSensor();

            float currentTime = Time.fixedTime;
            MotorScheduler(currentTime);
            LightScheduler(currentTime);
            SoundScheduler(currentTime);
            SimulateMotor();
        }

        public override void Reset()
        {
            base.Reset();

            ResetMagneticSensor();
        }

    }
}