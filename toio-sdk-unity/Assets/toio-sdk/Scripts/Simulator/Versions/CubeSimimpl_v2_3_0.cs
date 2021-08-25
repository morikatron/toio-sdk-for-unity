using System;
using UnityEngine;


namespace toio.Simulator
{

    internal class CubeSimImpl_v2_3_0 : CubeSimImpl_v2_2_0
    {
        public  CubeSimImpl_v2_3_0(CubeSimulator cube) : base(cube){}



        // ============ Magnetic Sensor ============
        public override Vector3 magneticForce { get; protected set; }
        protected Cube.MagneticSensorNotificationType magneticSensorNotificationType = Cube.MagneticSensorNotificationType.OnChanged;
        protected int magneticSensorInterval = 1;   // x20ms

        public override void ConfigMagneticSensor(Cube.MagneticSensorMode mode, int interval, Cube.MagneticSensorNotificationType notificationType)
        {
            this.magneticSensorMode = mode;
            this.magneticSensorInterval = Mathf.Clamp(interval, 0, 255);
            this.magneticSensorNotificationType = notificationType;
            this.configMagneticSensorCallback?.Invoke(true);
        }

        private Action<Vector3> magneticForceCallback = null;
        public override void StartNotification_MagneticForce(Action<Vector3> action)
        {
            this.magneticForceCallback = action;
            if (this.magneticSensorMode == Cube.MagneticSensorMode.MagneticForce)
                action?.Invoke(this.magneticForce);
        }

        private float magneticSensorNotificationLastTime = 0;
        public override void RequestMagneticSensor()
        {
            SimulateMagneticSensor();
            this.magnetStateCallback?.Invoke(this.magnetState);
            this.magneticForceCallback?.Invoke(this.magneticForce);
        }

        protected override void _SetMagnetState(Cube.MagnetState state)
        {
            bool isToNotify_Interval = this.magneticSensorInterval > 0
                && Time.time - magneticSensorNotificationLastTime > this.magneticSensorInterval *0.02f;

            bool isToNotify_Type = false;
            if (this.magneticSensorNotificationType == Cube.MagneticSensorNotificationType.Always)
                isToNotify_Type = true;
            else if (this.magneticSensorNotificationType == Cube.MagneticSensorNotificationType.OnChanged)
                isToNotify_Type = state != this.magnetState;

            if (isToNotify_Interval && isToNotify_Type)
            {
                this.magnetStateCallback?.Invoke(state);
                this.magnetState = state;
                this.magneticSensorNotificationLastTime = Time.time;
            }
        }
        protected virtual void _SetMagneticForce(Vector3 force)
        {
            bool isToNotify_Interval = this.magneticSensorInterval > 0
                && Time.time - magneticSensorNotificationLastTime > this.magneticSensorInterval *0.02f;

            bool isToNotify_Type = false;
            if (this.magneticSensorNotificationType == Cube.MagneticSensorNotificationType.Always)
                isToNotify_Type = true;
            else if (this.magneticSensorNotificationType == Cube.MagneticSensorNotificationType.OnChanged)
                isToNotify_Type = force != this.magneticForce;

            if (isToNotify_Interval && isToNotify_Type)
            {
                this.magneticForceCallback?.Invoke(force);
                this.magneticForce = force;
                this.magneticSensorNotificationLastTime = Time.time;
            }
        }

        protected virtual void SimulateMagneticForce(Vector3 force)
        {
            if (this.magneticSensorMode != Cube.MagneticSensorMode.MagneticForce)
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

    }

}
