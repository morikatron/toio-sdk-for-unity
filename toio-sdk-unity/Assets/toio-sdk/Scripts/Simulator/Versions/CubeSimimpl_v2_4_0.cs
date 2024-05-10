using System;
using UnityEngine;


namespace toio.Simulator
{

    internal class CubeSimImpl_v2_4_0 : CubeSimImpl_v2_3_0
    {
        public  CubeSimImpl_v2_4_0(CubeSimulator cube) : base(cube){}


        // ============ Attitude Sensor ============
        public override void ConfigAttitudeSensor(Cube.AttitudeFormat format, int intervalMs, Cube.AttitudeNotificationType notificationType)
        {
            this.attitudeFormat = format;
            this.attitudeNotificationInterval = Mathf.Clamp(intervalMs/10, 0, 255);
            this.attitudeNotificationType = notificationType;
            this.configAttitudeSensorCallback?.Invoke(true);
        }

        public override void StartNotification_Attitude(Action<Vector3> actionE, Action<Quaternion> actionQ)
        {
            this.attitudeEulersCallback = actionE;
            this.attitudeQuatCallback = actionQ;
            if (attitudeNotificationInterval > 0)
            {
                if (attitudeFormat == Cube.AttitudeFormat.Eulers || attitudeFormat == Cube.AttitudeFormat.PreciseEulers)
                    this.attitudeEulersCallback?.Invoke(this.eulers);
                else if (attitudeFormat == Cube.AttitudeFormat.Quaternion)
                    this.attitudeQuatCallback?.Invoke(this.quaternion);
                this.attitudeNotificationLastTime = Time.time;
            }
        }

        public override void RequestAttitudeSensor(Cube.AttitudeFormat format)
        {
            SimulateAttitudeSensor();
            if (attitudeNotificationInterval > 0)
            {
                if (format == Cube.AttitudeFormat.Eulers || format == Cube.AttitudeFormat.PreciseEulers)
                    this.attitudeEulersCallback?.Invoke(this.eulers);
                else if (format == Cube.AttitudeFormat.Quaternion)
                    this.attitudeQuatCallback?.Invoke(this.quaternion);
                this.attitudeNotificationLastTime = Time.time;
            }
        }

        protected override void SimulateAttitudeSensor()
        {
            var e = cube._GetIMU();
            static int cvtInt(float f) { return (Mathf.RoundToInt(f) + 180) % 360 - 180; }
            static float cvt(float f) { return (f + 180) % 360 - 180; }
            Vector3 eulers;
            if (this.attitudeFormat == Cube.AttitudeFormat.Eulers)
                eulers = new Vector3(cvtInt(e.x), cvtInt(e.y), cvtInt(e.z));
            else
                eulers = new Vector3(cvt(e.x), cvt(e.y), cvt(e.z));

            var quat = Quaternion.Euler(0,0,e.z) *  Quaternion.Euler(0,e.y,0) * Quaternion.Euler(e.x,0,0);

            _SetAttitude(eulers, quat);
        }

    }

}
