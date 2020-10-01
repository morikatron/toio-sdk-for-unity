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
            if (this.x != x || this.y != y || this.deg != deg || !this.onMat)
                this.IDCallback?.Invoke(x, y, deg, xSensor, ySensor);
            this.x = x; this.y = y; this.deg = deg;
            this.xSensor = xSensor; this.ySensor = ySensor;
            this.onMat = true;
            this.onStandardID = false;
        }
        protected void _SetSandardID(uint stdID, int deg)
        {
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
        protected float motorDuration = 0;
        protected float motorTimeElipsed = 0;
        protected Queue<int> motorLeftQ = new Queue<int>();
        protected Queue<int> motorRightQ = new Queue<int>();
        protected Queue<int> motorDurationQ = new Queue<int>();
        protected Queue<float> motorTimeQ = new Queue<float>();

        protected void MotorScheduler(float dt, float t)
        {
            motorTimeElipsed += dt;

            // ----- Simulate Lag -----
            while (motorTimeQ.Count > 0 && t > motorTimeQ.Peek() + cube.delay ){
                motorTimeElipsed = 0;
                motorDuration = motorDurationQ.Dequeue()/1000f;
                motorLeft = motorLeftQ.Dequeue();
                motorRight = motorRightQ.Dequeue();
                motorTimeQ.Dequeue();
            }

            // ----- Excute Order -----
            if (motorTimeElipsed > motorDuration && motorDuration > 0){
                motorLeft = 0; motorRight = 0;
            }
        }

        // ============ Light ============
        protected Cube.LightOperation[] lights = null;
        protected int lightRepeat;
        protected float lightTimeElipsed = 0;
        protected int lightRepeatedCnt = 0;
        protected bool lightLasting = false;
        protected Queue<Cube.LightOperation[]> lightsQ = new Queue<Cube.LightOperation[]>();
        protected Queue<int> lightRepeatQ = new Queue<int>();
        protected Queue<bool> lightLastingQ = new Queue<bool>();
        protected Queue<float> lightTimeQ = new Queue<float>();
        protected void LightScheduler(float dt, float t)
        {
            lightTimeElipsed += dt;

            // ----- Simulate Lag -----
            while (lightTimeQ.Count > 0 && t > lightTimeQ.Peek() + cube.delay ){
                lightTimeElipsed = 0;
                lights = lightsQ.Dequeue();
                lightRepeat = lightRepeatQ.Dequeue();
                lightTimeQ.Dequeue();
                lightLasting = lightLastingQ.Dequeue();
            }

            // ----- Excute Order -----
            if (lights == null)  cube._StopLight();
            else if (lightLasting){
                if (lightTimeElipsed==0)
                    // Turn on Light
                    cube._SetLight(lights[0].red, lights[0].green, lights[0].blue);
            }
            else
            {
                // Calc. period
                float period = 0;
                for (int i=0; i<lights.Length; ++i){
                    period += lights[i].durationMs/1000f;
                }
                if (period==0){
                    lightRepeatedCnt = 0;
                    lights = null;
                    cube._StopLight();
                }
                // Next repeat?
                if (lightTimeElipsed >= period){
                    lightRepeatedCnt += (int)(lightTimeElipsed/period);
                    lightTimeElipsed %= period;
                }
                // Repeat over
                if (lightRepeatedCnt >= lightRepeat && lightRepeat > 0){
                    lightRepeatedCnt = 0;
                    lights = null;
                    cube._StopLight();
                }
                else if (lights != null)
                {
                    // Index of current operation
                    float sum = 0; int index=0;
                    for (int i=0; i<lights.Length; ++i){
                        sum += lights[i].durationMs/1000f;
                        if (lightTimeElipsed < sum){
                            index = i;
                            break;
                        }
                    }
                    // Turn on Light
                    cube._SetLight(lights[index].red, lights[index].green, lights[index].blue);
                }
            }
        }


        // ============ Sound ============
        protected Cube.SoundOperation[] sounds = null;
        protected int playingSound=-1;
        protected int soundRepeat;
        protected float soundTimeElipsed = 0;
        protected int soundRepeatedCnt = 0;
        protected Queue<Cube.SoundOperation[]> soundsQ = new Queue<Cube.SoundOperation[]>();
        protected Queue<int> soundRepeatQ = new Queue<int>();
        protected Queue<float> soundTimeQ = new Queue<float>();
        protected void SoundScheduler(float dt, float t)
        {
            soundTimeElipsed += dt;

            // ----- Simulate Lag -----
            while (soundTimeQ.Count > 0 && t > soundTimeQ.Peek() + cube.delay){
                soundTimeElipsed = 0;
                sounds = soundsQ.Dequeue();
                soundRepeat = soundRepeatQ.Dequeue();
                soundTimeQ.Dequeue();
            }

            // ----- Excute Order -----
            if (sounds == null) cube._StopSound();
            else
            {
                // Calc. period
                float period = 0;
                for (int i=0; i<sounds.Length; ++i){
                    period += sounds[i].durationMs/1000f;
                }
                if (period == 0){
                    soundRepeatedCnt = 0;
                    sounds = null;
                    cube._StopSound();
                }
                // Next repeat?
                if (soundTimeElipsed >= period)
                {
                    soundRepeatedCnt += (int)(soundTimeElipsed/period);
                    soundTimeElipsed %= period;
                }
                // Repeat over
                if (soundRepeatedCnt >= soundRepeat && soundRepeat > 0)
                {
                    soundRepeatedCnt = 0;
                    sounds = null;
                    cube._StopSound();
                }
                else if (sounds != null)
                {
                    // Index of current operation
                    float sum = 0; int index=0;
                    for (int i=0; i<sounds.Length; ++i){
                        sum += sounds[i].durationMs/1000f;
                        if (soundTimeElipsed < sum){
                            index = i;
                            break;
                        }
                    }
                    // Play
                    int sound = sounds[index].note_number;
                    if (sound != playingSound){
                        playingSound = sound;
                        if (sound >= 128) cube._StopSound();
                        else cube._PlaySound(sound, sounds[index].volume);
                    }
                }
            }
        }


        // ============ Commands ============
        public override void Move(int left, int right, int durationMS)
        {
                motorDurationQ.Enqueue(Mathf.Clamp(durationMS, 0, 2550));
                motorLeftQ.Enqueue( Mathf.Clamp(left, -maxMotor, maxMotor));
                motorRightQ.Enqueue( Mathf.Clamp(right, -maxMotor, maxMotor));
                motorTimeQ.Enqueue(Time.time);
        }
        public override void StopLight()
        {
            Cube.LightOperation[] ops = new Cube.LightOperation[1];
            ops[0] = new Cube.LightOperation(100, 0, 0, 0);
            lightsQ.Enqueue(ops);
            lightRepeatQ.Enqueue(1);
            lightTimeQ.Enqueue(Time.time);
            lightLastingQ.Enqueue(false);
        }
        public override void SetLight(int r, int g, int b, int durationMS)
        {
            durationMS = Mathf.Clamp(durationMS / 10, 0, 255)*10;
            Cube.LightOperation[] ops = new Cube.LightOperation[1];
            ops[0] = new Cube.LightOperation((short)durationMS, (byte)r, (byte)g, (byte)b);
            lightsQ.Enqueue(ops);
            lightRepeatQ.Enqueue(1);
            lightTimeQ.Enqueue(Time.time);
            lightLastingQ.Enqueue(durationMS==0);
        }
        public override void SetLights(int repeatCount, Cube.LightOperation[] operations)
        {
            if (operations.Length == 0) return;
            repeatCount = Mathf.Clamp(repeatCount, 0, 255);
            operations = operations.Take(29).ToArray();
            lightsQ.Enqueue(operations);
            lightRepeatQ.Enqueue((byte)repeatCount);
            lightTimeQ.Enqueue(Time.time);
            lightLastingQ.Enqueue(false);
        }
        public override void PlaySound(int repeatCount, Cube.SoundOperation[] operations)
        {
            if (operations.Length == 0) return;
            repeatCount = Mathf.Clamp(repeatCount, 0, 255);
            operations = operations.Take(59).ToArray();
            soundsQ.Enqueue(operations);
            soundRepeatQ.Enqueue(repeatCount);
            soundTimeQ.Enqueue(Time.time);
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
            soundsQ.Enqueue(ops);
            soundRepeatQ.Enqueue(1);
            soundTimeQ.Enqueue(Time.time);
        }

        // 水平検出の閾値
        protected int slopeThreshold = 45;
        public override void SetSlopeThreshold(int degree)
        {
            slopeThreshold = degree;
        }
    }
}