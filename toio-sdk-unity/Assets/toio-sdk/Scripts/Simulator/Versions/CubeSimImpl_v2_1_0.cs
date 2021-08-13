using System;
using System.Linq;
using UnityEngine;


namespace toio.Simulator
{

    internal class CubeSimImpl_v2_1_0 : CubeSimImpl_v2_0_0
    {
        public override int maxMotor{get;} = 115;
        public override int deadzone{get;} = 8;
        public  CubeSimImpl_v2_1_0(CubeSimulator cube) : base(cube){}


        #region ============ Motor ============

        protected virtual void MotorSetCmd(MotorCmdType type, object cmd)
        {
            // ------ Overwrite Events ------

            // TargetCmd -> Any
            if (motorCmdType == MotorCmdType.MotorTargetCmd)
                this.targetMoveCallback?.Invoke(motorTargetCmd.configID, Cube.TargetMoveRespondType.OtherWrite);
            // MultiTargetCmd -> Other types
            else if (motorCmdType == MotorCmdType.MotorMultiTargetCmd && type != MotorCmdType.MotorMultiTargetCmd)
                this.multiTargetMoveCallback?.Invoke(motorMultiTargetCmd.configID, Cube.TargetMoveRespondType.OtherWrite);
            // MultiTargetCmd -> MultiTargetCmd
            else if (motorCmdType == MotorCmdType.MotorMultiTargetCmd && type == MotorCmdType.MotorMultiTargetCmd)
                if ( ((MotorMultiTargetCmd)cmd).multiWriteType == Cube.MultiWriteType.Write )
                    this.multiTargetMoveCallback?.Invoke(motorMultiTargetCmd.configID, Cube.TargetMoveRespondType.OtherWrite);
                else if ( this.hasNextMotorMultiTargetCmd)
                    this.multiTargetMoveCallback?.Invoke(((MotorMultiTargetCmd)cmd).configID, Cube.TargetMoveRespondType.AddRefused);

            // ------ Update Commands ------

            bool initSuccess = true;

            // TargetCmd
            if (type == MotorCmdType.MotorTargetCmd)
            {
                motorTargetCmd = (MotorTargetCmd) cmd;
                initSuccess = TargetMoveInit();
            }
            // MultiTargetCmd
            else if (type == MotorCmdType.MotorMultiTargetCmd)
            {
                var newCmd = (MotorMultiTargetCmd)cmd;
                if (motorCmdType != MotorCmdType.MotorMultiTargetCmd)
                {
                    this.motorMultiTargetCmd = newCmd;
                    this.hasNextMotorMultiTargetCmd = false;
                    initSuccess = MultiTargetMoveInit();
                }
                else if (newCmd.multiWriteType == Cube.MultiWriteType.Write)
                {
                    this.motorMultiTargetCmd = newCmd;
                    this.hasNextMotorMultiTargetCmd = false;
                    initSuccess = MultiTargetMoveInit();
                }
                else if (!this.hasNextMotorMultiTargetCmd)
                {
                    this.nextMotorMultiTargetCmd = newCmd;
                    this.hasNextMotorMultiTargetCmd = true;
                }
            }
            // AccCmd
            else if (type == MotorCmdType.MotorAccCmd)
            {
                var newCmd = (MotorAccCmd)cmd;
                // 前指令がAccの場合, 速度を継続する
                if (motorCmdType == MotorCmdType.MotorAccCmd)
                    newCmd.initialSpd = this.motorAccCmd.currSpd;
                else    // 前指令がAccじゃない場合
                    newCmd.initialSpd = 0;   // 速度を継続する     ※リアルのテスト結果
                    // newCmd.acc = 0;    // 直ちに目標速度にする      ※仕様書
                this.motorAccCmd = newCmd;
            }
            else if (type == MotorCmdType.MotorTimeCmd)
            {
                this.motorTimeCmd = (MotorTimeCmd)cmd;
            }

            // Set motorCmdType
            if (initSuccess) this.motorCmdType = type;
            else this.motorCmdType = MotorCmdType.None;
        }

        protected override void MotorScheduler(float t)
        {
            // ----- Excute Order -----
            switch (this.motorCmdType)
            {
                case MotorCmdType.MotorTimeCmd:
                {
                    float elipsed = t - motorTimeCmd.tRecv;
                    if (motorTimeCmd.duration==0 || elipsed < motorTimeCmd.duration/1000f)
                    {
                        motorCmdL = motorTimeCmd.left;
                        motorCmdR = motorTimeCmd.right;
                    }
                    else
                        this.motorCmdType = MotorCmdType.None;
                    break;
                }
                case MotorCmdType.MotorTargetCmd:
                {
                    float elipsed = t - motorTargetCmd.tRecv;
                    TargetMoveController(elipsed);
                    break;
                }
                case MotorCmdType.MotorMultiTargetCmd:
                {
                    float elipsed = t - motorMultiTargetCmd.tRecv;
                    MultiTargetMoveController(elipsed);
                    break;
                }
                case MotorCmdType.MotorAccCmd:
                {
                    float elipsed = t - motorAccCmd.tRecv;
                    AccMoveController(elipsed);
                    break;
                }
            }

            // Order Over
            if (this.motorCmdType == MotorCmdType.None)
            {
                motorCmdL = 0; motorCmdR = 0;
            }
        }

        protected override void ResetMotor()
        {
            base.ResetMotor();

            motorTargetCmd = default;
            targetMoveCallback = null;

            motorMultiTargetCmd = default;
            hasNextMotorMultiTargetCmd = false;
            nextMotorMultiTargetCmd = default;
            multiTargetMoveCallback = null;

            motorAccCmd = default;
        }

        #endregion


        #region  -------- Time Move --------
        public override void Move(int left, int right, int durationMS)
        {
            MotorTimeCmd cmd = new MotorTimeCmd();
            cmd.left = Mathf.Clamp(left, -maxMotor, maxMotor);
            cmd.right = Mathf.Clamp(right, -maxMotor, maxMotor);
            if (Mathf.Abs(cmd.left) < this.deadzone) cmd.left = 0;
            if (Mathf.Abs(cmd.right) < this.deadzone) cmd.right = 0;
            cmd.duration = Mathf.Clamp(durationMS, 0, 2550);
            cmd.tRecv = Time.time;

            MotorSetCmd(MotorCmdType.MotorTimeCmd, cmd);
        }
        #endregion


        #region  -------- Target Move --------
        // -------- Parameters --------
        protected float motorTargetPosTol = 10;
        protected float motorTargetDegTol = 15;

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
        protected MotorTargetCmd motorTargetCmd = default;  // current command

        protected virtual bool TargetMoveInit()
        {
            var cmd = motorTargetCmd;

            // Parameter Error
            if (cmd.x==65535 && cmd.y==65535
                && (cmd.targetRotationType==Cube.TargetRotationType.Original || cmd.targetRotationType==Cube.TargetRotationType.NotRotate))
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ParameterError);
                this.motorCmdType = MotorCmdType.None; return false;
            }

            // x/y of value 0xffff means last x/y
            if (this.motorTargetCmd.x==65535) this.motorTargetCmd.x = (ushort)this.x;
            if (this.motorTargetCmd.y==65535) this.motorTargetCmd.y = (ushort)this.y;

            cmd = motorTargetCmd;
            float dist = Mathf.Sqrt( (cmd.x-this.x)*(cmd.x-this.x)+(cmd.y-this.y)*(cmd.y-this.y) );

            // Unsupported
            if (cmd.maxSpd < 10)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.NonSupport);
                this.motorCmdType = MotorCmdType.None; return false;
            }

            // toio ID missed Error
            if (!this.onMat)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ToioIDmissed);
                this.motorCmdType = MotorCmdType.None; return false;
            }

            this.motorTargetCmd.acc = ((float)cmd.maxSpd*cmd.maxSpd-this.deadzone*this.deadzone) * CubeSimulator.VDotOverU / 2 / dist;

            this.motorTargetCmd.initialDeg = this.deg;
            return true;
        }
        protected virtual void TargetMoveController(float elipsed)
        {
            var cmd = motorTargetCmd;
            float translate=0, rotate=0;

            // ---- Timeout ----
            byte timeout = cmd.timeOut==0? (byte)10:cmd.timeOut;
            if (elipsed > timeout)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.Timeout);
                this.motorCmdType = MotorCmdType.None; return;
            }

            // ---- toio ID missed Error ----
            if (!this.onMat)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ToioIDmissed);
                this.motorCmdType = MotorCmdType.None; return;
            }

            // ---- Preprocess ----
            Vector2 targetPos = new Vector2(cmd.x, cmd.y);
            Vector2 pos = new Vector2(this.x, this.y);
            var dpos = targetPos - pos;

            // ---- Reach ----
            // reach pos
            if (!cmd.reach && dpos.magnitude < motorTargetPosTol)
            {
                this.motorTargetCmd.reach = true;
                if (cmd.targetRotationType == Cube.TargetRotationType.NotRotate)        // Not rotate
                    this.motorTargetCmd.absoluteDeg = Deg(this.deg);
                else if (cmd.targetRotationType == Cube.TargetRotationType.Original)    // Inital deg
                    this.motorTargetCmd.absoluteDeg = Deg(cmd.initialDeg);
                else if ((byte)cmd.targetRotationType <= 2)                             // Absolute deg
                    this.motorTargetCmd.absoluteDeg = Deg(cmd.deg);
                else                                                                    // Relative deg
                {
                    this.motorTargetCmd.absoluteDeg = Deg(this.deg + cmd.deg);
                    this.motorTargetCmd.relativeDeg = cmd.deg;
                    this.motorTargetCmd.lastDeg = this.deg;
                }
            }
            // reach deg
            if (cmd.reach && Mathf.Abs(Deg(this.deg-cmd.absoluteDeg))<motorTargetDegTol && cmd.relativeDeg<180)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.Normal);
                this.motorCmdType = MotorCmdType.None; return;
            }

            // ---- Update ----
            cmd = this.motorTargetCmd;

            // ---- Rotate ----
            if (cmd.reach)
            {
                float relativeDeg;
                (translate, rotate, relativeDeg) = TargetMove_RotateControl(
                    cmd.absoluteDeg, cmd.relativeDeg, cmd.lastDeg, cmd.targetRotationType,cmd.maxSpd);
                if (cmd.targetRotationType==Cube.TargetRotationType.RelativeClockwise
                    || cmd.targetRotationType==Cube.TargetRotationType.RelativeCounterClockwise)
                    this.motorTargetCmd.relativeDeg = relativeDeg;
            }
            // ---- Move ----
            else
            {
                (translate, rotate) = TargetMove_MoveControl(elipsed,
                    cmd.x, cmd.y, cmd.maxSpd, cmd.targetSpeedType, cmd.acc, cmd.targetMoveType );
            }

            // ---- Apply ----
            ApplyMotorControl(translate, rotate);
        }

        protected (float, float, float) TargetMove_RotateControl(
            float absoluteDeg, float relativeDeg, float lastDeg,
            Cube.TargetRotationType targetRotationType, byte maxSpd
        ){
            float rotate=0;

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
                        this.motorTargetCmd.lastDeg = this.deg;
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
                        this.motorTargetCmd.lastDeg = this.deg;
                        rotate = -360;
                    }
                    break;
                }
            }
            if (rotate>=0) rotate = Mathf.Clamp(rotate, deadzone, maxSpd);
            else rotate = Mathf.Clamp(rotate, -maxSpd, -deadzone);

            return (0, rotate, relativeDeg);
        }
        protected (float, float) TargetMove_MoveControl(
            float elipsed,
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
            var deg2tar = Deg(dir2tar - this.deg);                    // use when moving forward
            var deg2tar_back = (deg2tar+360)%360 -180;                // use when moving backward
            bool tarOnFront = Mathf.Abs(deg2tar) <= 90;

            // 速度変化タイプ
            switch (targetSpeedType)
            {
                case (Cube.TargetSpeedType.UniformSpeed):       // 速度一定
                { break; }
                case (Cube.TargetSpeedType.Acceleration):       // 目標地点まで徐々に加速
                {
                    spd = Mathf.Clamp(this.deadzone + acc*elipsed, this.deadzone, maxSpd);
                    break;
                }
                case (Cube.TargetSpeedType.Deceleration):       // 目標地点まで徐々に減速
                {
                    spd = Mathf.Clamp(maxSpd - acc*elipsed, this.deadzone, maxSpd);
                    break;
                }
                case (Cube.TargetSpeedType.VariableSpeed):      // 中間地点まで徐々に加速し、そこから目標地点まで減速
                {
                    spd = Mathf.Clamp(maxSpd - 2*acc*Mathf.Abs(elipsed - (maxSpd-this.deadzone)/acc/2), this.deadzone, maxSpd);
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
                    if (Mathf.Abs(deg2tar) < motorTargetDegTol) translate = spd;
                    break;
                }
            }
            rotate *= 0.6f;
            rotate = Mathf.Clamp(rotate, -this.maxMotor, this.maxMotor);
            return (translate, rotate);
        }

        protected void ApplyMotorControl(float translate, float rotate)
        {
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
            motorCmdL = uL; motorCmdR = uR;
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
            int configID,
            int timeOut,
            Cube.TargetMoveType targetMoveType,
            int maxSpd,
            Cube.TargetSpeedType targetSpeedType,
            Cube.TargetRotationType targetRotationType
        ){
#if !RELEASE
            if (65535 < targetX || targetX<-1)
                Debug.LogErrorFormat("[Cube.TargetMove]X座標範囲を超えました. targetX={0}", targetX);
            if (65535 < targetY || targetY<-1)
                Debug.LogErrorFormat("[Cube.TargetMove]Y座標範囲を超えました. targetY={0}", targetY);
            if (8191 < targetAngle)
                Debug.LogErrorFormat("[Cube.TargetMove]回転角度範囲を超えました. targetAngle={0}", targetAngle);
            if (255 < configID)
                Debug.LogErrorFormat("[Cube.TargetMove]制御識別値範囲を超えました. configID={0}", configID);
            if (255 < timeOut)
                Debug.LogErrorFormat("[Cube.TargetMove]制御時間範囲を超えました. timeOut={0}", timeOut);
            if (this.maxMotor < maxSpd)
                Debug.LogErrorFormat("[Cube.TargetMove]速度範囲を超えました. maxSpd={0}", maxSpd);
#endif
            MotorTargetCmd cmd = new MotorTargetCmd();
            cmd.x = (ushort)(targetX==-1?65535:Mathf.Clamp(targetX, 0, 65535));
            cmd.y = (ushort)(targetY==-1?65535:Mathf.Clamp(targetY, 0, 65535));
            cmd.deg = (ushort)Mathf.Clamp(targetAngle, 0, 8191);
            cmd.configID = (byte)Mathf.Clamp(configID, 0, 255);
            cmd.timeOut = (byte)Mathf.Clamp(timeOut, 0, 255);
            cmd.targetMoveType = targetMoveType;
            cmd.maxSpd = (byte)Mathf.Clamp(maxSpd, 0, this.maxMotor);
            cmd.targetSpeedType = targetSpeedType; cmd.targetRotationType = targetRotationType;
            cmd.tRecv = Time.time;

            MotorSetCmd(MotorCmdType.MotorTargetCmd, cmd);
        }

        #endregion


        #region  -------- Multi Target Move --------
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
        protected MotorMultiTargetCmd motorMultiTargetCmd = default;  // current command
        protected bool hasNextMotorMultiTargetCmd = false;
        protected MotorMultiTargetCmd nextMotorMultiTargetCmd = default;

        protected virtual bool MultiTargetMoveInit()
        {
            var cmd = motorMultiTargetCmd;

            this.motorMultiTargetCmd.initialDeg = this.deg;

            // Parameter Error
            if (cmd.xs[0]==65535 && cmd.ys[0]==65535
                && (cmd.rotTypes[0]==Cube.TargetRotationType.Original || cmd.rotTypes[0]==Cube.TargetRotationType.NotRotate))
            {
                this.multiTargetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ParameterError);
                hasNextMotorMultiTargetCmd = false; return false;
            }
            if (cmd.xs[0]==65535) this.motorMultiTargetCmd.xs[0] = (ushort)this.x;
            if (cmd.ys[0]==65535) this.motorMultiTargetCmd.ys[0] = (ushort)this.y;

            // Unsupported
            if (cmd.maxSpd < 10)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ToioIDmissed);
                hasNextMotorMultiTargetCmd = false; return false;
            }

            // toio ID missed Error
            if (!this.onMat)
            {
                this.targetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ToioIDmissed);
                hasNextMotorMultiTargetCmd = false; return false;
            }

            // Calc. Acceleration
            float overallDist = 0;
            float xi = cmd.xs[0], yi = cmd.ys[0];
            for (int i=0; i<cmd.xs.Length; i++)
            {
                float d;
                if (i==0)
                    d = Mathf.Sqrt( (xi-this.x)*(xi-this.x)+(yi-this.y)*(yi-this.y) );
                else
                {
                    var xi_1 = cmd.xs[i]==65535? xi:cmd.xs[i];
                    var yi_1 = cmd.ys[i]==65535? yi:cmd.ys[i];
                    d = Mathf.Sqrt( (xi_1-xi)*(xi_1-xi)+(yi_1-yi)*(yi_1-yi) );
                    overallDist += d;
                }
            }
            this.motorMultiTargetCmd.acc = ((float)cmd.maxSpd*cmd.maxSpd-this.deadzone*this.deadzone) * CubeSimulator.VDotOverU
                /2/overallDist;

            return true;
        }
        protected virtual void MultiTargetMove_NextIdx()
        {
            this.motorMultiTargetCmd.reach = false;
            this.motorMultiTargetCmd.elipsed = 0;
            this.motorMultiTargetCmd.initialDeg = this.motorMultiTargetCmd.absoluteDeg;
            this.motorMultiTargetCmd.idx = (byte)(this.motorMultiTargetCmd.idx + 1);
        }
        protected virtual void MultiTargetMoveController(float elipsed)
        {
            var cmd = motorMultiTargetCmd;
            float translate=0, rotate=0;

            // ---- Timeout ----
            byte timeout = cmd.timeOut==0? (byte)10:cmd.timeOut;
            if (cmd.elipsed > timeout)
            {
                this.multiTargetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.Timeout);
                this.motorCmdType = MotorCmdType.None; hasNextMotorMultiTargetCmd = false; return;
            }

            // ---- toio ID missed Error ----
            if (!this.onMat)
            {
                this.multiTargetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ToioIDmissed);
                this.motorCmdType = MotorCmdType.None; hasNextMotorMultiTargetCmd = false; return;
            }

            // ---- Parameter Error ----
            if (cmd.xs[cmd.idx]==65535 && cmd.ys[cmd.idx]==65535
                && (cmd.rotTypes[cmd.idx]==Cube.TargetRotationType.Original || cmd.rotTypes[cmd.idx]==Cube.TargetRotationType.NotRotate))
            {
                this.multiTargetMoveCallback?.Invoke(cmd.configID, Cube.TargetMoveRespondType.ParameterError);
                this.motorCmdType = MotorCmdType.None; hasNextMotorMultiTargetCmd = false; return;
            }

            // ---- Preprocess ----
            if (cmd.xs[cmd.idx]==65535 && cmd.idx>0)
                this.motorMultiTargetCmd.xs[cmd.idx] = cmd.xs[cmd.idx] = cmd.xs[cmd.idx-1];
            if (cmd.ys[cmd.idx]==65535 && cmd.idx>0)
                this.motorMultiTargetCmd.ys[cmd.idx] = cmd.ys[cmd.idx] = cmd.ys[cmd.idx-1];
            Vector2 targetPos = new Vector2(cmd.xs[cmd.idx], cmd.ys[cmd.idx]);
            Vector2 pos = new Vector2(this.x, this.y);
            var dpos = targetPos - pos;

            // ---- Reach ----
            // reach pos
            if (!cmd.reach && dpos.magnitude < motorTargetPosTol)
            {
                this.motorMultiTargetCmd.reach = true;
                var rotType = cmd.rotTypes[cmd.idx];
                if (rotType == Cube.TargetRotationType.NotRotate)           // Not rotate
                    this.motorMultiTargetCmd.absoluteDeg = Deg(this.deg);
                else if (rotType == Cube.TargetRotationType.Original)       // Inital deg
                    this.motorMultiTargetCmd.absoluteDeg = Deg(cmd.initialDeg);
                else if ((byte)rotType <= 2)                                // Absolute deg
                    this.motorMultiTargetCmd.absoluteDeg = Deg(cmd.degs[cmd.idx]);
                else                                                        // Relative deg
                {
                    this.motorMultiTargetCmd.absoluteDeg = Deg(this.deg + cmd.degs[cmd.idx]);
                    this.motorMultiTargetCmd.relativeDeg = cmd.degs[cmd.idx];
                    this.motorMultiTargetCmd.lastDeg = this.deg;
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
                        motorMultiTargetCmd = nextMotorMultiTargetCmd;
                        hasNextMotorMultiTargetCmd = false;
                        MultiTargetMoveInit();
                    }
                    // Over
                    else
                    {
                        this.motorCmdType = MotorCmdType.None; hasNextMotorMultiTargetCmd = false; return;
                    }
                }
                // Not the last target
                else MultiTargetMove_NextIdx();
            }

            // ---- Update ----
            cmd = motorMultiTargetCmd;

            // ---- Rotate ----
            if (cmd.reach)
            {
                float relativeDeg;
                (translate, rotate, relativeDeg) = TargetMove_RotateControl(
                    cmd.absoluteDeg, cmd.relativeDeg, cmd.lastDeg, cmd.rotTypes[cmd.idx], cmd.maxSpd);
                if (cmd.rotTypes[cmd.idx]==Cube.TargetRotationType.RelativeClockwise
                    || cmd.rotTypes[cmd.idx]==Cube.TargetRotationType.RelativeCounterClockwise)
                    this.motorMultiTargetCmd.relativeDeg = relativeDeg;
            }

            // ---- Move ----
            else
            {
                (translate, rotate) = TargetMove_MoveControl(elipsed,
                    cmd.xs[cmd.idx], cmd.ys[cmd.idx], cmd.maxSpd, cmd.targetSpeedType, cmd.acc, cmd.targetMoveType );
            }

            // ---- Apply ----
            ApplyMotorControl(translate, rotate);
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
            int configID,
            int timeOut,
            Cube.TargetMoveType targetMoveType,
            int maxSpd,
            Cube.TargetSpeedType targetSpeedType,
            Cube.MultiWriteType multiWriteType
        ){
#if !RELEASE
            if (targetXList.Length==0 || targetYList.Length==0 || targetAngleList.Length==0
                || !(targetXList.Length==targetYList.Length && targetYList.Length==targetAngleList.Length))
                Debug.LogErrorFormat("[Cube.TargetMove]座標・角度リストのサイズが不一致または０. targetXList.Length={0}, targetYList.Length={1}, targetAngleList.Length={2}", targetXList.Length, targetYList.Length, targetAngleList.Length);
            if (255 < configID)
                Debug.LogErrorFormat("[Cube.TargetMove]制御識別値範囲を超えました. configID={0}", configID);
            if (255 < timeOut)
                Debug.LogErrorFormat("[Cube.TargetMove]制御時間範囲を超えました. timeOut={0}", timeOut);
            if (this.maxMotor < maxSpd)
                Debug.LogErrorFormat("[Cube.TargetMove]速度範囲を超えました. maxSpd={0}", maxSpd);
#endif
            MotorMultiTargetCmd cmd = new MotorMultiTargetCmd();
            cmd.xs = Array.ConvertAll(targetXList, new Converter<int, ushort>(x=>(ushort)(x==-1?65535:Mathf.Clamp(x, 0, 65534))));
            if (cmd.xs.Length==0) return;
            cmd.xs = cmd.xs.Take(29).ToArray();
            cmd.ys = Array.ConvertAll(targetYList, new Converter<int, ushort>(x=>(ushort)(x==-1?65535:Mathf.Clamp(x, 0, 65534))));
            if (cmd.ys.Length==0) return;
            cmd.ys = cmd.ys.Take(29).ToArray();
            cmd.degs = Array.ConvertAll(targetAngleList, new Converter<int, ushort>(x=>(ushort)x));
            if (cmd.degs.Length==0) return;
            cmd.degs = cmd.degs.Take(29).ToArray();
            cmd.rotTypes = multiRotationTypeList!=null? multiRotationTypeList : new Cube.TargetRotationType[cmd.xs.Length];
            cmd.configID = (byte)Mathf.Clamp(configID, 0, 255);
            cmd.timeOut = (byte)Mathf.Clamp(timeOut, 0, 255);
            cmd.maxSpd = (byte)Mathf.Clamp(maxSpd, 0, this.maxMotor);
            cmd.targetMoveType = targetMoveType;
            cmd.targetSpeedType = targetSpeedType;
            cmd.multiWriteType = multiWriteType;
            cmd.tRecv = Time.time;

            MotorSetCmd(MotorCmdType.MotorMultiTargetCmd, cmd);
        }

        #endregion


        #region  -------- Acceleration Move --------
        protected struct MotorAccCmd
        {
            public byte acc, controlTime;
            public int spd, rotationSpeed;
            public Cube.AccPriorityType accPriorityType;
            public float tRecv;
            public float initialSpd, currSpd;
        }
        protected MotorAccCmd motorAccCmd = default;  // current command

        protected virtual void AccMoveController(float elipsed)
        {
            var cmd = motorAccCmd;
            float translate=0, rotate=0;

            // ---- Control Time ----
            if (cmd.controlTime!=0 && elipsed > (float)cmd.controlTime/100)
            {
                this.motorAccCmd.currSpd = 0;
                this.motorCmdType = MotorCmdType.None; return;
            }

            // ---- Acceleration ----
            float targetSpd = cmd.spd;
            float spd;
            if (cmd.acc == 0)
                spd = targetSpd;
            else
            {
                if (elipsed >= Mathf.Abs(targetSpd-cmd.initialSpd)/(cmd.acc/0.1f))   // Accelerate Over
                    spd = targetSpd;
                else
                    spd = cmd.initialSpd + Mathf.Sign(targetSpd-cmd.initialSpd) * elipsed*cmd.acc/0.1f;
            }
            this.motorAccCmd.currSpd = spd;
            translate = spd;

            // ---- Rotation ----
            rotate = cmd.rotationSpeed *Mathf.Deg2Rad * CubeSimulator.TireWidthDot/CubeSimulator.VDotOverU /2;


            // ---- Priority ----
            if (cmd.accPriorityType == Cube.AccPriorityType.Translation)
            {
                var limit = this.maxMotor - Mathf.Abs(translate);
                rotate = Mathf.Clamp(rotate, -limit, limit);
            }
            else if (cmd.accPriorityType == Cube.AccPriorityType.Rotation)
            {
                var limit = this.maxMotor - Mathf.Abs(rotate);
                translate = Mathf.Clamp(translate, -limit, limit);
            }

            // ---- Apply ----
            motorCmdL = translate + rotate;
            motorCmdR = translate - rotate;
        }

        // COMMAND API
        public override void AccelerationMove(
            int targetSpeed,
            int acceleration,
            int rotationSpeed,
            Cube.AccPriorityType accPriorityType,
            int controlTime
        ){
#if !RELEASE
            if (this.maxMotor < Mathf.Abs(targetSpeed)) Debug.LogErrorFormat("[Cube.AccelerationMove]直線速度範囲を超えました. targetSpeed={0}", targetSpeed);
            if (255 < acceleration) Debug.LogErrorFormat("[Cube.AccelerationMove]加速度範囲を超えました. acceleration={0}", acceleration);
            if (65535 < rotationSpeed) Debug.LogErrorFormat("[Cube.AccelerationMove]回転速度範囲を超えました. rotationSpeed={0}", rotationSpeed);
            if (255 < controlTime) Debug.LogErrorFormat("[Cube.AccelerationMove]制御時間範囲を超えました. controlTime={0}", controlTime);
#endif
            MotorAccCmd cmd = new MotorAccCmd();
            cmd.spd = Mathf.Clamp(targetSpeed, -this.maxMotor, this.maxMotor);
            cmd.acc = (byte)Mathf.Clamp(acceleration, 0, 255);
            cmd.rotationSpeed = Mathf.Clamp(rotationSpeed, -65535, 65535);
            cmd.accPriorityType = accPriorityType;
            cmd.controlTime = (byte)Mathf.Clamp(controlTime, 0, 255);
            cmd.tRecv = Time.time;

            MotorSetCmd(MotorCmdType.MotorAccCmd, cmd);
        }

        #endregion


        #region  ============ Motion Sensor ============
        protected override void InvokeMotionSensorCallback()
        {
            if (this.motionSensorCallback == null) return;
            object[] sensors = new object[5];
            sensors[0] = null;
            sensors[1] = (object)this._sloped;
            sensors[2] = (object)this._collisonDetected; this._collisonDetected = false;
            sensors[3] = (object)this._doubleTapped; this._doubleTapped = false;
            sensors[4] = (object)this._pose;
            this.motionSensorCallback.Invoke(sensors);
        }

        // ---------- Pose -----------
        protected Cube.PoseType _pose = Cube.PoseType.Up;
        public override Cube.PoseType pose {
            get{ return _pose; }
            internal set{
                if (this._pose != value){
                    this._pose = value;
                    this.InvokeMotionSensorCallback();
                }
            }
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
        protected bool _doubleTapped = false;
        internal override void TriggerDoubleTap()
        {
            this._doubleTapped = true;
            this.InvokeMotionSensorCallback();
            this._doubleTapped = false;
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

        protected override void ResetMotionSensor()
        {
            base.ResetMotionSensor();

            _pose = Cube.PoseType.Up;
            _doubleTapped = false;
        }

        #endregion

    }
}