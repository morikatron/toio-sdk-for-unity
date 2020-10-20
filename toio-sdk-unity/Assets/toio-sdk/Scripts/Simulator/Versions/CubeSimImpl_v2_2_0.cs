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
                    this.shakeTapCallback?.Invoke(value);
                }
                this._shake = value;
            }
        }
        protected System.Action<bool> shakeTapCallback = null;
        public override void StartNotification_Shake(System.Action<bool> action)
        {
            this.shakeTapCallback = action;
            this.shakeTapCallback.Invoke(_shake);
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

    }
}