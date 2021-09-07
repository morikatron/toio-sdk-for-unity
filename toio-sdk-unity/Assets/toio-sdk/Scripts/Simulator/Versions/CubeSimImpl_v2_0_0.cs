using System;
using System.Linq;
using UnityEngine;


namespace toio.Simulator
{
    internal class CubeSimImpl_v2_0_0 : CubeSimImpl
    {
        public override int maxMotor{get;} = 100;
        public override int deadzone{get;} = 10;
        public  CubeSimImpl_v2_0_0(CubeSimulator cube) : base(cube){}


        // ============ Simulate ============
        public override void Simulate()
        {
            SimulateIDSensor();
            SimulateMotionSensor();

            float currentTime = Time.fixedTime;
            MotorScheduler(currentTime);
            LightScheduler(currentTime);
            SoundScheduler(currentTime);
            SimulateMotor();
        }

        public override void Reset()
        {
            ResetMotor();

            ResetIDSensor();
            ResetButton();
            ResetMotionSensor();
            ResetLight();
            ResetSound();
        }

        #region ============ toio ID ============
        protected System.Action<uint, int> standardIDCallback = null;
        public override void StartNotification_StandardID(System.Action<uint, int> action)
        {
            this.standardIDCallback = action;
            this.standardIDCallback.Invoke(this.standardID, deg);
        }
        protected System.Action standardIDMissedCallback = null;
        public override void StartNotification_StandardIDMissed(System.Action action)
        {
            this.standardIDMissedCallback= action;
        }
        protected System.Action<int, int, int, int, int> IDCallback = null;
        public override void StartNotification_PositionID(System.Action<int, int, int, int, int> action)
        {
            this.IDCallback = action;
            this.IDCallback.Invoke(x, y, deg, xSensor, ySensor);
        }
        protected System.Action positionIDMissedCallback = null;
        public override void StartNotification_PositionIDMissed(System.Action action)
        {
            this.positionIDMissedCallback= action;
        }
        protected virtual void _SetXYDeg(int x, int y, int deg, int xSensor, int ySensor)
        {
            deg = (deg%360+360)%360;
            if (this.x != x || this.y != y || this.deg != deg || !this.onMat)
                this.IDCallback?.Invoke(x, y, deg, xSensor, ySensor);
            this.x = x; this.y = y; this.deg = deg;
            this.xSensor = xSensor; this.ySensor = ySensor;
            this.onMat = true;
            this.onStandardID = false;
        }
        protected virtual void _SetStandardID(uint stdID, int deg)
        {
            deg = (deg%360+360)%360;
            if (this.standardID != stdID || this.deg != deg || !this.onStandardID)
                this.standardIDCallback?.Invoke(stdID, deg);
            this.standardID = stdID;
            this.deg = deg;
            this.onStandardID = true;
            this.onMat = false;
        }
        protected virtual void _SetOffGround()
        {
            if (this.onMat)
                this.positionIDMissedCallback?.Invoke();
            if (this.onStandardID)
                this.standardIDMissedCallback?.Invoke();
            this.onMat = false;
            this.onStandardID = false;
        }

        /// <summary>
        /// Simulate reading toio ID
        /// </summary>
        protected virtual void SimulateIDSensor()
        {
            (var coordSensor, var stdID, var deg) = cube._GetIDSensor();
            if (coordSensor != Vector2.zero)
            {
                // Calc. Cube position from Sensor position
                const float dx = 0.0149f * Mat.DotPerM;
                const float dy = 0.0094f * Mat.DotPerM;
                var rad = deg * Mathf.PI/180;
                var dx2 = dx * Mathf.Cos(rad) - dy * Mathf.Sin(rad);
                var dy2 = dx * Mathf.Sin(rad) + dy * Mathf.Cos(rad);
                var cx = coordSensor.x + dx2;
                var cy = coordSensor.y + dy2;
                _SetXYDeg(
                    Mathf.RoundToInt(cx),
                    Mathf.RoundToInt(cy),
                    Mathf.RoundToInt(deg),
                    Mathf.RoundToInt(coordSensor.x),
                    Mathf.RoundToInt(coordSensor.y)
                );
            }
            else if (stdID != 0)
                _SetStandardID(stdID, Mathf.RoundToInt(deg));
            else
                _SetOffGround();
        }
        protected virtual void ResetIDSensor()
        {
            standardIDCallback = null;
            standardIDMissedCallback = null;
            IDCallback = null;
            positionIDMissedCallback = null;

            x = 0; y = 0; deg = 0;
            xSensor = 0; ySensor = 0;
            standardID = 0;
            onMat = false; onStandardID = false;
        }

        #endregion


        #region ============ Button ============
        protected bool _button;
        public override bool button
        {
            get {return this._button;}
            internal set
            {
                if (this._button!=value){
                    this.buttonCallback?.Invoke(value);
                }
                this._button = value;
                cube._SetPressed(value);
            }
        }
        protected System.Action<bool> buttonCallback = null;
        public override void StartNotification_Button(System.Action<bool> action)
        {
            this.buttonCallback = action;
        }
        protected virtual void ResetButton()
        {
            buttonCallback = null;

            _button = false;
        }

        #endregion


        #region ============ Motion Sensor ============
        protected System.Action<object[]> motionSensorCallback = null;
        public override void StartNotification_MotionSensor(System.Action<object[]> action)
        {
            this.motionSensorCallback = action;
        }
        protected virtual void InvokeMotionSensorCallback()
        {
            if (this.motionSensorCallback == null) return;
            object[] sensors = new object[3];
            sensors[0] = null;
            sensors[1] = (object)this._sloped;
            sensors[2] = (object)this._collisonDetected; this._collisonDetected = false;
            this.motionSensorCallback.Invoke(sensors);
        }
        protected virtual void ResetMotionSensor()
        {
            motionSensorCallback = null;

            _sloped = false;
            slopeThreshold = 45;
            _collisonDetected = false;
        }

        // ----------- Sloped -----------
        protected int slopeThreshold = 45;
        protected bool _sloped;
        public override bool sloped
        {
            get {return this._sloped;}
            internal set
            {
                if (this._sloped!=value){
                    this._sloped = value;
                    this.InvokeMotionSensorCallback();
                }
            }
        }
        // ----------- Collision Detected -----------
        protected bool _collisonDetected = false;
        internal override void TriggerCollision()
        {
            this._collisonDetected = true;
            this.InvokeMotionSensorCallback();
            this._collisonDetected = false;
        }

        // ----------- Simulate -----------
        protected virtual void SimulateMotionSensor()
        {
            // 水平検出
            if (cube.isSimulateSloped)
            {
                var slopedAngle = Vector3.Angle(Vector3.up, cube.transform.up);
                cube.sloped = slopedAngle > slopeThreshold && slopedAngle < 180-slopeThreshold;
            }

            // 衝突検出
            // 未実装
        }

        #endregion


        #region ============ Motor ============
        protected struct MotorTimeCmd
        {
            public int left, right, duration;
            public float tRecv;
        }
        protected MotorTimeCmd motorTimeCmd = default;  // current command

        protected virtual void MotorScheduler(float t)
        {
            // Excute MotorTimeCmd
            if (motorCmdType == MotorCmdType.MotorTimeCmd)
            {
                var elipsed = t - motorTimeCmd.tRecv;
                if (motorTimeCmd.duration==0 || elipsed < motorTimeCmd.duration/1000f)
                {
                    motorCmdL = motorTimeCmd.left;
                    motorCmdR = motorTimeCmd.right;
                }
                else
                    motorCmdType = MotorCmdType.None;
            }

            // Order Over
            if (motorCmdType == MotorCmdType.None)
            {
                motorCmdL = 0; motorCmdR = 0;
            }
        }
        protected override void ResetMotor()
        {
            base.ResetMotor();

            motorCmdType = MotorCmdType.None;
            motorTimeCmd = default;
        }

        #endregion


        #region ============ Light ============
        protected struct LightCmd
        {
            public byte r, g, b;
            public short duration;    // ms
            public float tRecv;
        }
        protected LightCmd lightCmd = default;

        protected struct LightSenarioCmd
        {
            public byte repeat;
            public Cube.LightOperation[] lights;
            public float tRecv;
            public float period;    // s
        }
        protected LightSenarioCmd lightSenarioCmd = default;

        protected void LightScheduler(float t)
        {
            // ----- Excute LightCmd -----
            if (lightCmdType == LightCmdType.LightCmd)
            {
                float elipsed = t - lightCmd.tRecv;
                if (lightCmd.duration==0 || elipsed < lightCmd.duration/1000f)
                    cube._SetLight(lightCmd.r, lightCmd.g, lightCmd.b);
                else
                    lightCmdType = LightCmdType.None;
            }
            // ----- Excute LightSenarioCmd -----
            else if (lightCmdType == LightCmdType.LightSenarioCmd)
            {
                float elipsed = t - lightSenarioCmd.tRecv;
                if (lightSenarioCmd.period==0 || lightSenarioCmd.repeat>0
                    && lightSenarioCmd.period*lightSenarioCmd.repeat <= elipsed
                ){
                    lightCmdType = LightCmdType.None;
                }
                else
                {
                    // Index of current operation
                    float sum = 0; int index=0; var lights = lightSenarioCmd.lights;
                    for (int i=0; i<lights.Length; ++i){
                        sum += lights[i].durationMs/1000f;
                        if (elipsed % lightSenarioCmd.period < sum){
                            index = i;
                            break;
                        }
                    }
                    cube._SetLight(lights[index].red, lights[index].green, lights[index].blue);
                }
            }

            // Order Over
            if (lightCmdType == LightCmdType.None)
                cube._StopLight();
        }

        protected virtual void ResetLight()
        {
            lightCmd = default;
            lightSenarioCmd = default;
            lightCmdType = LightCmdType.None;
        }

        #endregion


        #region ============ Sound ============
        protected struct SoundSenarioCmd
        {
            public byte repeat;
            public Cube.SoundOperation[] sounds;
            public float tRecv;
            public float period;
        }
        protected SoundSenarioCmd soundSenarioCmd = default;

        protected void SoundScheduler(float t)
        {
            // ----- Excute Order -----
            if (soundCmdType == SoundCmdType.SoundSenarioCmd)
            {
                var elipsed = t - soundSenarioCmd.tRecv;
                if (soundSenarioCmd.period==0 || soundSenarioCmd.repeat>0
                    && soundSenarioCmd.period*soundSenarioCmd.repeat <= elipsed
                )
                    soundCmdType = SoundCmdType.None;
                else
                {
                    // Index of current operation
                    float sum = 0; int index=0; var sounds = soundSenarioCmd.sounds;
                    for (int i=0; i<sounds.Length; ++i){
                        sum += sounds[i].durationMs/1000f;
                        if (elipsed%soundSenarioCmd.period < sum){
                            index = i;
                            break;
                        }
                    }
                    cube._PlaySound(sounds[index].note_number, sounds[index].volume);
                }
            }

            // Order Over
            if (soundCmdType == SoundCmdType.None)
                cube._StopSound();
        }

        protected virtual void ResetSound()
        {
            soundCmdType = SoundCmdType.None;
            soundSenarioCmd = default;
        }

        #endregion


        #region ============ Commands ============
        public override void Move(int left, int right, int durationMS)
        {
            MotorTimeCmd cmd = new MotorTimeCmd();
            cmd.left = Mathf.Clamp(left, -maxMotor, maxMotor);
            cmd.right = Mathf.Clamp(right, -maxMotor, maxMotor);
            if (Mathf.Abs(cmd.left) < this.deadzone) cmd.left = 0;
            if (Mathf.Abs(cmd.right) < this.deadzone) cmd.right = 0;
            cmd.duration = Mathf.Clamp(durationMS, 0, 2550);
            cmd.tRecv = Time.time;

            motorTimeCmd = cmd;
            motorCmdType = MotorCmdType.MotorTimeCmd;
        }
        public override void StopLight()
        {
            lightCmdType = LightCmdType.None;
        }
        public override void SetLight(int r, int g, int b, int durationMS)
        {
            LightCmd cmd = new LightCmd();
            cmd.r = (byte)r; cmd.g = (byte)g; cmd.b = (byte)b;
            cmd.duration = (short)durationMS;
            cmd.tRecv = Time.time;

            lightCmd = cmd;
            lightCmdType = LightCmdType.LightCmd;
        }
        public override void SetLights(int repeatCount, Cube.LightOperation[] operations)
        {
            if (operations.Length == 0) return;
            operations = operations.Take(29).ToArray();

            LightSenarioCmd cmd = new LightSenarioCmd();
            cmd.lights = operations;
            cmd.repeat = (byte)Mathf.Clamp(repeatCount, 0, 255);
            // calc. period
            cmd.period = 0;
            for (int i=0; i<cmd.lights.Length; ++i){
                cmd.period += cmd.lights[i].durationMs/1000f;
            }
            cmd.tRecv = Time.time;

            lightSenarioCmd = cmd;
            lightCmdType = LightCmdType.LightSenarioCmd;
        }
        public override void PlaySound(int repeatCount, Cube.SoundOperation[] operations)
        {
            if (operations.Length == 0) return;
            operations = operations.Take(59).ToArray();

            SoundSenarioCmd cmd = new SoundSenarioCmd();
            cmd.sounds = operations;
            cmd.repeat = (byte)Mathf.Clamp(repeatCount, 0, 255);
            // calc. period
            cmd.period = 0;
            for (int i=0; i<cmd.sounds.Length; ++i){
                cmd.period += cmd.sounds[i].durationMs/1000f;
            }
            cmd.tRecv = Time.time;

            soundSenarioCmd = cmd;
            soundCmdType = SoundCmdType.SoundSenarioCmd;
        }
        public override void PlayPresetSound(int soundId, int volume)
        {
            soundId = BitConverter.GetBytes(soundId)[0];
            if (this.presetSounds.Count == 0) return;
            if (soundId >= this.presetSounds.Count) soundId = 0;
            PlaySound(1, this.presetSounds[soundId]);
        }
        public override void StopSound()
        {
            soundCmdType = SoundCmdType.None;
        }

        public override void ConfigSlopeThreshold(int degree)
        {
            slopeThreshold = degree;
        }
        #endregion


        internal override void PlaySound_Connect()
        {
            Cube.SoundOperation[] ops = new Cube.SoundOperation[5];
            int dur = 120;
            ops[0] = new Cube.SoundOperation(dur, 255, 59);
            ops[1] = new Cube.SoundOperation(dur, 255, 66);
            ops[2] = new Cube.SoundOperation(dur, 255, 59);
            ops[3] = new Cube.SoundOperation(dur, 255, 66);
            ops[4] = new Cube.SoundOperation(dur*3, 255, 70);
            PlaySound(1, ops);
        }
        internal override void PlaySound_Disconnect()
        {
            Cube.SoundOperation[] ops = new Cube.SoundOperation[4];
            int dur = 120;
            ops[0] = new Cube.SoundOperation(dur, 255, 70);
            ops[1] = new Cube.SoundOperation(dur, 255, 66);
            ops[2] = new Cube.SoundOperation(dur, 255, 64);
            ops[3] = new Cube.SoundOperation(dur*3, 255, 59);
            PlaySound(1, ops);
        }
        internal override void PlaySound_PowerOn()
        {
            Cube.SoundOperation[] ops = new Cube.SoundOperation[3];
            int dur = 120;
            ops[0] = new Cube.SoundOperation(dur, 255, 59);
            ops[1] = new Cube.SoundOperation(dur, 255, 66);
            ops[2] = new Cube.SoundOperation(dur*3, 255, 75);
            PlaySound(1, ops);
        }
        internal override void PlaySound_PowerOff()
        {
            Cube.SoundOperation[] ops = new Cube.SoundOperation[3];
            int dur = 120;
            ops[0] = new Cube.SoundOperation(dur, 255, 70);
            ops[1] = new Cube.SoundOperation(dur, 255, 66);
            ops[2] = new Cube.SoundOperation(dur*2, 255, 59);
            PlaySound(1, ops);
        }

    }
}