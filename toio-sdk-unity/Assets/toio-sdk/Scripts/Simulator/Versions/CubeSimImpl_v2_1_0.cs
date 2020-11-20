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


        // ============ Motor ============

        protected override void MotorScheduler(float dt, float t)
        {
            motorCmdElipsed += dt;

            string currCmd = "";
            float latestRecvTime = 0;

            while (motorTimeCmdQ.Count>0 && t > motorTimeCmdQ.Peek().tRecv + cube.delay)
            {
                motorCmdElipsed = 0;
                currMotorTimeCmd = motorTimeCmdQ.Dequeue();
                currCmd = "Time"; latestRecvTime = currMotorTimeCmd.tRecv;
            }
            while (motorTargetCmdQ.Count>0 && t > motorTargetCmdQ.Peek().tRecv + cube.delay)
            {
                motorCmdElipsed = 0;
                currMotorTargetCmd = motorTargetCmdQ.Dequeue();
                if (currMotorTargetCmd.tRecv > latestRecvTime)
                {
                    currCmd = "Target"; latestRecvTime = currMotorTargetCmd.tRecv;
                }
            }
            while (motorMultiTargetCmdQ.Count>0 && t > motorMultiTargetCmdQ.Peek().tRecv + cube.delay)
            {
                motorCmdElipsed = 0;
                currMotorMultiTargetCmd = motorMultiTargetCmdQ.Dequeue();
                if (currMotorMultiTargetCmd.tRecv > latestRecvTime)
                {
                    currCmd = "MultiTarget"; latestRecvTime = currMotorMultiTargetCmd.tRecv;
                }
            }
            while (motorAccCmdQ.Count>0 && t > motorAccCmdQ.Peek().tRecv + cube.delay)
            {
                motorCmdElipsed = 0;
                currMotorAccCmd = motorAccCmdQ.Dequeue();
                if (currMotorAccCmd.tRecv > latestRecvTime)
                {
                    currCmd = "Acc"; latestRecvTime = currMotorAccCmd.tRecv;
                }
            }

            // ----- Excute Order -----
            switch (currCmd)
            {
                case "Time":
                {
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
                    break;
                }
                case "Target":
                {
                    TargetMoveController();
                    break;
                }
                case "MultiTarget":
                {
                    MultiTargetMoveController();
                    break;
                }
                case "Acc":
                {
                    AccMoveController();
                    break;
                }
            }

        }


        // -------- Target Move --------
        protected struct MotorTargetCmd
        {
            public ushort x, y, deg;
            public Cube.TargetMoveConfig config;
            public float tRecv;
        }
        protected Queue<MotorTargetCmd> motorTargetCmdQ = new Queue<MotorTargetCmd>(); // command queue
        protected MotorTargetCmd currMotorTargetCmd = default;  // current command

        protected virtual void TargetMoveController()
        {

        }

        // COMMAND API
        public override void TargetMove(int targetX, int targetY, int targetAngle, Cube.TargetMoveConfig config)
        {
            MotorTargetCmd cmd = new MotorTargetCmd();
            cmd.x = (ushort)targetX;
            cmd.y = (ushort)targetY;
            cmd.deg = (ushort)targetAngle;
            cmd.config = config;
            cmd.tRecv = Time.time;
            motorTargetCmdQ.Enqueue(cmd);
        }


        // -------- Multi Target Move --------
        protected struct MotorMultiTargetCmd
        {
            public ushort[] xs, ys, degs;
            public Cube.MultiMoveConfig config;
            public float tRecv;
        }
        protected Queue<MotorMultiTargetCmd> motorMultiTargetCmdQ = new Queue<MotorMultiTargetCmd>(); // command queue
        protected MotorMultiTargetCmd currMotorMultiTargetCmd = default;  // current command

        protected virtual void MultiTargetMoveController()
        {

        }

        // COMMAND API
        public override void MultiTargetMove(int[] targetXList, int[] targetYList, int[] targetAngleList, Cube.MultiMoveConfig config)
        {
            MotorMultiTargetCmd cmd = new MotorMultiTargetCmd();
            cmd.xs = Array.ConvertAll(targetXList, new Converter<int, ushort>(x=>(ushort)x));
            cmd.ys = Array.ConvertAll(targetYList, new Converter<int, ushort>(x=>(ushort)x));
            cmd.degs = Array.ConvertAll(targetAngleList, new Converter<int, ushort>(x=>(ushort)x));
            cmd.config = config;
            cmd.tRecv = Time.time;
            motorMultiTargetCmdQ.Enqueue(cmd);
        }


        // -------- Acceleration Move --------
        protected struct MotorAccCmd
        {
            public byte spd, acc;
            public Cube.AccMoveConfig config;
            public float tRecv;
        }
        protected Queue<MotorAccCmd> motorAccCmdQ = new Queue<MotorAccCmd>(); // command queue
        protected MotorAccCmd currMotorAccCmd = default;  // current command

        protected virtual void AccMoveController()
        {

        }

        // COMMAND API
        public override void AccelerationMove(int targetSpeed, int acceleration, Cube.AccMoveConfig config)
        {
            MotorAccCmd cmd = new MotorAccCmd();
            cmd.spd = (byte)targetSpeed;
            cmd.acc = (byte)acceleration;
            cmd.config = config;
            cmd.tRecv = Time.time;
            motorAccCmdQ.Enqueue(cmd);
        }



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
                this.pose = Cube.PoseType.front;
            }
            else if(Vector3.Angle(Vector3.up, cube.transform.forward)>180-slopeThreshold)
            {
                this.pose = Cube.PoseType.back;
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