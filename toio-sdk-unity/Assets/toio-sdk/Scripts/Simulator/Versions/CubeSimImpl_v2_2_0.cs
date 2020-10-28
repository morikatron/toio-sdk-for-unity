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
        // ---------- Shake -----------
        protected bool _shake;
        public override bool shake
        {
            get {return this._shake;}
            internal set
            {
                if (this._shake!=value){
                    this.shakeCallback?.Invoke(value);
                }
                this._shake = value;
            }
        }
        protected System.Action<bool> shakeCallback = null;
        public override void StartNotification_Shake(System.Action<bool> action)
        {
            this.shakeCallback = action;
            this.shakeCallback.Invoke(_shake);
        }
        protected virtual void SimulateShake()
        {
            // Not Implemented
        }


        // ---------- Motor Speed -----------
        protected bool motorSpeedEnabled = false;
        public override int leftMotorSpeed {get; protected set;}
        public override int rightMotorSpeed {get; protected set;}
        protected System.Action<int, int> motorSpeedCallback = null;
        public override void EnableMotorSpeed(bool enabled)
        {
            this.motorSpeedEnabled = enabled;
        }
        public override void StartNotification_MotorSpeed(System.Action<int, int> action)
        {
            this.motorSpeedCallback = action;
            if (motorSpeedEnabled)
                this.motorSpeedCallback.Invoke(leftMotorSpeed, rightMotorSpeed);
        }

        protected void _SetMotorSpeed(int left, int right)
        {
            if (motorSpeedEnabled)
                if (this.leftMotorSpeed != left || this.rightMotorSpeed != right)
                    this.motorSpeedCallback?.Invoke(left, right);
            this.leftMotorSpeed = left;
            this.rightMotorSpeed = right;
        }
        protected void SimulateMotorSpeedSensor()
        {
            int left = Mathf.RoundToInt(speedTireL/CubeSimulator.VMeterOverU);
            int right = Mathf.RoundToInt(speedTireR/CubeSimulator.VMeterOverU);
            _SetMotorSpeed(left, right);
        }


        // ----------- Simulate -----------
        protected override void SimulateMotionSensor()
        {
            base.SimulateMotionSensor();

            SimulateShake();
            SimulateMotorSpeedSensor();
        }

    }
}