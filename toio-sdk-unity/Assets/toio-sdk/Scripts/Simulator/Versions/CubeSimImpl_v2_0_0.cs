using System;
using System.Collections.Generic;
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

        // ============ toio ID ============
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
        protected void _SetXYDeg(int x, int y, int deg, int xSensor, int ySensor)
        {
            deg = (deg%360+360)%360;
            if (this.x != x || this.y != y || this.deg != deg || !this.onMat)
                this.IDCallback?.Invoke(x, y, deg, xSensor, ySensor);
            this.x = x; this.y = y; this.deg = deg;
            this.xSensor = xSensor; this.ySensor = ySensor;
            this.onMat = true;
            this.onStandardID = false;
        }
        protected void _SetSandardID(uint stdID, int deg)
        {
            deg = (deg%360+360)%360;
            if (this.standardID != stdID || this.deg != deg || !this.onStandardID)
                this.standardIDCallback?.Invoke(stdID, deg);
            this.standardID = stdID;
            this.deg = deg;
            this.onStandardID = true;
            this.onMat = false;
        }
        protected void _SetOffGround()
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
            // 読み取りセンサーを模擬
            // Simuate Position ID & Standard ID Sensor
            RaycastHit hit;
            Vector3 gposSensor = cube.transform.Find("sensor").position;
            Ray ray = new Ray(gposSensor, -cube.transform.up);
            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform.gameObject.tag == "Mat" && hit.distance < 0.005f){
                    var mat = hit.transform.gameObject.GetComponent<Mat>();
                    var coord = mat.UnityCoord2MatCoord(cube.transform.position);
                    var deg = mat.UnityDeg2MatDeg(cube.transform.eulerAngles.y);
                    var coordSensor = mat.UnityCoord2MatCoord(gposSensor);
                    var xSensor = coordSensor.x; var ySensor = coordSensor.y;
                    _SetXYDeg(coord.x, coord.y, deg, xSensor, ySensor);
                }
                else if (hit.transform.gameObject.tag == "StandardID" && hit.distance < 0.005f)
                {
                    var stdID = hit.transform.gameObject.GetComponentInParent<StandardID>();
                    var deg = stdID.UnityDeg2MatDeg(cube.transform.eulerAngles.y);
                    _SetSandardID(stdID.id, deg);
                }
                else _SetOffGround();
            }
            else _SetOffGround();
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

        // ============ Button ============
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

        // ============ Motion Sensor ============
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
            slopeThreshold = 45; // TODO need check
            _collisonDetected = false;
        }

        // ----------- Sloped -----------
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


        // ============ Motor ============
        protected struct MotorTimeCmd
        {
            public int left, right, duration;
            public float tRecv;
        }
        protected Queue<MotorTimeCmd> motorTimeCmdQ = new Queue<MotorTimeCmd>(); // command queue
        protected MotorTimeCmd currMotorTimeCmd = default;  // current command


        protected virtual void MotorScheduler(float t)
        {
            while (motorTimeCmdQ.Count>0 && t > motorTimeCmdQ.Peek().tRecv)
            {
                currMotorTimeCmd = motorTimeCmdQ.Dequeue();
            }
            var elipsed = t - currMotorTimeCmd.tRecv;

            // ----- Excute Order -----
            if (currMotorTimeCmd.duration==0
                || elipsed < currMotorTimeCmd.duration/1000f)
            {
                motorCmdL = currMotorTimeCmd.left;
                motorCmdR = currMotorTimeCmd.right;
            }
            else
            {
                motorCmdL = 0; motorCmdR = 0;
            }
        }
        protected override void ResetMotor()
        {
            base.ResetMotor();

            motorTimeCmdQ.Clear();
            currMotorTimeCmd = default;
        }

        // ============ Light ============
        protected struct LightCmd
        {
            public byte r, g, b;
            public short duration;    // ms
            public float tRecv;
        }
        protected Queue<LightCmd> lightCmdQ = new Queue<LightCmd>();
        protected LightCmd currLightCmd = default;

        protected struct LightSenarioCmd
        {
            public byte repeat;
            public Cube.LightOperation[] lights;
            public float tRecv;
            public float period;    // s
        }
        protected Queue<LightSenarioCmd> lightSenarioCmdQ = new Queue<LightSenarioCmd>();
        protected LightSenarioCmd currLightSenarioCmd = default;

        protected void LightScheduler(float t)
        {
            // ----- Simulate Lag -----
            while (lightCmdQ.Count > 0 && t > lightCmdQ.Peek().tRecv){
                currLightCmd = lightCmdQ.Dequeue();
            }
            while (lightSenarioCmdQ.Count > 0 && t > lightSenarioCmdQ.Peek().tRecv){
                currLightSenarioCmd = lightSenarioCmdQ.Dequeue();
            }

            // ----- Excute Order -----
            if (currLightCmd.tRecv >= currLightSenarioCmd.tRecv)    // light cmd
            {
                float elipsed = t - currLightCmd.tRecv;
                if (currLightCmd.duration==0 || elipsed < currLightCmd.duration/1000f)
                    cube._SetLight(currLightCmd.r, currLightCmd.g, currLightCmd.b);
                else cube._StopLight();
            }
            else    // light senario cmd
            {
                float elipsed = t - currLightSenarioCmd.tRecv;
                if (currLightSenarioCmd.period==0
                    || currLightSenarioCmd.repeat>0 && currLightSenarioCmd.period*currLightSenarioCmd.repeat <= elipsed){
                    cube._StopLight();
                }
                else
                {
                    // Index of current operation
                    float sum = 0; int index=0; var lights = currLightSenarioCmd.lights;
                    for (int i=0; i<lights.Length; ++i){
                        sum += lights[i].durationMs/1000f;
                        if (elipsed % currLightSenarioCmd.period < sum){
                            index = i;
                            break;
                        }
                    }
                    cube._SetLight(lights[index].red, lights[index].green, lights[index].blue);
                }
            }
        }

        protected virtual void ResetLight()
        {
            lightCmdQ.Clear();
            lightSenarioCmdQ.Clear();
            currLightCmd = default;
            currLightSenarioCmd = default;
        }


        // ============ Sound ============
        protected struct SoundSenarioCmd
        {
            public byte repeat;
            public Cube.SoundOperation[] sounds;
            public float tRecv;
            public float period;
        }
        protected Queue<SoundSenarioCmd> soundSenarioCmdQ = new Queue<SoundSenarioCmd>();
        protected SoundSenarioCmd currSoundSenarioCmd = default;

        protected void SoundScheduler(float t)
        {
            // ----- Simulate Lag -----
            while (soundSenarioCmdQ.Count > 0 && t > soundSenarioCmdQ.Peek().tRecv){
                currSoundSenarioCmd = soundSenarioCmdQ.Dequeue();
            }
            var elipsed = t - currSoundSenarioCmd.tRecv;

            // ----- Excute Order -----
            if (currSoundSenarioCmd.period==0
                || currSoundSenarioCmd.repeat>0 && currSoundSenarioCmd.period*currSoundSenarioCmd.repeat <= elipsed)
                cube._StopSound();
            else
            {
                // Index of current operation
                float sum = 0; int index=0; var sounds = currSoundSenarioCmd.sounds;
                for (int i=0; i<sounds.Length; ++i){
                    sum += sounds[i].durationMs/1000f;
                    if (elipsed%currSoundSenarioCmd.period < sum){
                        index = i;
                        break;
                    }
                }
                cube._PlaySound(sounds[index].note_number, sounds[index].volume);
            }
        }

        protected virtual void ResetSound()
        {
            soundSenarioCmdQ.Clear();
            currSoundSenarioCmd = default;
        }


        // ============ Commands ============
        public override void Move(int left, int right, int durationMS)
        {
            MotorTimeCmd cmd = new MotorTimeCmd();
            cmd.left = Mathf.Clamp(left, -maxMotor, maxMotor);
            cmd.right = Mathf.Clamp(right, -maxMotor, maxMotor);
            if (Mathf.Abs(cmd.left) < this.deadzone) cmd.left = 0;
            if (Mathf.Abs(cmd.right) < this.deadzone) cmd.right = 0;
            cmd.duration = Mathf.Clamp(durationMS, 0, 2550);
            cmd.tRecv = Time.time;
            motorTimeCmdQ.Enqueue(cmd);
        }
        public override void StopLight()
        {
            SetLight(0, 0, 0, 100);
        }
        public override void SetLight(int r, int g, int b, int durationMS)
        {
            LightCmd cmd = new LightCmd();
            cmd.r = (byte)r; cmd.g = (byte)g; cmd.b = (byte)b;
            cmd.duration = (short)durationMS;
            cmd.tRecv = Time.time;
            lightCmdQ.Enqueue(cmd);
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
            lightSenarioCmdQ.Enqueue(cmd);
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
            soundSenarioCmdQ.Enqueue(cmd);
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
            Cube.SoundOperation[] ops = new Cube.SoundOperation[1];
            ops[0] = new Cube.SoundOperation(100, 0, 128);
            PlaySound(1, ops);
        }
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

        // 水平検出の閾値
        protected int slopeThreshold = 45;
        public override void ConfigSlopeThreshold(int degree)
        {
            slopeThreshold = degree;
        }
    }
}