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

        }

    }

}
