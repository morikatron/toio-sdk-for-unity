using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;


namespace toio.Simulator
{
    [DisallowMultipleComponent]
    public class CubeSimulator : MonoBehaviour
    {
        #pragma warning disable 0414
        #pragma warning disable 0649

        // ======== Physical Constants ========
        // from https://toio.github.io/toio-spec/
        public const float TireWidthM = 0.0266f;
        public const float TireWidthDot= 0.0266f * Mat.DotPerM;
        public const float WidthM= 0.0318f;
        // ratio of Speed(Dot/s) and order ( 2.04f in real test )
        // theorically, 4.3 rpm/u * pi * 0.0125m / (60s/m) * DotPerM
        public const float VMeterOverU = 4.3f*Mathf.PI*0.0125f/60;
        public const float VDotOverU =  VMeterOverU * Mat.DotPerM; // about 2.06
        public const float MagneticFieldScale = 450;


        // ======== Simulator Settings ========
        public enum Version
        {
            v2_0_0,
            v2_1_0,
            v2_2_0,
            v2_3_0
        }
        [SerializeField]
        public Version version = Version.v2_3_0;
        [SerializeField]
        public bool powerStart = true;
        [SerializeField]
        public float motorTau = 0.04f; // parameter of one-order model for motor, τ
        [SerializeField]
        public float delay = 0.15f; // latency of communication

        [SerializeField]
        public bool forceStop = false;


        // ======== Properties ========

        private bool _power;
        public bool power { get {return _power;} set {
            if (value == _power) return;
            if (value) PowerOn();
            else PowerOff();
        } }

        /// <summary>
        /// モーター指令の最大値
        /// </summary>
        public int maxMotor { get{
            return impl.maxMotor;
        }}

        /// <summary>
        /// モーター指令のデッドゾーン
        /// </summary>
        public int deadzone { get{
            return impl.deadzone;
        }}


        /// <summary>
        /// シミュレータが稼働しているか
        /// </summary>
        public bool isRunning { get; private set; } = false;

        // ----- toio ID -----

        /// <summary>
        /// マット上の x 座標
        /// </summary>
        public int x { get {return impl.x;} internal set {impl.x = value;} }

        /// <summary>
        /// マット上の y 座標
        /// </summary>
        public int y { get {return impl.y;} internal set {impl.y = value;} }

        /// <summary>
        /// マット・スタンダードID上の角度
        /// </summary>
        public int deg { get {return impl.deg;} internal set {impl.deg = value;} }

        /// <summary>
        /// マット上の読み取りセンサーの y 座標
        /// </summary>
        public int xSensor { get {return impl.xSensor;} internal set {impl.xSensor = value;} }

        /// <summary>
        /// マット上の読み取りセンサーの y 座標
        /// </summary>
        public int ySensor { get {return impl.ySensor;} internal set {impl.ySensor = value;} }

        /// <summary>
        /// Standard ID の値
        /// </summary>
        public uint standardID { get {return impl.standardID;} internal set {impl.standardID = value;} }

        /// <summary>
        /// マット上にあるか
        /// </summary>
        public bool onMat { get {return impl.onMat;} internal set {impl.onMat = value;} }

        /// <summary>
        /// Standard ID 上にあるか
        /// </summary>
        public bool onStandardID { get {return impl.onStandardID;} internal set {impl.onStandardID = value;} }

        /// <summary>
        /// マット又はStandard ID 上にあるか
        /// </summary>
        public bool isGrounded { get {return onMat || onStandardID; } }

        // ----- Button -----

        /// <summary>
        /// ボタンが押されているか
        /// </summary>
        public bool button{ get {return impl.button;} internal set {impl.button = value;} }

        // ----- Motion Sensor -----
        // 2.0.0

        /// <summary>
        /// 水平検出をシミュレータがシミュレーションするか
        /// </summary>
        [HideInInspector]
        internal bool isSimulateSloped = true;

        /// <summary>
        /// 傾斜であるか
        /// </summary>
        public bool sloped{ get {return impl.sloped;} internal set {impl.sloped = value;} }

        // 2.1.0

        /// <summary>
        /// ポーズ
        /// </summary>
        public Cube.PoseType pose{ get {return impl.pose;} internal set {impl.pose = value;} }

        // 2.2.0

        /// <summary>
        /// シェイクが検出されたか
        /// </summary>
        public int shakeLevel{ get {return impl.shakeLevel;} internal set {impl.shakeLevel = value;} }

        /// <summary>
        /// コアキューブのモーター ID 1（左）の速度
        /// </summary>
        public int leftMotorSpeed{ get {return impl.leftMotorSpeed;} }

        /// <summary>
        /// コアキューブのモーター ID 2（右）の速度
        /// </summary>
        public int rightMotorSpeed{ get {return impl.rightMotorSpeed;} }

        // ----- Magnetic Sensor -----

        [HideInInspector]
        internal bool isSimulateMagneticSensor = true;
        public Cube.MagnetState magnetState { get {return impl.magnetState;} }
        public Vector3 magneticForce { get {return impl.magneticForce;} }


        // ======== Objects ========
        private Rigidbody rb;
        private AudioSource audioSource;
        private GameObject cubeModel;
        private GameObject LED;
        private BoxCollider col;

        private CubeSimImpl impl;



        private void Start()
        {
            this.rb = GetComponent<Rigidbody>();
            this.rb.maxAngularVelocity = 21f;
            this.audioSource = GetComponent<AudioSource>();
            this.LED = transform.Find("LED").gameObject;
            this.LED.GetComponent<Renderer>().material.color = Color.black;
            this.cubeModel = transform.Find("cube_model").gameObject;
            this.col = GetComponent<BoxCollider>();

            this._power = powerStart;
            this.isRunning = powerStart;

            switch (version)
            {
                case Version.v2_0_0 : this.impl = new CubeSimImpl_v2_0_0(this);break;
                case Version.v2_1_0 : this.impl = new CubeSimImpl_v2_1_0(this);break;
                case Version.v2_2_0 : this.impl = new CubeSimImpl_v2_2_0(this);break;
                case Version.v2_3_0 : this.impl = new CubeSimImpl_v2_3_0(this);break;
                default : this.impl = new CubeSimImpl_v2_3_0(this);break;
            }
            this._InitPresetSounds();
        }

        private void Update()
        {
        }

        private void FixedUpdate()
        {
            SimulatePhysics_Input();

            if (power)
            {
                // Hardware Simulation
                _SimulateIMU();

                // Firmware Simulation
                impl.Simulate();

                // Connection
                if (isRunning && !isConnected && isConnecting && Time.renderedFrameCount>4) // fixedTime is not stable at startup
                {
                    isConnected = true;
                    isConnecting = false;
                    onConnected?.Invoke();
                    impl.PlaySound_Connect();
                }
            }

            SimulatePhysics_Output();
        }

        private void OnDisable()
        {
            if (isConnected)
            {
                isConnected = false;
                isConnecting = false;
                this.onDisconnected?.Invoke();
                this.onConnected = null;
                this.onDisconnected = null;
            }
            StopAllCoroutines();
        }

        private void Init()
        {
            // Hardware
            _InitIMU();

            // Firmware
            this.impl.Init();
        }

        private void Reset()
        {
            // Firmware
            this.impl.Reset();

            // Hardware
            _StopLight();
            _StopSound();
            playingSoundId = -1;
        }

        private bool isPowerChanging = false;
        private void PowerOn()
        {
            if (isPowerChanging) return;
            isPowerChanging = true;
            Init();

            IEnumerator _PowerOn(){
                _power = true;
                impl.PlaySound_PowerOn();
                yield return new WaitForSeconds(0.5f);
                isRunning = true;
                isPowerChanging = false;
            }
            StartCoroutine(_PowerOn());
        }
        private void PowerOff()
        {
            if (isPowerChanging) return;
            isPowerChanging = true;

            if (isConnected)
            {
                isConnected = false;
                isConnecting = false;
                this.onDisconnected?.Invoke();
                this.onConnected = null;
                this.onDisconnected = null;
            }

            IEnumerator _PowerOff(){
                isRunning = false;
                impl.PlaySound_PowerOff();
                yield return new WaitForSeconds(0.7f);
                Reset();
                _power = false;
                isPowerChanging = false;
                StopAllCoroutines();
            }
            StartCoroutine(_PowerOff());
        }


        // ======== Connection ========
        public bool isConnected { get; private set; } = false;
        public bool isConnecting { get; private set; } = false;
        private Action onConnected = null;
        private Action onDisconnected = null;
        public bool Connect(Action onConnected, Action onDisconnected)
        {
            if (!isRunning) return false;
            if (isConnected || isConnecting) return false;
            isConnecting = true;
            this.onConnected = onConnected;
            this.onDisconnected = onDisconnected;
            return true;
        }
        public bool Disconnect()
        {
            if (!isRunning) return false;
            if (!isConnected) return false;
            this.Reset();
            impl.PlaySound_Disconnect();
            isConnected = false;
            isConnecting = false;
            this.onDisconnected?.Invoke();
            this.onConnected = null;
            this.onDisconnected = null;
            return true;
        }


        // ======== Hardware Simulation ========
        internal bool offGroundL = true;
        internal bool offGroundR = true;
        protected float speedL = 0;  // (m/s)
        protected float speedR = 0;
        internal float speedTireL = 0;
        internal float speedTireR = 0;
        private float motorTargetSpdL = 0;
        private float motorTargetSpdR = 0;
        private void SimulatePhysics_Input()
        {
            // タイヤの着地状態を調査
            // Check if tires are Off Ground
            RaycastHit hit;
            var ray = new Ray(transform.position+transform.up*0.001f-transform.right*0.0133f, -transform.up); // left wheel
            if (Physics.Raycast(ray, out hit) && hit.distance < 0.002f) offGroundL = false;
            ray = new Ray(transform.position+transform.up*0.001f+transform.right*0.0133f, -transform.up); // right wheel
            if (Physics.Raycast(ray, out hit) && hit.distance < 0.002f) offGroundR = false;

        }
        private void SimulatePhysics_Output()
        {
            // タイヤ速度を更新
            if (this.forceStop || this.button || !this.isConnected)   // 強制的に停止
            {
                speedTireL = 0; speedTireR = 0;
            }
            else
            {
                var dt = Time.fixedDeltaTime;
                speedTireL += (motorTargetSpdL - speedTireL) / Mathf.Max(this.motorTau, dt) * dt;
                speedTireR += (motorTargetSpdR - speedTireR) / Mathf.Max(this.motorTau, dt) * dt;
            }

            // 着地状態により、キューブの速度を取得
            // update object's speed
            // NOTES: simulation for slipping shall be implemented here
            speedL = offGroundL? 0: speedTireL;
            speedR = offGroundR? 0: speedTireR;

            // Output
            _SetSpeed(speedL, speedR);
        }

        internal void SetMotorTargetSpd(float left, float right)
        {
            motorTargetSpdL = left; motorTargetSpdR = right;
        }


        #region ============ Event ============

        // ------------ v2.0.0 ------------

        /// <summary>
        /// ボタンのイベントコールバックを設定する
        /// </summary>
        public void StartNotification_Button(System.Action<bool> action)
        {
            if (!isConnected) return;
            impl.StartNotification_Button(action);
        }

        /// <summary>
        /// Standard ID のイベントコールバックを設定する
        /// </summary>
        public void StartNotification_StandardID(System.Action<uint, int> action)
        {
            if (!isConnected) return;
            impl.StartNotification_StandardID(action);
        }

        /// <summary>
        /// Standard ID から持ち上げられた時のイベントコールバックを設定する
        /// </summary>
        public void StartNotification_StandardIDMissed(System.Action action)
        {
            if (!isConnected) return;
            impl.StartNotification_StandardIDMissed(action);
        }

        /// <summary>
        /// Position ID （マット座標）のイベントコールバックを設定する
        /// </summary>
        public void StartNotification_PositionID(System.Action<int, int, int, int, int> action)
        {
            if (!isConnected) return;
            impl.StartNotification_PositionID(action);
        }

        /// <summary>
        /// マットから持ち上げられた時のイベントコールバックを設定する
        /// </summary>
        public void StartNotification_PositionIDMissed(System.Action action)
        {
            if (!isConnected) return;
            impl.StartNotification_PositionIDMissed(action);
        }

        /// <summary>
        /// 水平検出のイベントコールバックを設定する
        /// </summary>
        public void StartNotification_MotionSensor(System.Action<object[]> action)
        {
            if (!isConnected) return;
            impl.StartNotification_MotionSensor(action);
        }

        // ------------ v2.1.0 ------------
        /// <summary>
        /// 目標指定付きモーター制御の応答コールバックを設定する
        /// </summary>
        public void StartNotification_TargetMove(System.Action<int, Cube.TargetMoveRespondType> action)
        {
            if (!isConnected) return;
            impl.StartNotification_TargetMove(action);
        }

        /// <summary>
        /// 複数目標指定付きモーター制御の応答コールバックを設定する
        /// </summary>
        public void StartNotification_MultiTargetMove(System.Action<int, Cube.TargetMoveRespondType> action)
        {
            if (!isConnected) return;
            impl.StartNotification_MultiTargetMove(action);
        }

        // ------------ v2.2.0 ------------
        /// <summary>
        /// モーター速度読み取りのイベントコールバックを設定する
        /// </summary>
        public void StartNotification_MotorSpeed(System.Action<int, int> action)
        {
            if (!isConnected) return;
            impl.StartNotification_MotorSpeed(action);
        }

        /// <summary>
        /// 磁石状態読み取りのイベントコールバックを設定する
        /// </summary>
        public void StartNotification_MagnetState(System.Action<Cube.MagnetState> action)
        {
            if (!isConnected) return;
            impl.StartNotification_MagnetState(action);
        }

        /// <summary>
        /// モーター速度設定の応答の読み出しコールバックを設定する
        /// 引数：モーター速度設定応答
        /// </summary>
        public void StartNotification_ConfigMotorRead(System.Action<bool> action)
        {
            if (!isConnected) return;
            impl.StartNotification_ConfigMotorRead(action);
        }

        public void StartNotification_ConfigIDNotification(System.Action<bool> action)
        {
            if (!isConnected) return;
            impl.StartNotification_ConfigIDNotification(action);
        }

        public void StartNotification_ConfigIDMissedNotification(System.Action<bool> action)
        {
            if (!isConnected) return;
            impl.StartNotification_ConfigIDMissedNotification(action);
        }

        public void StartNotification_ConfigMagneticSensor(System.Action<bool> action)
        {
            if (!isConnected) return;
            impl.StartNotification_ConfigMagneticSensor(action);
        }

        // ------------ v2.3.0 ------------
        public void StartNotification_MagneticForce(System.Action<Vector3> action)
        {
            if (!isConnected) return;
            impl.StartNotification_MagneticForce(action);
        }

        public void StartNotification_Attitude(System.Action<Vector3> actionE, System.Action<Quaternion> actionQ)
        {
            if (!isConnected) return;
            impl.StartNotification_Attitude(actionE, actionQ);
        }
        public void StartNotification_ConfigAttitudeSensor(System.Action<bool> action)
        {
            if (!isConnected) return;
            impl.StartNotification_ConfigAttitudeSensor(action);
        }

        #endregion



        #region ============ コマンド ============

        private void DelayCommand(Action action)
        {
            if (!isConnected) return;
            IEnumerator Cmd(){
                yield return new WaitForSeconds(this.delay);
                if (!isConnected) yield break;
                action?.Invoke();
            }
            StartCoroutine(Cmd());
        }

        // --------- 2.0.0 --------
        /// <summary>
        /// モーター：時間指定付きモーター制御
        /// </summary>
        public void Move(int left, int right, int durationMS)
        {
            DelayCommand(() => impl.Move(left, right, durationMS));
        }

        /// <summary>
        /// ランプ：消灯
        /// </summary>
        public void StopLight()
        {
            DelayCommand(() => impl.StopLight());
        }
        /// <summary>
        /// ランプ：点灯
        /// </summary>
        public void SetLight(int r, int g, int b, int durationMS)
        {
            DelayCommand(() => impl.SetLight(r, g, b, durationMS));
        }
        /// <summary>
        /// ランプ：連続的な点灯・消灯
        /// </summary>
        public void SetLights(int repeatCount, Cube.LightOperation[] operations)
        {
            DelayCommand(() => impl.SetLights(repeatCount, operations));
        }

        /// <summary>
        /// サウンド：MIDI note number の再生
        /// </summary>
        public void PlaySound(int repeatCount, Cube.SoundOperation[] operations)
        {
            DelayCommand(() => impl.PlaySound(repeatCount, operations));
        }
        /// <summary>
        /// サウンド：効果音の再生
        /// </summary>
        public void PlayPresetSound(int soundId, int volume)
        {
            DelayCommand(() => impl.PlayPresetSound(soundId, volume));
        }
        /// <summary>
        /// サウンド：再生の停止
        /// </summary>
        public void StopSound()
        {
            DelayCommand(() => impl.StopSound());
        }

        /// <summary>
        /// 水平検出の閾値を設定する（度）
        /// </summary>
        public void ConfigSlopeThreshold(int degree)
        {
            DelayCommand(() => impl.ConfigSlopeThreshold(degree));
        }

        // --------- 2.1.0 --------
        public void TargetMove(
            int targetX,
            int targetY,
            int targetAngle,
            int configID,
            int timeOut,
            Cube.TargetMoveType targetMoveType,
            int maxSpd,
            Cube.TargetSpeedType targetSpeedType,
            Cube.TargetRotationType targetRotationType
        ){
            DelayCommand(() => impl.TargetMove(targetX, targetY, targetAngle, configID, timeOut, targetMoveType, maxSpd, targetSpeedType, targetRotationType));
        }

        public void MultiTargetMove(
            int[] targetXList,
            int[] targetYList,
            int[] targetAngleList,
            Cube.TargetRotationType[] multiRotationTypeList,
            int configID,
            int timeOut,
            Cube.TargetMoveType targetMoveType,
            int maxSpd,
            Cube.TargetSpeedType targetSpeedType,
            Cube.MultiWriteType multiWriteType
        ){
            DelayCommand(() => impl.MultiTargetMove(targetXList, targetYList, targetAngleList, multiRotationTypeList, configID, timeOut, targetMoveType, maxSpd, targetSpeedType, multiWriteType));
        }

        public void AccelerationMove(
            int targetSpeed,
            int acceleration,
            int rotationSpeed,
            Cube.AccPriorityType accPriorityType,
            int controlTime
        ){
            DelayCommand(() => impl.AccelerationMove(targetSpeed, acceleration, rotationSpeed, accPriorityType, controlTime));
        }

        // --------- 2.2.0 --------
        /// <summary>
        /// モーターの速度情報の取得の設定
        /// </summary>
        public void ConfigMotorRead(bool enabled)
        {
            DelayCommand(() => impl.ConfigMotorRead(enabled));
        }

        public void ConfigIDNotification(int interval, Cube.IDNotificationType notificationType)
        {
            DelayCommand(() => impl.ConfigIDNotification(interval, notificationType));
        }

        public void ConfigIDMissedNotification(int sensitivity)
        {
            DelayCommand(() => impl.ConfigIDMissedNotification(sensitivity));
        }

        public void ConfigMagneticSensor(Cube.MagneticMode mode)
        {
            DelayCommand(() => impl.ConfigMagneticSensor(mode));
        }

        public void RequestMotionSensor()
        {
            DelayCommand(() => impl.RequestMotionSensor());
        }

        public void RequestMagneticSensor()
        {
            DelayCommand(() => impl.RequestMagneticSensor());
        }

        // --------- 2.3.0 --------

        public void ConfigMagneticSensor(Cube.MagneticMode mode, int interval, Cube.MagneticNotificationType notificationType)
        {
            DelayCommand(() => impl.ConfigMagneticSensor(mode, interval, notificationType));
        }

        public void ConfigAttitudeSensor(Cube.AttitudeFormat format, int interval, Cube.AttitudeNotificationType notificationType)
        {
            DelayCommand(() => impl.ConfigAttitudeSensor(format, interval, notificationType));
        }

        public void RequestAttitudeSensor(Cube.AttitudeFormat format)
        {
            DelayCommand(() => impl.RequestAttitudeSensor(format));
        }

        #endregion


        #region  ============ Internal Methods: Hardware Simulation ============

        // -------- ID Sensor --------
        internal (Vector2, uint, float) _GetIDSensor()
        {
            const float maxDistance = 0.005f;

            RaycastHit hit;
            Vector3 gposSensor = transform.Find("IDSensor").position;
            Ray ray = new Ray(gposSensor, -transform.up);
            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform.gameObject.tag == "t4u_Mat" && hit.distance < maxDistance){
                    var mat = hit.transform.gameObject.GetComponent<Mat>();
                    var deg = mat.UnityDeg2MatDegF(transform.eulerAngles.y);
                    var coordSensor = mat.UnityCoord2MatCoordF(gposSensor);
                    return (coordSensor, 0, deg);
                }
                else if (hit.transform.gameObject.tag == "t4u_StandardID" && hit.distance < maxDistance)
                {
                    var stdID = hit.transform.gameObject.GetComponentInParent<StandardID>();
                    var deg = stdID.UnityDeg2MatDegF(transform.eulerAngles.y);
                    return (Vector2.zero, stdID.id, deg);
                }
            }
            return (Vector2.zero, 0, 0);
        }

        // -------- Motion Sensor Triggers --------
        internal void _TriggerCollision()
        {
            if (!isConnected) return;
            this.impl.TriggerCollision();
        }

        internal void _TriggerDoubleTap()
        {
            if (!isConnected) return;
            this.impl.TriggerDoubleTap();
        }

        // -------- Motor --------
        internal void _SetSpeed(float speedL, float speedR)
        {
            this.rb.angularVelocity = transform.up * (float)((speedL - speedR) / TireWidthM);
            var vel = transform.forward * (speedL + speedR) / 2;
            var dv = vel - this.rb.velocity;
            this.rb.AddForce(dv, ForceMode.VelocityChange);
        }

        // -------- LED --------
        internal void _SetLight(int r, int g, int b){
            r = Mathf.Clamp(r, 0, 255);
            g = Mathf.Clamp(g, 0, 255);
            b = Mathf.Clamp(b, 0, 255);
            LED.GetComponent<Renderer>().material.color = new Color(r/255f, g/255f, b/255f);
        }

        internal void _StopLight(){
            LED.GetComponent<Renderer>().material.color = Color.black;
        }

        // -------- Sound --------
        private int playingSoundId = -1;
        internal void _PlaySound(int soundId, int volume){
            if (soundId >= 128) { _StopSound(); return; }
            if (soundId != playingSoundId)
            {
                playingSoundId = soundId;
                int octave = (int)(soundId/12);
                int idx = (int)(soundId%12);
                var aCubeOnSlot = Resources.Load("Octave/" + (octave*12+9)) as AudioClip;
                audioSource.pitch = (float)Math.Pow(2, ((float)idx-9)/12);
                audioSource.clip = aCubeOnSlot;
            }
            audioSource.volume = (float)volume/256 * 0.5f;
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        internal void _StopSound(){
            playingSoundId = -1;
            audioSource.clip = null;
            audioSource.Stop();
        }

        internal void _InitPresetSounds(){
            impl.presetSounds.Add( new Cube.SoundOperation[2]
            {
                new Cube.SoundOperation(60, 255, 71),
                new Cube.SoundOperation(60, 255, 67),
            });
            impl.presetSounds.Add( new Cube.SoundOperation[2]
            {
                new Cube.SoundOperation(40, 255, 78),
                new Cube.SoundOperation(200, 255, 81),
            });
            impl.presetSounds.Add( new Cube.SoundOperation[3]
            {
                new Cube.SoundOperation(70, 255, 69),
                new Cube.SoundOperation(60, 255, 67),
                new Cube.SoundOperation(60, 255, 66),
            });
            impl.presetSounds.Add( new Cube.SoundOperation[1]
            {
                new Cube.SoundOperation(70, 255, 69),
            });
            impl.presetSounds.Add( new Cube.SoundOperation[4]
            {
                new Cube.SoundOperation(120, 255, 62),
                new Cube.SoundOperation(120, 255, 67),
                new Cube.SoundOperation(150, 255, 71),
                new Cube.SoundOperation(240, 255, 67),
            });
            impl.presetSounds.Add( new Cube.SoundOperation[4]
            {
                new Cube.SoundOperation(120, 255, 69),
                new Cube.SoundOperation(150, 255, 71),
                new Cube.SoundOperation(150, 255, 67),
                new Cube.SoundOperation(240, 255, 62),
            });
            impl.presetSounds.Add( new Cube.SoundOperation[4]
            {
                new Cube.SoundOperation(60, 255, 66),
                new Cube.SoundOperation(80, 255, 69),
                new Cube.SoundOperation(40, 255, 74),
                new Cube.SoundOperation(60, 255, 76),
            });
            impl.presetSounds.Add( new Cube.SoundOperation[5]
            {
                new Cube.SoundOperation(80, 255, 74),
                new Cube.SoundOperation(30, 255, 128),
                new Cube.SoundOperation(80, 255, 74),
                new Cube.SoundOperation(30, 255, 128),
                new Cube.SoundOperation(140, 255, 81),
            });
            impl.presetSounds.Add( new Cube.SoundOperation[3]
            {
                new Cube.SoundOperation(60, 255, 71),
                new Cube.SoundOperation(60, 255, 67),
                new Cube.SoundOperation(120, 255, 74),
            });
            impl.presetSounds.Add( new Cube.SoundOperation[1]
            {
                new Cube.SoundOperation(70, 255, 74),
            });
            impl.presetSounds.Add( new Cube.SoundOperation[1]
            {
                new Cube.SoundOperation(70, 255, 66),
            });
        }

        // -------- Button --------
        internal void _SetPressed(bool pressed)
        {
            this.cubeModel.transform.localEulerAngles
                    = pressed? new Vector3(-93,0,0) : new Vector3(-90,0,0);
        }

        // -------- Magnetic Sensor --------
        private Vector3 _magneticField = default;
        internal Vector3 _GetMagneticField()
        {
            if (isSimulateMagneticSensor)
            {
                var magnetObjs = GameObject.FindGameObjectsWithTag("t4u_Magnet");
                var magnets = Array.ConvertAll(magnetObjs, obj => obj.GetComponent<Magnet>());

                Vector3 magSensor = transform.Find("MagneticSensor").position;

                Vector3 h = Vector3.zero;
                foreach (var magnet in magnets)
                {
                    h += magnet.SumUpH(magSensor);
                }

                this._magneticField = new Vector3(h.z, h.x, -h.y);
            }
            return this._magneticField;
        }
        internal void _SetMagneticField(Cube.MagnetState state)
        {
            Vector3 field = default;
            switch (state){
                case Cube.MagnetState.S_Center: field = Vector3.back * MagneticFieldScale * 24; break;
                case Cube.MagnetState.N_Center: field = Vector3.forward * MagneticFieldScale * 24; break;
                case Cube.MagnetState.N_Right: field = new Vector3(0, -1, 1) * MagneticFieldScale * 13; break;
                case Cube.MagnetState.N_Left: field = new Vector3(0, 1, 1) * MagneticFieldScale * 13; break;
                case Cube.MagnetState.S_Right: field = new Vector3(0, 1, -1) * MagneticFieldScale * 13; break;
                case Cube.MagnetState.S_Left: field = new Vector3(0, -1, -1) * MagneticFieldScale * 13; break;
                case Cube.MagnetState.None: field = Vector3.zero; break;
            }
            this._magneticField = field;
        }
        internal Vector3 _GetScaledMagneticField()
        {
            return _GetMagneticField() / MagneticFieldScale;
        }
        internal void _SetMagneticField(Vector3 scaledField)
        {
            this._magneticField = scaledField * MagneticFieldScale;
        }

        // -------- Magnetic Sensor --------

        private float _attitudeYawBias;
        private float _attitudeYawBiasD;
        private void _InitIMU()
        {
            this._attitudeYawBias = transform.eulerAngles.y;
        }
        private void _SimulateIMU()
        {
            this._attitudeYawBiasD += (UnityEngine.Random.value-0.5f) * 0.1f;
            this._attitudeYawBiasD = Mathf.Clamp(this._attitudeYawBiasD, -1, 1);
            this._attitudeYawBias += (this._attitudeYawBiasD + UnityEngine.Random.value-0.5f) * 0.01f;
        }
        internal Vector3 _GetIMU()
        {
            var e = transform.eulerAngles;
            float roll = e.z;
            float pitch = e.x;
            float yaw = e.y - this._attitudeYawBias;

            // https://toio.github.io/toio-spec/docs/ble_high_precision_tilt_sensor/#姿勢角情報の取得オイラー角での通知
            return new Vector3(roll, pitch, yaw);
        }

        #endregion


    }

}