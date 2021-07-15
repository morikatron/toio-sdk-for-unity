using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace toio.Simulator
{

    internal class CubeSimImpl_v2_2_0 : CubeSimImpl_v2_1_0
    {
        public  CubeSimImpl_v2_2_0(CubeSimulator cube) : base(cube){}


        // ============ Motion Sensor ============
        protected override void InvokeMotionSensorCallback()
        {
            if (this.motionSensorCallback == null) return;
            object[] sensors = new object[6];
            sensors[0] = null;
            sensors[1] = (object)this._sloped;
            sensors[2] = (object)this._collisonDetected; this._collisonDetected = false;
            sensors[3] = (object)this._doubleTapped; this._doubleTapped = false;
            sensors[4] = (object)this._pose;
            sensors[5] = (object)this._shakeLevel;
            this.motionSensorCallback.Invoke(sensors);
        }

        // ---------- Shake -----------
        protected int _shakeLevel = 0;
        public override int shakeLevel
        {
            get {return this._shakeLevel;}
            internal set
            {
                if (this._shakeLevel != value){
                    this._shakeLevel = value;
                    this.InvokeMotionSensorCallback();
                }
            }
        }
        protected virtual void SimulateShake()
        {
            // Not Implemented
        }

        // ----------- Simulate -----------
        protected override void SimulateMotionSensor()
        {
            base.SimulateMotionSensor();

            SimulateShake();
        }

        protected override void ResetMotionSensor()
        {
            base.ResetMotionSensor();

            _shakeLevel = 0;
        }

        // ---------- Request Sensor -----------
        public override void RequestSensor()
        {
            this.InvokeMotionSensorCallback();
        }



        // ============ Motor ============

        // ---------- Motor Speed -----------
        protected bool motorSpeedEnabled = false;
        public override int leftMotorSpeed {get; protected set;}
        public override int rightMotorSpeed {get; protected set;}
        protected System.Action<int, int> motorSpeedCallback = null;
        public override void StartNotification_MotorSpeed(System.Action<int, int> action)
        {
            this.motorSpeedCallback = action;
            if (this.motorSpeedEnabled)
                this.motorSpeedCallback?.Invoke(leftMotorSpeed, rightMotorSpeed);
        }

        protected void _SetMotorSpeed(int left, int right)
        {
            left = Mathf.Abs(left);
            right = Mathf.Abs(right);
            if (motorSpeedEnabled)
                if (this.leftMotorSpeed != left || this.rightMotorSpeed != right)
                    this.motorSpeedCallback?.Invoke(left, right);
            this.leftMotorSpeed = left;
            this.rightMotorSpeed = right;
        }

        // ----------- Simulate -----------
        protected void SimulateMotorSpeedSensor()
        {
            int left = Mathf.RoundToInt(cube.speedTireL/CubeSimulator.VMeterOverU);
            int right = Mathf.RoundToInt(cube.speedTireR/CubeSimulator.VMeterOverU);
            _SetMotorSpeed(left, right);
        }

        protected override void ResetMotor()
        {
            base.ResetMotor();

            configMotorReadCallback = null;
            motorSpeedEnabled = false;
            leftMotorSpeed = 0; rightMotorSpeed = 0;
        }

        // ---------- Config -----------
        protected System.Action<bool> configMotorReadCallback = null;
        public override void StartNotification_ConfigMotorRead(System.Action<bool> action)
        {
            this.configMotorReadCallback = action;
        }
        public override void ConfigMotorRead(bool enabled)
        {
            this.motorSpeedEnabled = enabled;
            this.configMotorReadCallback?.Invoke(enabled);  // TODO probably not same as REAL
            // this.motorReadCallback?.Invoke(leftMotorSpeed, rightMotorSpeed);
        }


        // ============ Simulate ============
        public override void Simulate()
        {
            SimulateIDSensor();
            SimulateMotionSensor();
            SimulateMotorSpeedSensor();

            float currentTime = Time.fixedTime;
            MotorScheduler(currentTime);
            LightScheduler(currentTime);
            SoundScheduler(currentTime);
            SimulateMotor();
        }

    }
}