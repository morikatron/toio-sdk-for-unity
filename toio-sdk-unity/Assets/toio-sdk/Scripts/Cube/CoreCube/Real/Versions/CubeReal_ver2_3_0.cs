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
        protected CallbackProvider<Cube> _attitudeCallback = new CallbackProvider<Cube>();
        protected RequestInfo attitudeSensorRequest = null;
        protected AttitudeFormat attitudeFormat = AttitudeFormat.Eulers;
        protected AttitudeFormat requestedAttitudeFormat = AttitudeFormat.Eulers;
        protected Vector3 _magneticForce = default;


        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        public override string version { get { return "2.3.0"; } }
        public override Vector3 magneticForce {
            get {
                if (null == this.magneticSensorRequest)
                {
                    Debug.Log("磁気センサーが有効化されていません. ConfigMagneticSensor関数を実行して有効化して下さい.");
                    return default;
                }
                else if (this.magneticMode != MagneticMode.MagneticForce || !this.magneticSensorRequest.hasReceivedData)
                    return default;
                else
                    return this._magneticForce;
            }
            protected set { this.NotImplementedWarning(); }
        }
        public override CallbackProvider<Cube> magneticForceCallback { get => _magneticForceCallback; }
        public override CallbackProvider<Cube> attitudeCallback { get => _attitudeCallback; }
        public override Vector3 eulers { get; protected set; }
        public override Quaternion quaternion { get; protected set; }


        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        public override async UniTask ConfigMagneticSensor(MagneticMode mode, int intervalMs, MagneticNotificationType notificationType,
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

            this.requestedMagneticMode = mode;

            this.magneticSensorRequest.request = () =>
            {
                byte[] buff = new byte[5];
                buff[0] = 0x1b;
                buff[1] = 0;
                buff[2] = (byte) mode;
                buff[3] = (byte) BitConverter.GetBytes(Mathf.Clamp(intervalMs/20, 0, 255))[0];
                buff[4] = (byte) notificationType;
                this.Request(CHARACTERISTIC_CONFIG, buff, true, order, "ConfigMagneticSensor", mode, intervalMs, notificationType, timeOutSec, callback, order);
            };
            await this.magneticSensorRequest.Run();
        }

        public override async UniTask ConfigAttitudeSensor(AttitudeFormat format, int intervalMs, AttitudeNotificationType notificationType,
            float timeOutSec = 0.5f, Action<bool,Cube> callback = null, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
            if (this.attitudeSensorRequest == null) this.attitudeSensorRequest = new RequestInfo(this);
            if (!this.isConnected || !this.isInitialized)
            {
                callback?.Invoke(false, this); return;
            }
#if !RELEASE
            const float minTimeOut = 0.5f;
            if (minTimeOut > timeOutSec)
            {
                Debug.LogWarningFormat("[CubeReal_ver2_3_0.ConfigAttitudeSensor]誤作動を避けるため, タイムアウト時間は {0} 秒以上にして下さい.", minTimeOut);
            }
#endif
            bool availabe = await this.attitudeSensorRequest.GetAccess(Time.time + timeOutSec, callback);
            if (!availabe) return;

            this.attitudeFormat = format;

            this.attitudeSensorRequest.request = () =>
            {
                byte[] buff = new byte[5];
                buff[0] = 0x1d;
                buff[1] = 0;
                buff[2] = (byte) format;
                buff[3] = (byte) BitConverter.GetBytes(Mathf.Clamp(intervalMs/10, 0, 255))[0];
                buff[4] = (byte) notificationType;
                this.Request(CHARACTERISTIC_CONFIG, buff, true, order, "ConfigAttitudeSensor", format, intervalMs, notificationType, timeOutSec, callback, order);
            };
            await this.attitudeSensorRequest.Run();
        }

        public override void RequestAttitudeSensor(AttitudeFormat format, ORDER_TYPE order)
        {
            if (!this.isConnected) { return; }

            byte[] buff = new byte[1];
            buff[0] = 0x82;

            this.Request(CHARACTERISTIC_SENSOR, buff, true, order, "RequestMagneticSensor");
        }

        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      CoreCube API < recv >
        //_/_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected override void Recv_sensor(byte[] data)
        {
            base.Recv_sensor(data);
            int type = data[0];

            // Magnetic Sensor https://toio.github.io/toio-spec/docs/ble_magnetic_sensor
            if (2 == type)
            {
                var magnitude = (float) data[2];
                float dirX = (sbyte)data[3];
                float dirY = (sbyte)data[4];
                float dirZ = (sbyte)data[5];
                Vector3 force = new Vector3(dirX, dirY, dirZ);
                force.Normalize();
                force *= magnitude;

                this._magneticForce = force;
                if (this.magneticMode == MagneticMode.MagneticForce)
                    this.magneticForceCallback.Notify(this);
            }
            // Attitude Sensor
            else if (3 == type)
            {
                if (this.attitudeSensorRequest != null)
                    this.attitudeSensorRequest.hasReceivedData = true;
                AttitudeFormat format = (AttitudeFormat)data[1];
                if (format != this.attitudeFormat)
                {
                    Debug.LogWarning("Received attitude foramt does not match this.attitudeSensorFormat.");
                    this.attitudeFormat = format;
                }

                if (format == AttitudeFormat.Eulers)
                {
                    int roll = BitConverter.ToInt16(data, 2);
                    int pitch = BitConverter.ToInt16(data, 4);
                    int yaw = BitConverter.ToInt16(data, 6);
                    Vector3 eulers = new Vector3(roll, pitch, yaw);
                    // TODO ファームウェアからのオイラーとクォータニオンの座標系が不一致のため、変換しない
                    // 仕様書により、「回転順序はYaw（ヨー/Z軸）、Pitch（ピッチ/Y軸）、Roll（ロール/X軸）の順です。」 ※仕様書の xyz は unity のzxy に相当する。
                    // Quaternion.Euler の順序が zxy の為、仕様書通りの zyx を組むために掛け算をする。
                    // Quaternion q = Quaternion.Euler(roll,0,0) * Quaternion.Euler(0,pitch,0) * Quaternion.Euler(0,0,yaw);

                    if (eulers != this.eulers)
                    {
                        this.eulers = eulers;
                        this.quaternion = Quaternion.identity;
                        this.attitudeCallback?.Notify(this);
                    }
                }
                else if (format == AttitudeFormat.Quaternion)
                {
                    float w = BitConverter.ToInt16(data, 2) / 10000f;
                    float x = BitConverter.ToInt16(data, 4) / 10000f;
                    float y = BitConverter.ToInt16(data, 6) / 10000f;
                    float z = BitConverter.ToInt16(data, 8) / 10000f;
                    Quaternion q = new Quaternion(x, y, z, w);

                    // TODO ファームウェアからのオイラーとクォータニオンの座標系が不一致のため、変換しない
                    // Quaternion to ZYX-ordered eulers
                    // Vector3 eulers = Vector3.zero;
                    // float sinx_cosy = 2 * (w * x + y * z);
                    // float cosx_cosy = 1 - 2 * (x * x + y * y);
                    // eulers.x = Mathf.Atan2(sinx_cosy, cosx_cosy) * 180/Mathf.PI;

                    // float siny = 2 * (w * y - z * x);
                    // if (Mathf.Abs(siny) >= 1)
                    //     eulers.y = 90 * Mathf.Sign(siny);
                    // else
                    //     eulers.y = Mathf.Asin(siny) * 180/Mathf.PI;

                    // float sinz_cosy = 2 * (w * z + x * y);
                    // float cosz_cosy = 1 - 2 * (y * y + z * z);
                    // eulers.z = Mathf.Atan2(sinz_cosy, cosz_cosy) * 180/Mathf.PI;

                    if (q != this.quaternion)
                    {
                        this.eulers = Vector3.zero;
                        this.quaternion = q;
                        this.attitudeCallback?.Notify(this);
                    }
                }
            }
        }

        protected override void Recv_config(byte[] data)
        {
            base.Recv_config(data);

            // https://toio.github.io/toio-spec/docs/ble_configuration
            int type = data[0];
            if (0x9d == type)       // Attitude
            {
                this.attitudeSensorRequest.hasConfigResponse = true;
                this.attitudeSensorRequest.isConfigResponseSucceeded = (0x00 == data[2]);
                if (this.attitudeSensorRequest.isConfigResponseSucceeded)
                    this.attitudeFormat = this.requestedAttitudeFormat;
            }
        }

    }

}
