using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class CubeReal_ver2_3_0 : CubeReal_ver2_2_0
    {
        public CubeReal_ver2_3_0(BLEPeripheralInterface peripheral) : base(peripheral) {}


        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      内部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected CallbackProvider<Cube> _magneticForceCallback = new CallbackProvider<Cube>();


        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        public override string version { get { return "2.3.0"; } }
        public override Vector3 magneticForce { get; protected set; }
        public override CallbackProvider<Cube> magneticForceCallback { get => _magneticForceCallback; }


        public override async UniTask ConfigMagneticSensor(MagneticSensorMode mode, int interval, MagneticSensorNotificationType notificationType,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            if (this.magneticSensorRequest == null) this.magneticSensorRequest = new RequestInfo(this);
            if (!this.isConnected || !this.isInitialized)
            {
                callback?.Invoke(false, this); return;
            }
#if !RELEASE
            const float minTimeOut = 0.5f;
            if (minTimeOut > timeOutSec)
            {
                Debug.LogWarningFormat("[CubeReal_ver2_3_0.ConfigMagneticSensor]誤作動を避けるため, タイムアウト時間は {0} 秒以上にして下さい.", minTimeOut);
            }
#endif
            bool availabe = await this.magneticSensorRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!availabe) return;

            this.requestedMagneticSensorMode = mode;

            this.magneticSensorRequest.request = () =>
            {
                byte[] buff = new byte[5];
                buff[0] = 0x1b;
                buff[1] = 0;
                buff[2] = (byte) mode;
                buff[3] = (byte) interval;
                buff[4] = (byte) notificationType;
                this.Request(CHARACTERISTIC_CONFIG, buff, true, order, "ConfigMagneticSensor", mode, interval, notificationType, timeOutSec, callback, order);
            };
            await this.magneticSensorRequest.Run();
        }


        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < recv >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected override void Recv_sensor(byte[] data)
        {
            base.Recv_sensor(data);
            int type = data[0];

            // Magnetic Sensor https://toio.github.io/toio-spec/docs/2.2.0/ble_magnetic_sensor
            if (2 == type)
            {
                var magnitude = (float) data[2];
                float dirX = (sbyte)data[3];
                float dirY = (sbyte)data[4];
                float dirZ = (sbyte)data[5];
                Vector3 force = new Vector3(dirX, dirY, dirZ);
                force.Normalize();
                force *= magnitude;

                if (force != this.magneticForce)
                {
                    this.magneticForce = force;
                    this.magneticForceCallback.Notify(this);
                }
            }
        }
    }

}
