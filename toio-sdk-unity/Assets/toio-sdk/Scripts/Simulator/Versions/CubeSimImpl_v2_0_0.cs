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

            float dt = Time.deltaTime;
            float currentTime = Time.time;
            MotorScheduler(dt, currentTime);
            LightScheduler(dt, currentTime);
            SoundScheduler(dt, currentTime);
            SimulateMotor();
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

        // ============ Motion Sensor ============
        // ----------- Sloped -----------
        protected bool _sloped;
        public override bool sloped
        {
            get {return this._sloped;}
            internal set
            {
                if (this._sloped!=value){
                    this.slopeCallback?.Invoke(value);
                }
                this._sloped = value;
            }
        }
        protected System.Action<bool> slopeCallback = null;
        public override void StartNotification_Sloped(System.Action<bool> action)
        {
            this.slopeCallback = action;
            this.slopeCallback.Invoke(_sloped);
        }
        // ----------- Collision Detected -----------
        protected bool _collisionDetected;
        public override bool collisionDetected
        {
            get {return this._collisionDetected;}
            internal set
            {
                if (this._collisionDetected!=value){
                    this.collisionDetectedCallback?.Invoke(value);
                }
                this._collisionDetected = value;
            }
        }
        protected System.Action<bool> collisionDetectedCallback = null;
        public override void StartNotification_CollisionDetected(System.Action<bool> action)
        {
            this.collisionDetectedCallback = action;
            this.collisionDetectedCallback.Invoke(_collisionDetected);
        }

        // ----------- Simulate -----------
        protected virtual void SimulateMotionSensor()
        {
            // 水平検出
            if (cube.isSimulateSloped)
            {
                cube.sloped = Vector3.Angle(Vector3.up, cube.transform.up)>slopeThreshold;
            }

            // 衝突検出
            // 未実装
        }


        // ============ Motor ============
        protected float motorCmdElipsed = 0;
        protected struct MotorTimeCmd
        {
            public int left, right, duration;
            public float tRecv;
        }
        protected Queue<MotorTimeCmd> motorTimeCmdQ = new Queue<MotorTimeCmd>(); // command queue
        protected MotorTimeCmd currMotorTimeCmd = default;  // current command


        protected virtual void MotorScheduler(float dt, float t)
        {
            motorCmdElipsed += dt;

            while (motorTimeCmdQ.Count>0 && t > motorTimeCmdQ.Peek().tRecv + cube.delay)
            {
                motorCmdElipsed = 0;
                currMotorTimeCmd = motorTimeCmdQ.Dequeue();
            }

            // ----- Excute Order -----
            if (currMotorTimeCmd.duration==0
                || motorCmdElipsed < currMotorTimeCmd.duration/1000f)
            {
                motorLeft = currMotorTimeCmd.left;
                motorRight = currMotorTimeCmd.right;
            }
            else
            {
                motorLeft = 0; motorRight = 0;
            }
        }

        // ============ Light ============
        protected float lightCmdElipsed = 0;
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

        protected void LightScheduler(float dt, float t)
        {
            lightCmdElipsed += dt;

            // ----- Simulate Lag -----
            while (lightCmdQ.Count > 0 && t > lightCmdQ.Peek().tRecv + cube.delay){
                lightCmdElipsed = 0;
                currLightCmd = lightCmdQ.Dequeue();
            }
            while (lightSenarioCmdQ.Count > 0 && t > lightSenarioCmdQ.Peek().tRecv + cube.delay){
                lightCmdElipsed = 0;
                currLightSenarioCmd = lightSenarioCmdQ.Dequeue();
            }

            // ----- Excute Order -----
            if (currLightCmd.tRecv >= currLightSenarioCmd.tRecv)    // light cmd
            {
                if (currLightCmd.duration==0 || lightCmdElipsed < currLightCmd.duration/1000f)
                    cube._SetLight(currLightCmd.r, currLightCmd.g, currLightCmd.b);
                else cube._StopLight();
            }
            else    // light senario cmd
            {
                if (currLightSenarioCmd.period==0
                    || currLightSenarioCmd.repeat>0 && currLightSenarioCmd.period*currLightSenarioCmd.repeat <= lightCmdElipsed){
                    cube._StopLight();
                }
                else
                {
                    // Index of current operation
                    float sum = 0; int index=0; var lights = currLightSenarioCmd.lights;
                    for (int i=0; i<lights.Length; ++i){
                        sum += lights[i].durationMs/1000f;
                        if (lightCmdElipsed % currLightSenarioCmd.period < sum){
                            index = i;
                            break;
                        }
                    }
                    cube._SetLight(lights[index].red, lights[index].green, lights[index].blue);
                }
            }
        }


        // ============ Sound ============
        protected float soundCmdElipsed = 0;
        protected struct SoundSenarioCmd
        {
            public byte repeat;
            public Cube.SoundOperation[] sounds;
            public float tRecv;
            public float period;
        }
        protected Queue<SoundSenarioCmd> soundSenarioCmdQ = new Queue<SoundSenarioCmd>();
        protected SoundSenarioCmd currSoundSenarioCmd = default;

        protected void SoundScheduler(float dt, float t)
        {
            soundCmdElipsed += dt;

            // ----- Simulate Lag -----
            while (soundSenarioCmdQ.Count > 0 && t > soundSenarioCmdQ.Peek().tRecv + cube.delay){
                soundCmdElipsed = 0;
                currSoundSenarioCmd = soundSenarioCmdQ.Dequeue();
            }

            // ----- Excute Order -----
            if (currSoundSenarioCmd.period==0
                || currSoundSenarioCmd.repeat>0 && currSoundSenarioCmd.period*currSoundSenarioCmd.repeat <= soundCmdElipsed)
                cube._StopSound();
            else
            {
                // Index of current operation
                float sum = 0; int index=0; var sounds = currSoundSenarioCmd.sounds;
                for (int i=0; i<sounds.Length; ++i){
                    sum += sounds[i].durationMs/1000f;
                    if (soundCmdElipsed%currSoundSenarioCmd.period < sum){
                        index = i;
                        break;
                    }
                }
                cube._PlaySound(sounds[index].note_number, sounds[index].volume);
            }
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

        // 水平検出の閾値
        protected int slopeThreshold = 45;
        public override void ConfigSlopeThreshold(int degree)
        {
            slopeThreshold = degree;
        }
    }
}