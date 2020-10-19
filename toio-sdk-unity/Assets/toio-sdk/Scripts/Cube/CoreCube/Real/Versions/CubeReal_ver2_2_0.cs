using System.Collections.Generic;

namespace toio
{
    public class CubeReal_ver2_2_0 : CubeReal_ver2_1_0
    {
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      内部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/

        protected CallbackProvider _shakeCallback = new CallbackProvider();

        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        //      外部変数
        //_/_/_/_/_/_/_/_/_/_/_/_/_/
        public override bool isShake { get; protected set; }
        public override string version { get { return "2.2.0"; } }

        // シェイクコールバック
        public override CallbackProvider shakeCallback { get { return this._shakeCallback; } }

        public CubeReal_ver2_2_0(BLEPeripheralInterface peripheral, Dictionary<string, BLECharacteristicInterface> characteristicTable)
        : base(peripheral, characteristicTable)
        {
        }

        protected override void Recv_sensor(byte[] data)
        {
            // https://toio.github.io/toio-spec/docs/ble_sensor
            int type = data[0];
            if (1 == type)
            {
                var _isSloped = data[1] == 0 ? true : false;
                var _isCollisionDetected = data[2] == 1 ? true : false;
                var _isDoubleTap = data[3] == 1 ? true : false;
                PoseType _pose = (PoseType)data[4];
                var _isShake = data[5] == 1 ? true : false;

                if (_isSloped != this.isSloped)
                {
                    this.isSloped = _isSloped;
                    this.slopeCallback.Notify(this);
                }

                if (_isCollisionDetected != this.isCollisionDetected)
                {
                    this.isCollisionDetected = _isCollisionDetected;
                    this.collisionCallback.Notify(this);
                }

                if (_isDoubleTap != this.isDoubleTap)
                {
                    this.isDoubleTap = _isDoubleTap;
                    this.doubleTapCallback.Notify(this);
                }

                if (_pose != this.pose)
                {
                    this.pose = _pose;
                    this.poseCallback.Notify(this);
                }

                if (_isShake != this.isShake)
                {
                    this.isShake = _isShake;
                    this.shakeCallback.Notify(this);
                }
            }
        }




    }
}