using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace toio.Simulator
{

    internal class CubeSimImpl_v2_1_0 : CubeSimImpl_v2_0_0
    {
        public override int maxMotor{get;} = 115;
        public override int deadzone{get;} = 8;
        public  CubeSimImpl_v2_1_0(CubeSimulator cube) : base(cube){}


        // ============ Motion Sensor ============
        // ---------- Pose -----------
        protected Cube.PoseType _pose = Cube.PoseType.up;
        public override Cube.PoseType pose {
            get{ return _pose; }
            internal set{
                if (this._pose!=value){
                    this.poseCallback?.Invoke(value);
                }
                _pose = value;
            }
        }

        protected System.Action<Cube.PoseType> poseCallback = null;
        public override void StartNotification_Pose(System.Action<Cube.PoseType> action)
        {
            this.poseCallback = action;
            this.poseCallback.Invoke(_pose);
        }
        protected virtual void SimulatePose()
        {
            if(Vector3.Angle(Vector3.up, cube.transform.up)<slopeThreshold)
            {
                this.pose = Cube.PoseType.up;
            }
            else if(Vector3.Angle(Vector3.up, cube.transform.up)>180-slopeThreshold)
            {
                this.pose = Cube.PoseType.down;
            }
            else if(Vector3.Angle(Vector3.up, cube.transform.forward)<slopeThreshold)
            {
                this.pose = Cube.PoseType.forward;
            }
            else if(Vector3.Angle(Vector3.up, cube.transform.forward)>180-slopeThreshold)
            {
                this.pose = Cube.PoseType.backward;
            }
            else if(Vector3.Angle(Vector3.up, cube.transform.right)<slopeThreshold)
            {
                this.pose = Cube.PoseType.right;
            }
            else if(Vector3.Angle(Vector3.up, cube.transform.right)>180-slopeThreshold)
            {
                this.pose = Cube.PoseType.left;
            }
        }

        // ---------- Double Tap -----------
        protected bool _doubleTap;
        public override bool doubleTap
        {
            get {return this._doubleTap;}
            internal set
            {
                if (this._doubleTap!=value){
                    this.doubleTapCallback?.Invoke(value);
                }
                this._doubleTap = value;
            }
        }
        protected System.Action<bool> doubleTapCallback = null;
        public override void StartNotification_DoubleTap(System.Action<bool> action)
        {
            this.doubleTapCallback = action;
            this.doubleTapCallback.Invoke(_doubleTap);
        }
        protected virtual void SimulateDoubleTap()
        {
            // Not Implemented
        }


        // ----------- Simulate -----------
        protected override void SimulateMotionSensor()
        {
            base.SimulateMotionSensor();

            // 姿勢検出
            SimulatePose();
            SimulateDoubleTap();
        }

    }
}