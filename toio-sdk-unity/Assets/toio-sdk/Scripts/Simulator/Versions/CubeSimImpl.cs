using System;
using System.Collections.Generic;
using System.Linq;
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
        // ---------- 2.0.0 ----------
        // Sloped
        public virtual bool sloped {
            get{ NotSupportedWarning(); return default; }
            internal set{ NotSupportedWarning(); }}
        public virtual void StartNotification_Sloped(System.Action<bool> action)
        { NotSupportedWarning(); }

        // Collision Detected
        public virtual bool collisionDetected {
            get{ NotSupportedWarning(); return default; }
            internal set{ NotSupportedWarning(); }}
        public virtual void StartNotification_CollisionDetected(System.Action<bool> action)
        { NotSupportedWarning(); }
        // ---------- 2.1.0 ----------
        // Pose
        public virtual Cube.PoseType pose {
            get{ NotSupportedWarning(); return default; }
            internal set{ NotSupportedWarning(); }}
        public virtual void StartNotification_Pose(System.Action<Cube.PoseType> action)
        { NotSupportedWarning(); }
        // Double Tap
        public virtual bool doubleTap {
            get{ NotSupportedWarning(); return default; }
            internal set{ NotSupportedWarning(); }}
        public virtual void StartNotification_DoubleTap(System.Action<bool> action)
        { NotSupportedWarning(); }
        // ---------- 2.2.0 ----------
        // Shake
        public virtual bool shake {
            get{ NotSupportedWarning(); return default; }
            internal set{ NotSupportedWarning(); }}
        public virtual void StartNotification_Shake(System.Action<bool> action)
        { NotSupportedWarning(); }
        // Motor Speed
        public virtual int leftMotorSpeed {
            get{ NotSupportedWarning(); return default; }
            protected set{ NotSupportedWarning(); }}
        public virtual int rightMotorSpeed {
            get{ NotSupportedWarning(); return default; }
            protected set{ NotSupportedWarning(); }}
        public virtual void StartNotification_MotorSpeed(System.Action<int, int> action)
        { NotSupportedWarning(); }
        public virtual void StartNotification_ConfigMotorRead(System.Action<bool> action)
        { NotSupportedWarning(); }



        // ============ Motor ============
        protected float speedL = 0;  // (m/s)
        protected float speedR = 0;
        protected float speedTireL = 0;
        protected float speedTireR = 0;
        protected float motorLeft{get; set;} = 0;   // モーター指令値
        protected float motorRight{get; set;} = 0;
        public virtual void SimulateMotor()
        {
            var dt = Time.deltaTime;

            // 目標速度を計算
            // target speed
            float targetSpeedL = motorLeft * CubeSimulator.VDotOverU / Mat.DotPerM;
            float targetSpeedR = motorRight * CubeSimulator.VDotOverU / Mat.DotPerM;
            // if (Mathf.Abs(motorLeft) < deadzone) targetSpeedL = 0;
            // if (Mathf.Abs(motorRight) < deadzone) targetSpeedR = 0;

            // 速度更新
            // update tires' speed
            if (cube.forceStop || this.button)   // 強制的に停止
            {
                speedTireL = 0; speedTireR = 0;
            }
            else
            {
                speedTireL += (targetSpeedL - speedTireL) / Mathf.Max(cube.motorTau,dt) * dt;
                speedTireR += (targetSpeedR - speedTireR) / Mathf.Max(cube.motorTau,dt) * dt;
            }

            // update object's speed
            // NOTES: simulation for slipping shall be implemented here
            speedL = cube.offGroundL? 0: speedTireL;
            speedR = cube.offGroundR? 0: speedTireR;

            cube._SetSpeed(speedL, speedR);
        }



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
        public virtual void StopSound()
        { NotSupportedWarning(); }
        public virtual void ConfigSlopeThreshold(int angle)
        { NotSupportedWarning(); }

        // ---------- 2.1.0 ----------
        public virtual void TargetMove(
            int targetX,
            int targetY,
            int targetAngle,
            byte configID,
            byte timeOut,
            Cube.TargetMoveType targetMoveType,
            byte maxSpd,
            Cube.TargetSpeedType targetSpeedType,
            Cube.TargetRotationType targetRotationType
        ){ NotSupportedWarning(); }
        public virtual void MultiTargetMove(
            int[] targetXList,
            int[] targetYList,
            int[] targetAngleList,
            Cube.TargetRotationType[] multiRotationTypeList,
            byte configID,
            byte timeOut,
            Cube.TargetMoveType targetMoveType,
            byte maxSpd,
            Cube.TargetSpeedType targetSpeedType,
            Cube.MultiWriteType multiWriteType
        ){ NotSupportedWarning(); }
        public virtual void AccelerationMove(
            int targetSpeed,
            int acceleration,
            ushort rotationSpeed,
            Cube.AccRotationType accRotationType,
            Cube.AccMoveType accMoveType,
            Cube.AccPriorityType accPriorityType,
            byte controlTime
        ){ NotSupportedWarning(); }

        // ---------- 2.2.0 ----------
        public virtual void ConfigMotorRead(bool enabled)
        { NotSupportedWarning(); }

        protected virtual void NotSupportedWarning()
        {
            // Debug.LogWarning("Not Supported in this firmware version.");
        }
    }
}