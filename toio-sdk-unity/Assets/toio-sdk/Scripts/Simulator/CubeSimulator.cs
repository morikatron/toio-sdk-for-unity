using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace toio.Simulator
{
    public class CubeSimulator : MonoBehaviour
    {
        #pragma warning disable 0414
        #pragma warning disable 0649

        // ======== Physical Constants ========
        // from https://toio.github.io/toio-spec/
        public static readonly float TireWidthM = 0.0266f;
        public static readonly float TireWidthDot= 0.0266f * Mat.DotPerM;
        public static readonly float WidthM= 0.0318f;
        // ratio of Speed(Dot/s) and order ( 2.04f in real test )
        // theorically, 4.3 rpm/u * pi * 0.0125m / (60s/m) * DotPerM
        public static readonly float VDotOverU =  4.3f*Mathf.PI*0.0125f/60 * Mat.DotPerM; // about 2.06


        // ======== Simulator Settings ========
        public enum Version
        {
            v2_0_0,
        }
        [SerializeField]
        public Version version = Version.v2_0_0;
        [SerializeField]
        public float motorTau = 0.04f; // parameter of one-order model for motor, τ
        [SerializeField]
        public float delay = 0.15f; // latency of communication

        [SerializeField]
        public bool forceStop = false;


        // ======== Properties ========

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
        /// シミュレータが初期化できたか
        /// </summary>
        public bool ready { get; private set; } = false;

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
        public bool isSimulateSloped = true;

        /// <summary>
        /// 傾斜であるか
        /// </summary>
        public bool sloped{ get {return impl.sloped;} internal set {impl.sloped = value;} }

        /// <summary>
        /// 衝突が検出されたか
        /// </summary>
        public bool collisionDetected{ get {return impl.collisionDetected;} internal set {impl.collisionDetected = value;} }


        // ======== Objects ========
        private Rigidbody rb;
        private AudioSource audioSource;
        private GameObject cubeModel;
        private GameObject LED;
        private BoxCollider col;

        private CubeSimImpl impl;



        private void Start()
        {
            #if !UNITY_EDITOR   // Editor以外で実行される場合は自身を無効かします
                this.gameObject.SetActive(false);
            #else
                this.rb = GetComponent<Rigidbody>();
                this.rb.maxAngularVelocity = 21f;
                this.audioSource = GetComponent<AudioSource>();
                this.LED = transform.Find("LED").gameObject;
                this.LED.GetComponent<Renderer>().material.color = Color.black;
                this.cubeModel = transform.Find("cube_model").gameObject;
                this.col = GetComponent<BoxCollider>();

                switch (version)
                {
                    case Version.v2_0_0 : this.impl = new CubeSimImpl_v2_0_0(this);break;
                    default : this.impl = new CubeSimImpl_v2_0_0(this);break;
                }
                this._InitPresetSounds();
            #endif
        }

        private void Update()
        {
        }

        private void FixedUpdate()
        {
            SimulatePhysics();

            impl.Simulate();

            this.ready = true;  // 一回更新してからシミュレーターがreadyになる
        }

        internal bool offGroundL = true;
        internal bool offGroundR = true;
        private void SimulatePhysics()
        {
            // タイヤの着地状態を調査
            // Check if tires are Off Ground
            RaycastHit hit;
            var ray = new Ray(transform.position+transform.up*0.001f-transform.right*0.0133f, -transform.up); // left wheel
            if (Physics.Raycast(ray, out hit) && hit.distance < 0.002f) offGroundL = false;
            ray = new Ray(transform.position+transform.up*0.001f+transform.right*0.0133f, -transform.up); // right wheel
            if (Physics.Raycast(ray, out hit) && hit.distance < 0.002f) offGroundR = false;
        }



        // ============ Event ============

        // ------------ v2.0.0 ------------

        /// <summary>
        /// ボタンのイベントコールバックを設定する
        /// </summary>
        public void StartNotification_Button(System.Action<bool> action)
        {
            impl.StartNotification_Button(action);
        }

        /// <summary>
        /// Standard ID のイベントコールバックを設定する
        /// </summary>
        public void StartNotification_StandardID(System.Action<uint, int> action)
        {
            impl.StartNotification_StandardID(action);
        }

        /// <summary>
        /// Standard ID から持ち上げられた時のイベントコールバックを設定する
        /// </summary>
        public void StartNotification_StandardIDMissed(System.Action action)
        {
            impl.StartNotification_StandardIDMissed(action);
        }

        /// <summary>
        /// Position ID （マット座標）のイベントコールバックを設定する
        /// </summary>
        public void StartNotification_PositionID(System.Action<int, int, int, int, int> action)
        {
            impl.StartNotification_PositionID(action);
        }

        /// <summary>
        /// マットから持ち上げられた時のイベントコールバックを設定する
        /// </summary>
        public void StartNotification_PositionIDMissed(System.Action action)
        {
            impl.StartNotification_PositionIDMissed(action);
        }

        /// <summary>
        /// 水平検出のイベントコールバックを設定する
        /// </summary>
        public void StartNotification_Sloped(System.Action<bool> action)
        {
            impl.StartNotification_Sloped(action);
        }

        /// <summary>
        /// 衝突検出のイベントコールバックを設定する
        /// </summary>
        public void StartNotification_CollisionDetected(System.Action<bool> action)
        {
            impl.StartNotification_CollisionDetected(action);
        }

        // ============ コマンド ============

        /// <summary>
        /// モーター：時間指定付きモーター制御
        /// </summary>
        public void Move(int left, int right, int durationMS)
        {
            impl.Move(left, right, durationMS);
        }

        /// <summary>
        /// ランプ：消灯
        /// </summary>
        public void StopLight()
        {
            impl.StopLight();
        }
        /// <summary>
        /// ランプ：点灯
        /// </summary>
        public void SetLight(int r, int g, int b, int durationMS)
        {
            impl.SetLight(r, g, b, durationMS);
        }
        /// <summary>
        /// ランプ：連続的な点灯・消灯
        /// </summary>
        public void SetLights(int repeatCount, Cube.LightOperation[] operations)
        {
            impl.SetLights(repeatCount, operations);
        }

        /// <summary>
        /// サウンド：MIDI note number の再生
        /// </summary>
        public void PlaySound(int repeatCount, Cube.SoundOperation[] operations)
        {
            impl.PlaySound(repeatCount, operations);
        }
        /// <summary>
        /// サウンド：効果音の再生 （未実装）
        /// </summary>
        public void PlayPresetSound(int soundId, int volume)
        {
            impl.PlayPresetSound(soundId, volume);
        }
        /// <summary>
        /// サウンド：再生の停止
        /// </summary>
        public void StopSound()
        {
            impl.StopSound();
        }

        /// <summary>
        /// 水平検出の閾値を設定する（度）
        /// </summary>
        public void SetSlopeThreshold(int degree)
        {
            impl.SetSlopeThreshold(degree);
        }


        // ====== 内部関数 ======

        // 速度変化によって力を与え、位置と角度を更新
        internal void _SetSpeed(float speedL, float speedR)
        {
            this.rb.angularVelocity = transform.up * (float)((speedL - speedR) / TireWidthM);
            var vel = transform.forward * (speedL + speedR) / 2;
            var dv = vel - this.rb.velocity;
            this.rb.AddForce(dv, ForceMode.VelocityChange);
        }
        internal void _SetLight(int r, int g, int b){
            r = Mathf.Clamp(r, 0, 255);
            g = Mathf.Clamp(g, 0, 255);
            b = Mathf.Clamp(b, 0, 255);
            LED.GetComponent<Renderer>().material.color = new Color(r/255f, g/255f, b/255f);
        }

        internal void _StopLight(){
            LED.GetComponent<Renderer>().material.color = Color.black;
        }

        internal void _PlaySound(int soundId, int volume){
            int octave = (int)(soundId/12);
            int idx = (int)(soundId%12);
            var aCubeOnSlot = Resources.Load("Octave/" + (octave*12+9)) as AudioClip;
            audioSource.volume = (float)volume/256;
            audioSource.pitch = (float)Math.Pow(2, ((float)idx-9)/12);
            audioSource.clip = aCubeOnSlot;
            audioSource.Play();
        }
        internal void _StopSound(){
            audioSource.clip = null;
            audioSource.Stop();
        }

        // Sound Preset を設定
        internal void _InitPresetSounds(){
            Cube.SoundOperation[] sounds = new Cube.SoundOperation[3];
            sounds[0] = new Cube.SoundOperation(200, 255, 48);
            sounds[1] = new Cube.SoundOperation(200, 255, 50);
            sounds[2] = new Cube.SoundOperation(200, 255, 52);
            impl.presetSounds.Add(sounds);
        }

        internal void _SetPressed(bool pressed)
        {
            this.cubeModel.transform.localEulerAngles
                    = pressed? new Vector3(-93,0,0) : new Vector3(-90,0,0);
        }



    }

}