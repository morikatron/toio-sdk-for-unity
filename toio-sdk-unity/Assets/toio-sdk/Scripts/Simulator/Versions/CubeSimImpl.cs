using System.Collections.Generic;
using UnityEngine;


namespace toio.Simulator
{
    internal class CubeSimImpl
    {
        public virtual int maxMotor{get;}
        public virtual int deadzone{get;}
        public List<Cube.SoundOperation[]> presetSounds = new List<Cube.SoundOperation[]>();


        protected CubeSimulator cube;
        public CubeSimImpl(CubeSimulator cube)
        {
            this.cube = cube;
        }


        // ============ Simulate ============
        public virtual void Simulate(){
            SimulateMotor();
        }

        public virtual void Init(){

        }

        public virtual void Reset(){
            ResetMotor();
        }


        // ============ toio ID ============
        public virtual int x { get; internal set; }
        public virtual int y { get; internal set; }
        public virtual int deg { get; internal set; }
        public virtual int xSensor { get; internal set; }
        public virtual int ySensor { get; internal set; }
        public virtual uint standardID { get; internal set; }
        public virtual bool onMat { get; internal set; } = false;
        public virtual bool onStandardID { get; internal set; } = false;
        public virtual void StartNotification_StandardID(System.Action<uint, int> action)
        { NotSupportedWarning(); }
        public virtual void StartNotification_StandardIDMissed(System.Action action)
        { NotSupportedWarning(); }
        public virtual void StartNotification_PositionID(System.Action<int, int, int, int, int> action)
        { NotSupportedWarning(); }
        public virtual void StartNotification_PositionIDMissed(System.Action action)
        { NotSupportedWarning(); }


        // ============ Button ============
        public virtual bool button {
            get{ NotSupportedWarning(); return default; }
            internal set{ NotSupportedWarning(); }}
        public virtual void StartNotification_Button(System.Action<bool> action)
        { NotSupportedWarning(); }


        // ============ Motion Sensor ============
        public virtual void StartNotification_MotionSensor(System.Action<object[]> action)
        { NotSupportedWarning(); }

        // ---------- 2.0.0 ----------
        // Sloped
        public virtual bool sloped {
            get{ NotSupportedWarning(); return default; }
            internal set{ NotSupportedWarning(); }}

        // Collision Detected
        internal virtual void TriggerCollision()
        { NotSupportedWarning(); }
        // ---------- 2.1.0 ----------
        // Pose
        public virtual Cube.PoseType pose {
            get{ NotSupportedWarning(); return default; }
            internal set{ NotSupportedWarning(); }}
        // Double Tap
        internal virtual void TriggerDoubleTap()
        { NotSupportedWarning(); }

        // ---------- 2.2.0 ----------
        // Shake
        public virtual int shakeLevel {
            get{ NotSupportedWarning(); return default; }
            internal set{ NotSupportedWarning(); }}


        // ============ Motor ============
        protected enum MotorCmdType : byte
        {
            None, MotorTimeCmd, MotorTargetCmd, MotorMultiTargetCmd, MotorAccCmd
        }
        protected MotorCmdType motorCmdType = MotorCmdType.None;

        protected float motorCmdL {get; set;} = 0;   // モーター指令値
        protected float motorCmdR {get; set;} = 0;
        public virtual void SimulateMotor()
        {
            float targetSpeedL = motorCmdL * CubeSimulator.VDotOverU / Mat.DotPerM;
            float targetSpeedR = motorCmdR * CubeSimulator.VDotOverU / Mat.DotPerM;

            cube.SetMotorTargetSpd(targetSpeedL, targetSpeedR);
        }
        protected virtual void ResetMotor()
        {
            motorCmdL = 0; motorCmdR = 0;
        }

        // ---------- 2.1.0 ----------
        // Target Move
        public virtual void StartNotification_TargetMove(System.Action<int, Cube.TargetMoveRespondType> action)
        { NotSupportedWarning(); }
        // Multi Target Move
        public virtual void StartNotification_MultiTargetMove(System.Action<int, Cube.TargetMoveRespondType> action)
        { NotSupportedWarning(); }

        // ---------- 2.2.0 ----------
        // Motor Speed
        public virtual int leftMotorSpeed {
            get{ NotSupportedWarning(); return default; }
            protected set{ NotSupportedWarning(); }}
        public virtual int rightMotorSpeed {
            get{ NotSupportedWarning(); return default; }
            protected set{ NotSupportedWarning(); }}
        public virtual void StartNotification_MotorSpeed(System.Action<int, int> action)
        { NotSupportedWarning(); }


        // ============ Magnetic ============
        // ---------- 2.2.0 ----------
        // Magnet State
        public virtual Cube.MagnetState magnetState {
            get{ NotSupportedWarning(); return default; }
            protected set{ NotSupportedWarning(); }}
        public virtual void StartNotification_MagnetState(System.Action<Cube.MagnetState> action)
        { NotSupportedWarning(); }

        // ---------- 2.3.0 ----------
        // Magnetic Force
        public virtual void StartNotification_Attitude(System.Action<Vector3> actionE, System.Action<Quaternion> actionQ)
        { NotSupportedWarning(); }

        // ============ Light ============
        protected enum LightCmdType : byte
        {
            None, LightCmd, LightSenarioCmd
        }
        protected LightCmdType lightCmdType = LightCmdType.None;

        // ============ Sound ============
        protected enum SoundCmdType : byte
        {
            None, SoundSenarioCmd
        }
        protected SoundCmdType soundCmdType = SoundCmdType.None;

        // ============ Attitude ============
        // ---------- 2.3.0 ----------
        public virtual Vector3 magneticForce {
            get{ NotSupportedWarning(); return default; }
            protected set{ NotSupportedWarning(); }}
        public virtual void StartNotification_MagneticForce(System.Action<Vector3> action)
        { NotSupportedWarning(); }


        // ============ Config ============
        public virtual void StartNotification_ConfigMotorRead(System.Action<bool> action)
        { NotSupportedWarning(); }
        public virtual void StartNotification_ConfigIDNotification(System.Action<bool> action)
        { NotSupportedWarning(); }
        public virtual void StartNotification_ConfigIDMissedNotification(System.Action<bool> action)
        { NotSupportedWarning(); }
        public virtual void StartNotification_ConfigMagneticSensor(System.Action<bool> action)
        { NotSupportedWarning(); }
        public virtual void StartNotification_ConfigAttitudeSensor(System.Action<bool> action)
        { NotSupportedWarning(); }


        // ============ Commands ============
        // ---------- 2.0.0 ----------
        public virtual void Move(int left, int right, int durationMS)
        { NotSupportedWarning(); }
        public virtual void StopLight()
        { NotSupportedWarning(); }
        public virtual void SetLight(int r, int g, int b, int durationMS)
        { NotSupportedWarning(); }
        public virtual void SetLights(int repeatCount, Cube.LightOperation[] operations)
        { NotSupportedWarning(); }
        public virtual void PlaySound(int repeatCount, Cube.SoundOperation[] operations)
        { NotSupportedWarning(); }
        public virtual void PlayPresetSound(int soundId, int volume)
        { NotSupportedWarning(); }
        internal virtual void PlaySound_Connect()
        { NotSupportedWarning(); }
        internal virtual void PlaySound_Disconnect()
        { NotSupportedWarning(); }
        internal virtual void PlaySound_PowerOn()
        { NotSupportedWarning(); }
        internal virtual void PlaySound_PowerOff()
        { NotSupportedWarning(); }
        public virtual void StopSound()
        { NotSupportedWarning(); }
        public virtual void ConfigSlopeThreshold(int angle)
        { NotSupportedWarning(); }

        // ---------- 2.1.0 ----------
        public virtual void TargetMove(
            int targetX,
            int targetY,
            int targetAngle,
            int configID,
            int timeOut,
            Cube.TargetMoveType targetMoveType,
            int maxSpd,
            Cube.TargetSpeedType targetSpeedType,
            Cube.TargetRotationType targetRotationType
        ){ NotSupportedWarning(); }
        public virtual void MultiTargetMove(
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
        ){ NotSupportedWarning(); }
        public virtual void AccelerationMove(
            int targetSpeed,
            int acceleration,
            int rotationSpeed,
            Cube.AccPriorityType accPriorityType,
            int controlTime
        ){ NotSupportedWarning(); }

        // ---------- 2.2.0 ----------
        public virtual void ConfigMotorRead(bool enabled)
        { NotSupportedWarning(); }

        public virtual void ConfigIDNotification(int interval, Cube.IDNotificationType notificationType)
        { NotSupportedWarning(); }

        public virtual void ConfigIDMissedNotification(int sensitivity)
        { NotSupportedWarning(); }

        public virtual void ConfigMagneticSensor(Cube.MagneticMode mode)
        { NotSupportedWarning(); }
        public virtual void RequestMotionSensor()
        { NotSupportedWarning(); }

        public virtual void RequestMagneticSensor()
        { NotSupportedWarning(); }

        // ---------- 2.3.0 ----------
        public virtual void ConfigMagneticSensor(Cube.MagneticMode mode, int interval, Cube.MagneticNotificationType notificationType)
        { NotSupportedWarning(); }
        public virtual void ConfigAttitudeSensor(Cube.AttitudeFormat format, int interval, Cube.AttitudeNotificationType notificationType)
        { NotSupportedWarning(); }

        public virtual void RequestAttitudeSensor(Cube.AttitudeFormat format)
        { NotSupportedWarning(); }



        // ============ Utils ============
        protected float Deg(float d)
        {
            return (d%360 + 540)%360 -180;
        }
        protected virtual void NotSupportedWarning()
        {
            // Debug.LogWarning("Not Supported in this BLE protocol version.");
        }

    }
}