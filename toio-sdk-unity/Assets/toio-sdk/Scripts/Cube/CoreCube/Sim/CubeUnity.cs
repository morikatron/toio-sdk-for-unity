using UnityEngine;
using toio.Simulator;

namespace toio
{
    public class CubeUnity : Cube
    {
        GameObject gameObject;
        CubeSimulator simulator;

        // コールバック
        CallbackProvider _buttonCallback = new CallbackProvider();
        CallbackProvider _slopeCallback = new CallbackProvider();
        CallbackProvider _collisionCallback = new CallbackProvider();
        CallbackProvider _idCallback = new CallbackProvider();
        CallbackProvider _standardIdCallback = new CallbackProvider();
        CallbackProvider _idMissedCallback = new CallbackProvider();
        CallbackProvider _standardIdMissedCallback = new CallbackProvider();
        CallbackProvider _doubleTapCallback = new CallbackProvider();
        CallbackProvider _poseCallback = new CallbackProvider();

        public CubeUnity(GameObject gameObject)
        {
            this.gameObject = gameObject;
            id = gameObject.GetInstanceID().ToString();
            simulator = gameObject.GetComponent<CubeSimulator>();
        }
        public bool Init()
        {
			if (isConnected)
			{
				simulator.StartNotification_Button(this.Recv_Button);
				simulator.StartNotification_StandardID(this.Recv_StandardId);
				simulator.StartNotification_PositionID(this.Recv_PositionId);
				simulator.StartNotification_StandardIDMissed(this.Recv_StandardIdMissed);
				simulator.StartNotification_PositionIDMissed(this.Recv_PositionIdMissed);

				simulator.StartNotification_Sloped(this.Recv_Sloped);
				simulator.StartNotification_CollisionDetected(this.Recv_CollisionDetected);
				return true;
			}
			return false;
        }

        public override string id { get; protected set; }
        public override int battery { get { return 100; } protected set { } }
        public override string version { get {
                if (simulator.version == CubeSimulator.Version.v2_0_0) return "2.0.0";
                return "2.0.0";
        } }
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

        // コールバック
        public override CallbackProvider buttonCallback { get { return this._buttonCallback; } }
        public override CallbackProvider slopeCallback { get { return this._slopeCallback; } }
        public override CallbackProvider collisionCallback { get { return this._collisionCallback; } }
        public override CallbackProvider idCallback { get { return this._idCallback; } }
        public override CallbackProvider standardIdCallback { get { return this._standardIdCallback; } }
        public override CallbackProvider idMissedCallback { get { return this._idMissedCallback; } }
        public override CallbackProvider standardIdMissedCallback { get { return this._standardIdMissedCallback; } }

        ///////////////   RETRIEVE INFO   ////////////

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

        private void Recv_Sloped(bool sloped)// コアキューブの傾き状態
        {
                this.isSloped = sloped;
                this.slopeCallback.Notify(this);
        }

        private void Recv_CollisionDetected(bool collisionDetected)// コアキューブの衝突状態
        {
                this.isCollisionDetected = collisionDetected;
                this.collisionCallback.Notify(this);
        }

        ///////////////   COMMAND API  ///////////////

        public override void Move(int left, int right, int durationMs, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
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
                data[i].durationMs = (short)(buff[start + i * 3] * 10);
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
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.SetLight(red, green, blue, duration), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.SetLight(red, green, blue, durationMs), order, "turnLedOn", red, green, blue, durationMs);
#endif
        }

        public override void TurnOnLightWithScenario(int repeatCount, Cube.LightOperation[] operations, ORDER_TYPE order = ORDER_TYPE.Weak)
        {
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.SetLights(repeatCount, operations), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.SetLights(repeatCount, operations), order, "turnOnLightWithScenario", repeatCount, operations);
#endif
        }
        public override void ConfigSlopeThreshold(int angle, ORDER_TYPE order = ORDER_TYPE.Strong)
        {
#if RELEASE
            CubeOrderBalancer.Instance.AddOrder(this, () => simulator.SetSlopeThreshold(angle), order);
#else
            CubeOrderBalancer.Instance.DEBUG_AddOrderParams(this, () => simulator.SetSlopeThreshold(angle), order, "configSlopeThreshold", angle);
#endif
        }
        public override void ConfigCollisionThreshold(int level, ORDER_TYPE order = ORDER_TYPE.Strong) { }

        //  no use
        public override string addr { get { return id; } }
        public override bool isConnected { get { return simulator.ready; } }

        public string objName { get { return this.simulator.gameObject.name; } }
    }
}