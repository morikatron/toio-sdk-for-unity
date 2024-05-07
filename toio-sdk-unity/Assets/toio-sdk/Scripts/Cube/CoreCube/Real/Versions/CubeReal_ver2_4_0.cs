using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class CubeReal_ver2_4_0 : CubeReal_ver2_3_0
    {
        public CubeReal_ver2_4_0(BLEPeripheralInterface peripheral) : base(peripheral) {}


        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        public override string version { get { return "2.4.0"; } }


        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        public override async UniTask ConfigAttitudeSensor(AttitudeFormat format, int intervalMs, AttitudeNotificationType notificationType,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            await this._ConfigAttitudeSensor(format, intervalMs, notificationType, timeOutSec, callback, order);
        }

        public override void RequestAttitudeSensor(AttitudeFormat format, ORDER_TYPE order)
        {
            if (!this.isConnected) { return; }

            byte[] buff = new byte[2];
            buff[0] = 0x83;
            buff[1] = (byte) format;

            this.Request(CHARACTERISTIC_SENSOR, buff, true, order, "RequestMagneticSensor");
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < recv >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected override void Recv_sensor(byte[] data)
        {
            base.Recv_sensor(data);
            int type = data[0];

            if (3 == type)
            {
                if (this.attitudeSensorRequest != null)
                    this.attitudeSensorRequest.hasReceivedData = true;
                AttitudeFormat format = (AttitudeFormat)data[1];

                if (format == AttitudeFormat.Quaternion)
                {
                    float w = BitConverter.ToSingle(data, 2);
                    float x = BitConverter.ToSingle(data, 6);
                    float y = BitConverter.ToSingle(data, 10);
                    float z = BitConverter.ToSingle(data, 14);
                    Quaternion q = new Quaternion(x, y, z, w);

                    // Quaternion to ZYX-ordered eulers
                    Vector3 eulers = Vector3.zero;
                    float sinx_cosy = 2 * (w * x + y * z);
                    float cosx_cosy = 1 - 2 * (x * x + y * y);
                    eulers.x = Mathf.Atan2(sinx_cosy, cosx_cosy) * 180/Mathf.PI;
                    float siny = 2 * (w * y - z * x);
                    if (Mathf.Abs(siny) >= 1)
                        eulers.y = 90 * Mathf.Sign(siny);
                    else
                        eulers.y = Mathf.Asin(siny) * 180/Mathf.PI;
                    float sinz_cosy = 2 * (w * z + x * y);
                    float cosz_cosy = 1 - 2 * (y * y + z * z);
                    eulers.z = Mathf.Atan2(sinz_cosy, cosz_cosy) * 180/Mathf.PI;

                    if (q != this.quaternion)
                    {
                        this.eulers = Vector3.zero;
                        this.quaternion = q;
                        this.attitudeCallback?.Notify(this);
                    }
                }
                else if (format == AttitudeFormat.PreciseEulers)
                {
                    float roll = BitConverter.ToSingle(data, 2);
                    float pitch = BitConverter.ToSingle(data, 6);
                    float yaw = BitConverter.ToSingle(data, 10);
                    Vector3 eulers = new Vector3(roll, pitch, yaw);

                    if (eulers != this.eulers)
                    {
                        this.eulers = eulers;
                        // 仕様書により、「回転順序はYaw（ヨー/Z軸）、Pitch（ピッチ/Y軸）、Roll（ロール/X軸）の順です。」
                        this.quaternion = Quaternion.Euler(0,0,yaw) *  Quaternion.Euler(0,pitch,0) * Quaternion.Euler(roll,0,0);
                        this.attitudeCallback?.Notify(this);
                    }
                }
            }
        }
    }
}

