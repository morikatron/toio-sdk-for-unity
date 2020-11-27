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

        protected string motorCurrentCmdType = "";
        protected override void MotorScheduler(float dt, float t)
        {
            motorCmdElipsed += dt;

            float latestRecvTime = 0;
            bool newCmd = false;
            string oldCmdType = motorCurrentCmdType;
            byte oldConfigID = 0;
            bool overwriteMulti = false;
            switch (motorCurrentCmdType)
            {
                case "Target": { oldConfigID = currMotorTargetCmd.configID; break; }
                case "MultiTarget": { oldConfigID = currMotorMultiTargetCmd.configID; break; }
                case "Acc": { oldConfigID = currMotorAccCmd.configID; break; }
            }
            MotorMultiTargetCmd multiCmdTemp = default;


            // ----- Dequeue Commands -----
            while (motorTimeCmdQ.Count>0 && t > motorTimeCmdQ.Peek().tRecv + cube.delay)
            {
                motorCmdElipsed = 0;
                currMotorTimeCmd = motorTimeCmdQ.Dequeue();
                motorCurrentCmdType = "Time"; latestRecvTime = currMotorTimeCmd.tRecv;
                newCmd = true;
                overwriteMulti = true;
            }
            while (motorTargetCmdQ.Count>0 && t > motorTargetCmdQ.Peek().tRecv + cube.delay)
            {
                motorCmdElipsed = 0;
                currMotorTargetCmd = motorTargetCmdQ.Dequeue();
                if (currMotorTargetCmd.tRecv > latestRecvTime)
                {
                    motorCurrentCmdType = "Target"; latestRecvTime = currMotorTargetCmd.tRecv;
                    newCmd = true;
                }
                overwriteMulti = true;
            }
            while (motorMultiTargetCmdQ.Count>0 && t > motorMultiTargetCmdQ.Peek().tRecv + cube.delay)
            {
                motorCmdElipsed = 0;
                multiCmdTemp = motorMultiTargetCmdQ.Dequeue();
                if (multiCmdTemp.tRecv > latestRecvTime)
                {
                    motorCurrentCmdType = "MultiTarget"; latestRecvTime = multiCmdTemp.tRecv;
                    newCmd = true;
                }
            }
            while (motorAccCmdQ.Count>0 && t > motorAccCmdQ.Peek().tRecv + cube.delay)
            {
                motorCmdElipsed = 0;
                currMotorAccCmd = motorAccCmdQ.Dequeue();
                if (currMotorAccCmd.tRecv > latestRecvTime)
                {
                    motorCurrentCmdType = "Acc"; latestRecvTime = currMotorAccCmd.tRecv;
                    newCmd = true;
                }
                overwriteMulti = true;
            }

            // ----- Target -----
            if (newCmd && oldCmdType == "Target")
                this.targetMoveCallback?.Invoke(oldConfigID, Cube.TargetMoveRespondType.OtherWrite);
            if (newCmd && motorCurrentCmdType == "Target")
                TargetMoveInit();

            // ----- Multi Target -----
            if (overwriteMulti) hasNextMotorMultiTargetCmd = false;
            if (oldCmdType == "MultiTarget" && overwriteMulti)
            {
                this.multiTargetMoveCallback?.Invoke(oldConfigID, Cube.TargetMoveRespondType.OtherWrite);
                if (motorCurrentCmdType=="MultiTarget" && newCmd)
                {
                    currMotorMultiTargetCmd = multiCmdTemp;
                    MultiTargetMoveInit();
                }
            }
            else if (oldCmdType != "MultiTarget")
            {
                if (motorCurrentCmdType=="MultiTarget" && newCmd)
                {
                    currMotorMultiTargetCmd = multiCmdTemp;
                    MultiTargetMoveInit();
                }
            }
            else if (newCmd)    // oldCmdType == "MultiTarget" && !overwriteMulti
            {
                if (multiCmdTemp.multiWriteType == Cube.MultiWriteType.Write)
                {
                    this.multiTargetMoveCallback?.Invoke(oldConfigID, Cube.TargetMoveRespondType.OtherWrite);
                    currMotorMultiTargetCmd = multiCmdTemp;
                    MultiTargetMoveInit();
                }
                else if (hasNextMotorMultiTargetCmd)
                {
                    this.multiTargetMoveCallback?.Invoke(multiCmdTemp.configID, Cube.TargetMoveRespondType.AddRefused);
                }
                else
                {
                    nextMotorMultiTargetCmd = multiCmdTemp;
                }
            }


            // ----- Excute Order -----
            switch (motorCurrentCmdType)
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
            public byte configID, timeOut, maxSpd;
            public Cube.TargetMoveType targetMoveType;
            public Cube.TargetSpeedType targetSpeedType;
            public Cube.TargetRotationType targetRotationType;
            public float tRecv;
            // Temp vars
            public bool reach;
            public float initialDeg, absoluteDeg, relativeDeg, lastDeg, acc;
        }
        protected Queue<MotorTargetCmd> motorTargetCmdQ = new Queue<MotorTargetCmd>(); // command queue
        protected MotorTargetCmd currMotorTargetCmd = default;  // current command

        protected virtual void TargetMoveInit()
        {
            // x/y of value 0xffff means last x/y
            if (this.currMotorTargetCmd.x==65535) this.currMotorTargetCmd.x = (ushort)this.x;
            if (this.currMotorTargetCmd.y==65535) this.currMotorTargetCmd.y = (ushort)this.y;

            var cmd = currMotorTargetCmd;
            float dist = Mathf.Sqrt( (cmd.x-this.x)*(cmd.x-this.x)+(cmd.y-this.y)*(cmd.y-this.y) );

            // Unsupported
            if (cmd.maxSpd < 10)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ToioIDmissed);
                motorCurrentCmdType = ""; motorLeft = 0; motorRight = 0; return;
            }

            // toio ID missed Error
            if (!this.onMat)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ToioIDmissed);
                motorCurrentCmdType = ""; motorLeft = 0; motorRight = 0; return;
            }

            // Parameter Error
            // if (dist < 8 &&
            //     cmd.targetRotationType==Cube.TargetRotationType.NotRotate
            //     || cmd.targetRotationType==Cube.TargetRotationType.Original
            //     || cmd.targetRotationType==Cube.TargetRotationType.RelativeClockwise && cmd.deg==0
            //     || cmd.targetRotationType==Cube.TargetRotationType.RelativeCounterClockwise && cmd.deg==0
            //     || (byte)cmd.targetRotationType<3 && Mathf.Abs(Deg(cmd.deg-this.deg))<5
            // ){
            //     this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ParameterError);
            //     motorCurrentCmdType = ""; motorLeft = 0; motorRight = 0; return;
            // }

            this.currMotorTargetCmd.acc = ((float)cmd.maxSpd*cmd.maxSpd-this.deadzone*this.deadzone) * CubeSimulator.VDotOverU
                /2/dist;

            this.currMotorTargetCmd.initialDeg = this.deg;
        }
        protected virtual void TargetMoveController()
        {
            var cmd = currMotorTargetCmd;
            float translate=0, rotate=0;

            // ---- Timeout ----
            byte timeout = cmd.timeOut==0? (byte)10:cmd.timeOut;  // TODO delete byte after merge
            if (motorCmdElipsed > timeout)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.Timeout);
                motorCurrentCmdType = ""; motorLeft = 0; motorRight = 0; return;
            }

            // ---- toio ID missed Error ----
            if (!this.onMat)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ToioIDmissed);
                motorCurrentCmdType = ""; motorLeft = 0; motorRight = 0; return;
            }

            // ---- Preprocess ----
            Vector2 targetPos = new Vector2(cmd.x, cmd.y);
            Vector2 pos = new Vector2(this.x, this.y);
            var dpos = targetPos - pos;

            // ---- Reach ----
            // reach pos
            if (!cmd.reach && dpos.magnitude < 10)
            {
                this.currMotorTargetCmd.reach = true;
                if (cmd.targetRotationType == Cube.TargetRotationType.NotRotate)        // Not rotate
                    this.currMotorTargetCmd.absoluteDeg = Deg(this.deg);
                else if (cmd.targetRotationType == Cube.TargetRotationType.Original)    // Inital deg
                    this.currMotorTargetCmd.absoluteDeg = Deg(cmd.initialDeg);
                else if ((byte)cmd.targetRotationType <= 2)                             // Absolute deg
                    this.currMotorTargetCmd.absoluteDeg = Deg(cmd.deg);
                else                                                                    // Relative deg
                {
                    this.currMotorTargetCmd.absoluteDeg = Deg(this.deg + cmd.deg);
                    this.currMotorTargetCmd.relativeDeg = cmd.deg;
                    this.currMotorTargetCmd.lastDeg = this.deg;
                }
            }
            // reach deg
            if (cmd.reach && Mathf.Abs(Deg(this.deg-cmd.absoluteDeg))<15 && cmd.relativeDeg<180)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.Normal);
                motorCurrentCmdType = ""; motorLeft = 0; motorRight = 0; return;
            }

            // ---- Update ----
            cmd = this.currMotorTargetCmd;

            // ---- Rotate ----
            if (cmd.reach)
            {
                float relativeDeg;
                (translate, rotate, relativeDeg) = TargetMove_RotateControl(
                    cmd.absoluteDeg, cmd.relativeDeg, cmd.lastDeg, cmd.targetRotationType);
                if (cmd.targetRotationType==Cube.TargetRotationType.RelativeClockwise
                    || cmd.targetRotationType==Cube.TargetRotationType.RelativeCounterClockwise)
                    this.currMotorTargetCmd.relativeDeg = relativeDeg;
            }
            // ---- Move ----
            else
            {
                (translate, rotate) = TargetMove_MoveControl(
                    cmd.x, cmd.y, cmd.maxSpd, cmd.targetSpeedType, cmd.acc, cmd.targetMoveType );
            }

            // ---- Apply ----
            var miu = Mathf.Abs(translate / this.maxMotor);
            rotate *= miu * Mathf.Abs(translate/50) + (1-miu) * 1;
            var uL = translate + rotate;
            var uR = translate - rotate;
            // truncate
            if (Mathf.Max(uL, uR) > this.maxMotor)
            {
                uL -= Mathf.Max(uL, uR) - this.maxMotor;
                uR -= Mathf.Max(uL, uR) - this.maxMotor;
            }
            else if (Mathf.Min(uL, uR) < -this.maxMotor)
            {
                uL -= Mathf.Min(uL, uR) + this.maxMotor;
                uR -= Mathf.Min(uL, uR) + this.maxMotor;
            }
            motorLeft = uL; motorRight = uR;
        }

        protected (float, float, float) TargetMove_RotateControl(
            float absoluteDeg, float relativeDeg, float lastDeg,
            Cube.TargetRotationType targetRotationType
        ){
            float translate=0, rotate=0;

            var ddeg = Deg(absoluteDeg - this.deg);
            // 回転タイプ
            switch (targetRotationType)
            {
                case (Cube.TargetRotationType.AbsoluteLeastAngle):       // 絶対角度 回転量が少ない方向
                case (Cube.TargetRotationType.Original):      // 書き込み操作時と同じ 回転量が少ない方向
                {
                    rotate = ddeg;
                    break;
                }
                case (Cube.TargetRotationType.AbsoluteClockwise):       // 絶対角度 正方向(時計回り)
                {
                    rotate = (ddeg + 360)%360;
                    break;
                }
                case (Cube.TargetRotationType.RelativeClockwise):      // 相対角度 正方向(時計回り)
                {
                    if (relativeDeg<180) rotate = (ddeg + 360)%360;
                    else
                    {
                        var ddegr = Deg(this.deg - lastDeg);
                        relativeDeg = relativeDeg - ddegr;
                        this.currMotorTargetCmd.lastDeg = this.deg;
                        rotate = 360;
                    }
                    break;
                }
                case (Cube.TargetRotationType.AbsoluteCounterClockwise):       // 絶対角度 負方向(反時計回り)
                {
                    rotate = -(-ddeg + 360)%360;
                    break;
                }
                case (Cube.TargetRotationType.RelativeCounterClockwise):      // 相対角度 負方向(反時計回り)
                {
                    if (relativeDeg<180) rotate = -(-ddeg + 360)%360;
                    else
                    {
                        var ddegr = Deg(this.deg - lastDeg);
                        relativeDeg = relativeDeg + ddegr;
                        this.currMotorTargetCmd.lastDeg = this.deg;
                        rotate = -360;
                    }
                    break;
                }
            }

            return (translate, rotate, relativeDeg);
        }
        protected (float, float) TargetMove_MoveControl(
            ushort x, ushort y,
            byte maxSpd,
            Cube.TargetSpeedType targetSpeedType,
            float acc,
            Cube.TargetMoveType targetMoveType
        ){
            float spd = maxSpd;
            float translate=0, rotate=0;

            Vector2 targetPos = new Vector2(x, y);
            Vector2 pos = new Vector2(this.x, this.y);
            var dpos = targetPos - pos;
            var dir2tar = Vector2.SignedAngle(Vector2.right, dpos);
            var deg2tar = Deg(dir2tar - this.deg);   // use when moving forward
            var deg2tar_back = (deg2tar+360)%360 -180;                // use when moving backward
            bool tarOnFront = Mathf.Abs(deg2tar) <= 90;

            // 速度変化タイプ
            switch (targetSpeedType)
            {
                case (Cube.TargetSpeedType.UniformSpeed):       // 速度一定
                { break; }
                case (Cube.TargetSpeedType.Acceleration):       // 目標地点まで徐々に加速
                {
                    spd = Mathf.Clamp(this.deadzone + acc*motorCmdElipsed, this.deadzone, maxSpd);
                    break;
                }
                case (Cube.TargetSpeedType.Deceleration):       // 目標地点まで徐々に減速
                {
                    spd = Mathf.Clamp(maxSpd - acc*motorCmdElipsed, this.deadzone, maxSpd);
                    break;
                }
                case (Cube.TargetSpeedType.VariableSpeed):      // 中間地点まで徐々に加速し、そこから目標地点まで減速
                {
                    spd = Mathf.Clamp(maxSpd - 2*acc*Mathf.Abs(motorCmdElipsed - (maxSpd-this.deadzone)/acc/2), this.deadzone, maxSpd);
                    break;
                }
            }

            // 移動タイプ
            switch (targetMoveType)
            {
                case (Cube.TargetMoveType.RotatingMove):        // 回転しながら移動
                {
                    rotate = tarOnFront? deg2tar : deg2tar_back;
                    translate = tarOnFront? spd : -spd;
                    break;
                }
                case (Cube.TargetMoveType.RoundForwardMove):    // 回転しながら移動（後退なし）
                {
                    rotate = deg2tar;
                    translate = spd;
                    break;
                }
                case (Cube.TargetMoveType.RoundBeforeMove):     // 回転してから移動
                {
                    rotate = deg2tar;
                    if (Mathf.Abs(deg2tar) < 15) translate = spd;
                    break;
                }
            }
            rotate *= 0.6f;
            rotate = Mathf.Clamp(rotate, -this.maxMotor, this.maxMotor);
            return (translate, rotate);
        }

        // Callback
        protected System.Action<int, Cube.TargetMoveRespondType> targetMoveCallback = null;
        public override void StartNotification_TargetMove(System.Action<int, Cube.TargetMoveRespondType> action)
        {
            this.targetMoveCallback = action;
        }

        // COMMAND API
        public override void TargetMove(
            int targetX,
            int targetY,
            int targetAngle,
            byte configID,
            byte timeOut,
            Cube.TargetMoveType targetMoveType,
            byte maxSpd,
            Cube.TargetSpeedType targetSpeedType,
            Cube.TargetRotationType targetRotationType
        ){
            MotorTargetCmd cmd = new MotorTargetCmd();
            cmd.x = (ushort)targetX; cmd.y = (ushort)targetY; cmd.deg = (ushort)targetAngle;
            cmd.configID = configID; cmd.timeOut = timeOut; cmd.targetMoveType = targetMoveType;
            cmd.maxSpd = maxSpd; cmd.targetSpeedType = targetSpeedType; cmd.targetRotationType = targetRotationType;
            cmd.tRecv = Time.time;
            motorTargetCmdQ.Enqueue(cmd);
        }


        // -------- Multi Target Move --------
        protected struct MotorMultiTargetCmd
        {
            public ushort[] xs, ys, degs;
            public Cube.TargetRotationType[] rotTypes;
            public byte configID, timeOut, maxSpd;
            public Cube.TargetMoveType targetMoveType;
            public Cube.TargetSpeedType targetSpeedType;
            public Cube.MultiWriteType multiWriteType;
            public float tRecv;
            // Temp vars
            public bool reach;
            public float initialDeg, absoluteDeg, relativeDeg, lastDeg, acc;
            public float elipsed;
            public byte idx;
        }
        protected Queue<MotorMultiTargetCmd> motorMultiTargetCmdQ = new Queue<MotorMultiTargetCmd>(); // command queue
        protected MotorMultiTargetCmd currMotorMultiTargetCmd = default;  // current command
        protected bool hasNextMotorMultiTargetCmd = false;
        protected MotorMultiTargetCmd nextMotorMultiTargetCmd = default;

        protected virtual void MultiTargetMoveInit()
        {
            var cmd = currMotorMultiTargetCmd;

            this.currMotorMultiTargetCmd.initialDeg = this.deg;

            // x/y of value 0xffff means last x/y
            for (int i=0; i<cmd.xs.Length; i++)
            {
                if (i==0)
                {
                    if (cmd.xs[i]==65535) this.currMotorMultiTargetCmd.xs[i] = (ushort)this.x;
                    if (cmd.ys[i]==65535) this.currMotorMultiTargetCmd.ys[i] = (ushort)this.y;
                }
                else
                {
                    if (cmd.xs[i]==65535) this.currMotorMultiTargetCmd.xs[i] = this.currMotorMultiTargetCmd.xs[i-1];
                    if (cmd.ys[i]==65535) this.currMotorMultiTargetCmd.ys[i] = this.currMotorMultiTargetCmd.xs[i-1];
                }
            }

            cmd = currMotorMultiTargetCmd;  // update

            // Unsupported
            if (cmd.maxSpd < 10)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ToioIDmissed);
                motorCurrentCmdType = ""; hasNextMotorMultiTargetCmd = false; motorLeft = 0; motorRight = 0; return;
            }

            // toio ID missed Error
            if (!this.onMat)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ToioIDmissed);
                motorCurrentCmdType = ""; hasNextMotorMultiTargetCmd = false; motorLeft = 0; motorRight = 0; return;
            }

            // Parameter Error ??

            // Calc. Acceleration
            float overallDist = 0;
            for (int i=0; i<cmd.xs.Length; i++)
            {
                float d;
                if (i==0)
                    d = Mathf.Sqrt( (cmd.xs[i]-this.x)*(cmd.xs[i]-this.x)+(cmd.ys[i]-this.y)*(cmd.ys[i]-this.y) );
                else
                    d = Mathf.Sqrt( (cmd.xs[i]-cmd.xs[i-1])*(cmd.xs[i]-cmd.xs[i-1])+(cmd.ys[i]-cmd.ys[i-1])*(cmd.ys[i]-cmd.ys[i-1]) );
                overallDist += d;
            }
            this.currMotorMultiTargetCmd.acc = ((float)cmd.maxSpd*cmd.maxSpd-this.deadzone*this.deadzone) * CubeSimulator.VDotOverU
                /2/overallDist;


        }
        protected virtual void MultiTargetMove_NextIdx()
        {
            this.currMotorMultiTargetCmd.reach = false;
            this.currMotorMultiTargetCmd.elipsed = 0;
            this.currMotorMultiTargetCmd.initialDeg = this.currMotorMultiTargetCmd.absoluteDeg;
            this.currMotorMultiTargetCmd.idx = (byte)(this.currMotorMultiTargetCmd.idx + 1);
        }
        protected virtual void MultiTargetMoveController()
        {
            var cmd = currMotorMultiTargetCmd;
            float translate=0, rotate=0;

            // ---- Timeout ----
            byte timeout = cmd.timeOut==0? (byte)10:cmd.timeOut;  // TODO delete byte after merge
            if (cmd.elipsed > timeout)
            {
                this.multiTargetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.Timeout);
                motorCurrentCmdType = ""; hasNextMotorMultiTargetCmd = false; motorLeft = 0; motorRight = 0; return;
            }

            // toio ID missed Error
            if (!this.onMat)
            {
                this.multiTargetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ToioIDmissed);
                motorCurrentCmdType = ""; hasNextMotorMultiTargetCmd = false; motorLeft = 0; motorRight = 0; return;
            }

            // ---- Preprocess ----
            Vector2 targetPos = new Vector2(cmd.xs[cmd.idx], cmd.ys[cmd.idx]);
            Vector2 pos = new Vector2(this.x, this.y);
            var dpos = targetPos - pos;

            // ---- Reach ----
            // reach pos
            if (!cmd.reach && dpos.magnitude < 10)
            {
                this.currMotorMultiTargetCmd.reach = true;
                var rotType = cmd.rotTypes[cmd.idx];
                if (rotType == Cube.TargetRotationType.NotRotate)           // Not rotate
                    this.currMotorMultiTargetCmd.absoluteDeg = Deg(this.deg);
                else if (rotType == Cube.TargetRotationType.Original)       // Inital deg
                    this.currMotorMultiTargetCmd.absoluteDeg = Deg(cmd.initialDeg);
                else if ((byte)rotType <= 2)                                // Absolute deg
                    this.currMotorMultiTargetCmd.absoluteDeg = Deg(cmd.degs[cmd.idx]);
                else                                                        // Relative deg
                {
                    this.currMotorMultiTargetCmd.absoluteDeg = Deg(this.deg + cmd.degs[cmd.idx]);
                    this.currMotorMultiTargetCmd.relativeDeg = cmd.degs[cmd.idx];
                    this.currMotorMultiTargetCmd.lastDeg = this.deg;
                }
            }
            // reach deg
            if (cmd.reach && Mathf.Abs(Deg(this.deg-cmd.absoluteDeg))<15 && cmd.relativeDeg<180)
            {
                // This is the last target
                if (cmd.idx==cmd.xs.Length-1)
                {
                    this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.Normal);
                    // Load Next Command
                    if (hasNextMotorMultiTargetCmd)
                    {
                        currMotorMultiTargetCmd = nextMotorMultiTargetCmd;
                        hasNextMotorMultiTargetCmd = false;
                        MultiTargetMoveInit();
                    }
                    // Over
                    else
                    {
                        motorCurrentCmdType = ""; hasNextMotorMultiTargetCmd = false; motorLeft = 0; motorRight = 0; return;
                    }
                }
                // Not the last target
                else MultiTargetMove_NextIdx();
            }

            // ---- Update ----
            cmd = currMotorMultiTargetCmd;

            // ---- Rotate ----
            if (cmd.reach)
            {
                float relativeDeg;
                (translate, rotate, relativeDeg) = TargetMove_RotateControl(
                    cmd.absoluteDeg, cmd.relativeDeg, cmd.lastDeg, cmd.rotTypes[cmd.idx]);
                if (cmd.rotTypes[cmd.idx]==Cube.TargetRotationType.RelativeClockwise
                    || cmd.rotTypes[cmd.idx]==Cube.TargetRotationType.RelativeCounterClockwise)
                    this.currMotorMultiTargetCmd.relativeDeg = relativeDeg;
            }

            // ---- Move ----
            else
            {
                (translate, rotate) = TargetMove_MoveControl(
                    cmd.xs[cmd.idx], cmd.ys[cmd.idx], cmd.maxSpd, cmd.targetSpeedType, cmd.acc, cmd.targetMoveType );
            }

            // ---- Apply ----
            motorLeft = translate + rotate;
            motorRight = translate - rotate;
        }

        // Callback
        protected System.Action<int, Cube.TargetMoveRespondType> multiTargetMoveCallback = null;
        public override void StartNotification_MultiTargetMove(System.Action<int, Cube.TargetMoveRespondType> action)
        {
            this.multiTargetMoveCallback = action;
        }

        // COMMAND API
        public override void MultiTargetMove(
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
        ){
            MotorMultiTargetCmd cmd = new MotorMultiTargetCmd();
            cmd.xs = Array.ConvertAll(targetXList, new Converter<int, ushort>(x=>(ushort)x));
            cmd.ys = Array.ConvertAll(targetYList, new Converter<int, ushort>(x=>(ushort)x));
            cmd.degs = Array.ConvertAll(targetAngleList, new Converter<int, ushort>(x=>(ushort)x));
            cmd.rotTypes = multiRotationTypeList!=null? multiRotationTypeList : new Cube.TargetRotationType[targetXList.Length];
            cmd.configID = configID; cmd.timeOut = timeOut; cmd.maxSpd = maxSpd;
            cmd.targetMoveType = targetMoveType; cmd.targetSpeedType = targetSpeedType; cmd.multiWriteType = multiWriteType;
            cmd.tRecv = Time.time;
            motorMultiTargetCmdQ.Enqueue(cmd);
        }


        // -------- Acceleration Move --------
        protected struct MotorAccCmd
        {
            public byte spd, acc, controlTime, configID;
            public ushort rotationSpeed;
            public Cube.AccRotationType accRotationType;
            public Cube.AccMoveType accMoveType;
            public Cube.AccPriorityType accPriorityType;
            public float tRecv;
        }
        protected Queue<MotorAccCmd> motorAccCmdQ = new Queue<MotorAccCmd>(); // command queue
        protected MotorAccCmd currMotorAccCmd = default;  // current command

        protected virtual void AccMoveController()
        {

        }

        // COMMAND API
        public override void AccelerationMove(
            int targetSpeed,
            int acceleration,
            ushort rotationSpeed,
            Cube.AccRotationType accRotationType,
            Cube.AccMoveType accMoveType,
            Cube.AccPriorityType accPriorityType,
            byte controlTime
        ){
            MotorAccCmd cmd = new MotorAccCmd();
            cmd.spd = (byte)targetSpeed; cmd.acc = (byte)acceleration;
            cmd.rotationSpeed = rotationSpeed; cmd.accRotationType = accRotationType;
            cmd.accMoveType = accMoveType; cmd.accPriorityType = accPriorityType; cmd.controlTime = controlTime;
            cmd.tRecv = Time.time;
            motorAccCmdQ.Enqueue(cmd);
        }



        // ============ Motion Sensor ============
        // ---------- Pose -----------
        protected Cube.PoseType _pose = Cube.PoseType.Up;
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
                this.pose = Cube.PoseType.Up;
            }
            else if(Vector3.Angle(Vector3.up, cube.transform.up)>180-slopeThreshold)
            {
                this.pose = Cube.PoseType.Down;
            }
            else if(Vector3.Angle(Vector3.up, cube.transform.forward)<slopeThreshold)
            {
                this.pose = Cube.PoseType.Front;
            }
            else if(Vector3.Angle(Vector3.up, cube.transform.forward)>180-slopeThreshold)
            {
                this.pose = Cube.PoseType.Back;
            }
            else if(Vector3.Angle(Vector3.up, cube.transform.right)<slopeThreshold)
            {
                this.pose = Cube.PoseType.Right;
            }
            else if(Vector3.Angle(Vector3.up, cube.transform.right)>180-slopeThreshold)
            {
                this.pose = Cube.PoseType.Left;
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