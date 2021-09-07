using System;
using UnityEngine;


namespace toio.Simulator
{

    internal class CubeSimImpl_v2_3_0 : CubeSimImpl_v2_2_0
    {
        public  CubeSimImpl_v2_3_0(CubeSimulator cube) : base(cube){}



        // ============ Magnetic Sensor ============
        public override Vector3 magneticForce { get; protected set; }
        protected Cube.MagneticNotificationType magneticNotificationType = Cube.MagneticNotificationType.OnChanged;
        protected int magneticNotificationInterval = 1;   // x20ms

        public override void ConfigMagneticSensor(Cube.MagneticMode mode, int intervalMs, Cube.MagneticNotificationType notificationType)
        {
            this.magneticMode = mode;
            this.magneticNotificationInterval = Mathf.Clamp(intervalMs/20, 0, 255);
            this.magneticNotificationType = notificationType;
            this.configMagneticSensorCallback?.Invoke(true);
        }

        private Action<Vector3> magneticForceCallback = null;
        public override void StartNotification_MagneticForce(Action<Vector3> action)
        {
            this.magneticForceCallback = action;
            if (this.magneticMode == Cube.MagneticMode.MagneticForce)
                action?.Invoke(this.magneticForce);
        }

        private float magneticNotificationLastTime = 0;
        public override void RequestMagneticSensor()
        {
            SimulateMagneticSensor();
            if (this.magneticMode == Cube.MagneticMode.MagnetState)
                this.magnetStateCallback?.Invoke(this.magnetState);
            else if (this.magneticMode == Cube.MagneticMode.MagneticForce)
                this.magneticForceCallback?.Invoke(this.magneticForce);
        }

        protected override void _SetMagnetState(Cube.MagnetState state)
        {
            bool isToNotify_Interval = this.magneticNotificationInterval > 0
                && Time.time - magneticNotificationLastTime > this.magneticNotificationInterval *0.02f;

            bool isToNotify_Type = false;
            if (this.magneticNotificationType == Cube.MagneticNotificationType.Always)
                isToNotify_Type = true;
            else if (this.magneticNotificationType == Cube.MagneticNotificationType.OnChanged)
                isToNotify_Type = state != this.magnetState;

            if (isToNotify_Interval && isToNotify_Type)
            {
                this.magnetStateCallback?.Invoke(state);
                this.magnetState = state;
                this.magneticNotificationLastTime = Time.time;
            }
        }
        protected virtual void _SetMagneticForce(Vector3 force)
        {
            bool isToNotify_Interval = this.magneticNotificationInterval > 0
                && Time.time - magneticNotificationLastTime > this.magneticNotificationInterval *0.02f;

            bool isToNotify_Type = false;
            if (this.magneticNotificationType == Cube.MagneticNotificationType.Always)
                isToNotify_Type = true;
            else if (this.magneticNotificationType == Cube.MagneticNotificationType.OnChanged)
                isToNotify_Type = force != this.magneticForce;

            if (isToNotify_Interval && isToNotify_Type)
            {
                this.magneticForceCallback?.Invoke(force);
                this.magneticForce = force;
                this.magneticNotificationLastTime = Time.time;
            }
        }

        protected virtual void SimulateMagneticForce(Vector3 force)
        {
            if (this.magneticMode != Cube.MagneticMode.MagneticForce)
            {
                this.magneticForce = Vector3.zero;
                return;
            }

            force /= 450;
            var orient = force.normalized * 10;
            int ox = Mathf.RoundToInt(orient.x);
            int oy = Mathf.RoundToInt(orient.y);
            int oz = Mathf.RoundToInt(orient.z);
            int mag = Mathf.RoundToInt(force.magnitude);
            Vector3 f = new Vector3(ox, oy, oz);
            f.Normalize();
            f *= mag;
            _SetMagneticForce(f);
        }

        protected override void SimulateMagneticSensor()
        {
            var force = cube._GetMagneticField();
            SimulateMagnetState(force);
            SimulateMagneticForce(force);
        }
        protected override void ResetMagneticSensor()
        {
            base.ResetMagneticSensor();
            this.magneticForce = Vector3.zero;
            this.magneticForceCallback = null;
        }


        // ============ Attitude Sensor ============
        protected Vector3 attitudeEulers;
        protected Quaternion attitudeQuat;
        Cube.AttitudeFormat attitudeFormat = Cube.AttitudeFormat.Eulers;
        Cube.AttitudeNotificationType attitudeNotificationType = Cube.AttitudeNotificationType.OnChanged;
        protected int attitudeNotificationInterval = 0;     // x10ms
        private float attitudeNotificationLastTime = 0;

        private Action<bool> configAttitudeSensorCallback = null;
        public override void StartNotification_ConfigAttitudeSensor(Action<bool> action)
        {
            this.configAttitudeSensorCallback = action;
        }

        public override void ConfigAttitudeSensor(Cube.AttitudeFormat format, int intervalMs, Cube.AttitudeNotificationType notificationType)
        {
            this.attitudeFormat = format;
            this.attitudeNotificationInterval = Mathf.Clamp(intervalMs/10, 0, 255);
            this.attitudeNotificationType = notificationType;
            this.configAttitudeSensorCallback?.Invoke(true);
        }

        private Action<Vector3> attitudeEulersCallback = null;
        private Action<Quaternion> attitudeQuatCallback = null;
        public override void StartNotification_Attitude(Action<Vector3> actionE, Action<Quaternion> actionQ)
        {
            this.attitudeEulersCallback = actionE;
            this.attitudeQuatCallback = actionQ;
            if (attitudeNotificationInterval > 0)
            {
                if (attitudeFormat == Cube.AttitudeFormat.Eulers)
                {
                    this.attitudeEulersCallback?.Invoke(this.attitudeEulers);
                    this.attitudeNotificationLastTime = Time.time;
                }
                if (attitudeFormat == Cube.AttitudeFormat.Quaternion)
                {
                    this.attitudeQuatCallback?.Invoke(this.attitudeQuat);
                    this.attitudeNotificationLastTime = Time.time;
                }
            }
        }

        public override void RequestAttitudeSensor(Cube.AttitudeFormat format)
        {
            SimulateAttitudeSensor();
            if (attitudeNotificationInterval > 0)
            {
                if (format == Cube.AttitudeFormat.Eulers)
                {
                    this.attitudeEulersCallback?.Invoke(this.attitudeEulers);
                    this.attitudeNotificationLastTime = Time.time;
                }
                if (format == Cube.AttitudeFormat.Quaternion)
                {
                    this.attitudeQuatCallback?.Invoke(this.attitudeQuat);
                    this.attitudeNotificationLastTime = Time.time;
                }
            }
        }

        protected virtual void SimulateAttitudeSensor()
        {
            var e = cube._GetIMU();
            int cvt(float f) { return (Mathf.RoundToInt(f) + 180) % 360 - 180; }
            var eulers = new Vector3(cvt(e.x), cvt(e.y), cvt(e.z));

            // NOTE Reproducing real firmware's BUG
            var quat = Quaternion.Euler(0, 0, -e.z) * Quaternion.Euler(0, -e.y, 0) * Quaternion.Euler(e.x+180, 0, 0);
            quat = new Quaternion(Mathf.Floor(quat.x*10000)/10000f, Mathf.Floor(quat.y*10000)/10000f,
                                    Mathf.Floor(quat.z*10000)/10000f, Mathf.Floor(quat.w*10000)/10000f);

            _SetAttitude(eulers, quat);
        }

        protected virtual void _SetAttitude(Vector3 eulers, Quaternion quat)
        {
            bool isToNotify_Interval = this.attitudeNotificationInterval > 0
                && Time.time - this.attitudeNotificationLastTime > this.attitudeNotificationInterval / 100f;

            bool isToNotify_Type = false;
            if (this.attitudeNotificationType == Cube.AttitudeNotificationType.Always)
                isToNotify_Type = true;
            else if (this.attitudeNotificationType == Cube.AttitudeNotificationType.OnChanged)
            {
                if (this.attitudeFormat == Cube.AttitudeFormat.Eulers)
                    isToNotify_Type = eulers != this.attitudeEulers;
                if (this.attitudeFormat == Cube.AttitudeFormat.Quaternion)
                    isToNotify_Type = quat != this.attitudeQuat;
            }

            if (isToNotify_Interval && isToNotify_Type)
            {
                if (this.attitudeFormat == Cube.AttitudeFormat.Eulers)
                    this.attitudeEulersCallback?.Invoke(eulers);
                if (this.attitudeFormat == Cube.AttitudeFormat.Quaternion)
                    this.attitudeQuatCallback?.Invoke(quat);
                this.attitudeEulers = eulers;
                this.attitudeQuat = quat;
                this.attitudeNotificationLastTime = Time.time;
            }
        }

        protected virtual void ResetAttitudeSensor()
        {
            this.attitudeNotificationType = Cube.AttitudeNotificationType.OnChanged;
            this.attitudeNotificationInterval = 0;
            this.attitudeFormat = Cube.AttitudeFormat.Eulers;
            this.configAttitudeSensorCallback = null;

            this.attitudeEulers = Vector3.zero;
            this.attitudeQuat = Quaternion.identity;
            this.attitudeEulersCallback = null;
            this.attitudeQuatCallback = null;
        }


        // ============ Attitude Sensor ============

        public override void Simulate()
        {
            SimulateIDSensor();
            SimulateMotionSensor();
            SimulateMotorSpeedSensor();
            SimulateMagneticSensor();
            SimulateAttitudeSensor();

            float currentTime = Time.fixedTime;
            MotorScheduler(currentTime);
            LightScheduler(currentTime);
            SoundScheduler(currentTime);
            SimulateMotor();
        }

        public override void Reset()
        {
            base.Reset();

            ResetAttitudeSensor();
        }

    }

}
