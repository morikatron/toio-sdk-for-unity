using System;
using System.Collections.Generic;
using UnityEngine;
using static toio.MathUtils.Utils;
using Vector = toio.MathUtils.Vector;
using static System.Math;
using toio.Simulator;


namespace toio
{
    public class CubeHandle
    {
        //////////////////////////////
        //          Fields
        //////////////////////////////

        // --- Hyper-Parameters ---
        public static double TireWidthDot { get { return CubeSimulator.TireWidthDot; } } // (dot)
        public static double VDotOverU { get { return CubeSimulator.VDotOverU; } } // (dot/sec) / cmd
        public static double DotPerM { get { return Mat.DotPerM; } } // dot / mm
        public static readonly float MotorTau = 0.04f;
        public double Deadzone { get; protected set; }
        public int MaxSpd { get; protected set; }
        public static double dt = 1.0 / 60 * 3;
        public static double lag = 0.130;

        // --- Parameters ---
        public int CenterX = 250;
        public int CenterY = 250;
        public int SizeX = 410;
        public int SizeY = 410;
        public int RangeX = 370;
        public int RangeY = 370;

        // --- Properties ---
        public Vector pos { get; protected set; }
        public double x { get; protected set; }
        public double y { get; protected set; }
        public double rad { get; protected set; }
        public double deg { get; protected set; }
        public Vector dir { get; protected set; }
        public int lagMS { get { return (int)(lag*1000); } }
        public int dtMS { get { return (int)(dt*1000); } }

        // --- Prediction ---
        public double spdL, spdR;   // current
        public double radPred, xPred, yPred, spdPredL, spdPredR;    // in lag
        public double stopRadPred, stopXPred, stopYPred;    // stopped
        public double spd { get { return (spdL + spdR) / 2; } }
        public double w { get { return (spdL - spdR) / TireWidthDot; } }
        public Vector v { get { return Vector.fromRadMag(rad, spd); } }
        public Vector posPred { get { return new Vector(xPred, yPred); } }
        public Vector stopPosPred { get { return new Vector(stopXPred, stopYPred); } }
        public double spdPred { get { return (spdPredL + spdPredR) / 2; } }
        public double wPred { get { return (spdPredL - spdPredR) / TireWidthDot; } }
        public Vector vPred { get { return Vector.fromRadMag(rad, spdPred); } }

        protected List<float> sentTimeHist = new List<float>();
        protected List<float> durationHist = new List<float>();
        protected List<double> uLHist = new List<double>();
        protected List<double> uRHist = new List<double>();

        // --- Object ---
        public Cube cube { get; protected set; }


        //////////////////////////////
        //        Constructor
        //////////////////////////////

        public CubeHandle(Cube _cube)
        {
            this.cube = _cube;

            if (this.cube.version == "2.0.0"){
                MaxSpd = 100; Deadzone = 10;
            }
            else{
                MaxSpd = 100; Deadzone = 10;
            }
        }



        //////////////////////////////
        //      Update
        //////////////////////////////

        protected long updateLastFrm = -1;
        /// <summary>
        /// Must call this once at first at each frame Command method is used.
        /// 状態更新、制御のあるフレームに（制御する前に）読んでください。
        /// </summary>
        public virtual void Update()
        {
            var frm = Time.frameCount;
            if (frm == updateLastFrm) return;
            updateLastFrm = frm;

            x = cube.x;
            y = cube.y;
            deg = Deg(cube.angle);
            UpdateProperty();

            Predict();
        }

        protected void UpdateProperty()
        {
            rad = Deg2Rad(deg);
            dir = Vector.fromRadMag(rad, 1);
            pos = new Vector(x, y);
        }

        protected const double p_pred_max_acc = 2000; // max acceleration of tire
        protected const double p_pred_dt = 0.01; // timestep of prediction
        protected long predictLastFrm = -1;
        /// <summary>
        /// Predict current speed, state after lag, state when stopped.
        /// Will update spdPredL, spdPredR, radPred, stopXPred, stopYPred, stopRadPred.
        /// </summary>
        public void Predict()
        {
            var frm = Time.frameCount;
            if (frm == predictLastFrm) return;
            predictLastFrm = frm;

            var now = Time.time;
            var t0 = now - lag;

            xPred = x; yPred = y; radPred = rad;
            double spdL = 0, spdR = 0;
            for (var t = t0 - 5 * p_pred_dt; t < now; t += p_pred_dt)
            {
                // order at t
                double tarL = 0, tarR = 0;
                var sentTimeHistCount = sentTimeHist.Count;
                for (int i = 0; i < sentTimeHistCount; ++i)
                {
                    if (sentTimeHist[i] <= t && (i == sentTimeHistCount - 1 || sentTimeHist[i + 1] > t))
                    {
                        if (sentTimeHist[i] + durationHist[i] > t)
                        {
                            tarL = uLHist[i] * VDotOverU;
                            tarR = uRHist[i] * VDotOverU;
                            break;
                        }
                    }
                }

                // speed at (t+lag), according to order at t.
                spdL += Sign(tarL - spdL) * Min(Abs(tarL - spdL), p_pred_dt * p_pred_max_acc);
                spdR += Sign(tarR - spdR) * Min(Abs(tarR - spdR), p_pred_dt * p_pred_max_acc);

                // save speed at now
                if (Abs(t - t0) < 0.001)
                {
                    this.spdL = spdL; this.spdR = spdR;
                }

                // state at (now+lag)
                if (t >= t0 - 0.001)
                {
                    this.radPred += (float)((spdL - spdR) / TireWidthDot) * p_pred_dt;
                    this.radPred = Rad(this.radPred);
                    this.xPred += Cos(this.radPred) * (spdL + spdR) / 2 * p_pred_dt;
                    this.yPred += Sin(this.radPred) * (spdL + spdR) / 2 * p_pred_dt;
                }
            }

            // speed at (now+lag)
            this.spdPredL = spdL;
            this.spdPredR = spdR;

            // predict stopped state
            stopRadPred = radPred; stopXPred = xPred; stopYPred = yPred;
            for (int i = 0; i < 5; ++i)
            {
                spdL += Sign(0 - spdL) * Min(Abs(0 - spdL), p_pred_dt * p_pred_max_acc);
                spdR += Sign(0 - spdR) * Min(Abs(0 - spdR), p_pred_dt * p_pred_max_acc);

                stopRadPred += (float)((spdL - spdR) / TireWidthDot) * p_pred_dt;
                stopRadPred = Rad(stopRadPred);
                stopXPred += Cos(stopRadPred) * (spdL + spdR) / 2 * p_pred_dt;
                stopYPred += Sin(stopRadPred) * (spdL + spdR) / 2 * p_pred_dt;
            }

            if (Abs(spdPredL) < 2) spdPredL = 0;
            if (Abs(spdPredR) < 2) spdPredR = 0;
        }



        //////////////////////////////
        //     Command - Basic
        // Each call sends 1 order to Cube
        //////////////////////////////

        /// <summary>
        /// Move with order of left/right motors.
        /// Orders are saved for prediction, so use this instead of Cube.move.
        /// </summary>
        public void MoveRaw(double uL, double uR, int durationMs = 1000, Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Weak)
        {
            uL = clip(uL, -MaxSpd, MaxSpd);
            uR = clip(uR, -MaxSpd, MaxSpd);
            if (durationMs < 10)
            {
                if (durationMs <= 5)
                { uL = 0; uR = 0; }
                durationMs = 10;
            }

            cube.Move(ToInt(uL), ToInt(uR), durationMs, order);

            // Save orders for prediction
            var now = Time.time;
            int size = (int)(lag / dt + 4);
            uLHist.Add(uL); uRHist.Add(uR); sentTimeHist.Add(now); durationHist.Add(durationMs/1000f);
            while (uLHist.Count > size) uLHist.RemoveAt(0);
            while (uRHist.Count > size) uRHist.RemoveAt(0);
            while (sentTimeHist.Count > size) sentTimeHist.RemoveAt(0);
            while (durationHist.Count > size) durationHist.RemoveAt(0);
        }

        /// <summary>
        /// Move with translation ordr, rotation order, duration.
        /// 前進指令、回転指令、継続時間で移動
        /// Return actually sent Movement
        /// </summary>
        public Movement Move(double translate, double rotate, int durationMs = 1000,
            bool border = true, Cube.ORDER_TYPE order = Cube.ORDER_TYPE.Weak)
        {

            // transform order. 指令形式変換
            double uL = translate + rotate / 2;
            double uR = translate - rotate / 2;

            // --- Deadzone processing ---
            // Simply add offset to avoid deadzone.
            // 単純に0じゃない指令値をDeadzone外に引っ張り出す
            // {
            //     if (uL > 0) uL += Deadzone;
            //     if (uL < 0) uL -= Deadzone;
            //     if (uR > 0) uR += Deadzone;
            //     if (uR < 0) uR -= Deadzone;
            // }

            // Adjust uneffective uL,uR to nearest effective uL,uR
            // Deadzone外の有効値に一番近い指令にする、回転指令の維持を優先的に考慮
            {

                var l = uL; var r = uR; var M = Max(l, r); var m = Min(l, r);
                if ((l == 0 || l >= Deadzone || l <= -Deadzone) && (r == 0 || r >= Deadzone || r <= -Deadzone))
                {
                    // Outside deadzone
                }
                else if (Abs(l - r) < Deadzone)
                {
                    if ((l + r) > 0) { uL += Deadzone - m; uR += Deadzone - m; }
                    else if ((l + r) < 0) { uL += -Deadzone - M; uR += -Deadzone - M; }
                    else { } //uL=Sign(uL)*Deadzone; uR=Sign(uR)*Deadzone;}
                }
                else if (Abs(l - r) < 2 * Deadzone)
                {
                    if ((l + r) > 0)
                    {
                        if (m < 0) { uL += 0 - m; uR += 0 - m; }
                        else if (m < Deadzone / 2) { uL += 0 - m; uR += 0 - m; }
                        else { uL += Deadzone - m; uR += Deadzone - m; }
                    }
                    else if ((l + r) < 0)
                    {
                        if (M > 0) { uL += 0 - M; uR += 0 - M; }
                        else if (M > -Deadzone / 2) { uL += 0 - M; uR += 0 - M; }
                        else { uL += -Deadzone - M; uR += -Deadzone - M; }
                    }
                    else { uL = Sign(l) * Deadzone; uR = Sign(r) * Deadzone; }
                }
                else
                {
                    if ((l + r) > 0)
                    {
                        if (m < -Deadzone / 2) { uL += -Deadzone - m; uR += -Deadzone - m; }
                        else if (m > Deadzone / 2) { uL += Deadzone - m; uR += Deadzone - m; }
                        else { uL += 0 - m; uR += 0 - m; }
                    }
                    else
                    {
                        if (m < -Deadzone / 2) { uL += -Deadzone - M; uR += -Deadzone - M; }
                        else if (m > Deadzone / 2) { uL += Deadzone - M; uR += Deadzone - M; }
                        else { uL += 0 - M; uR += 0 - M; }
                    }
                }

            }

            // truncate
            if (Max(uL, uR) > MaxSpd)
            {
                uL -= Max(uL, uR) - MaxSpd;
                uR -= Max(uL, uR) - MaxSpd;
            }
            else if (Min(uL, uR) < -MaxSpd)
            {
                uL -= Min(uL, uR) + MaxSpd;
                uR -= Min(uL, uR) + MaxSpd;
            }

            // transform order
            translate = (uL + uR) / 2;
            rotate = uL - uR;

            // --- Border Limitation ---
            // Predict trajectory and cut it before crossing border, by cutting duration.
            // ボーダー制限：ボーダーから出ないようdurationを制限する
            int dur = durationMs;
            if (border)
            {
                // parameters
                double rx = (double)RangeX / 2; double ry = (double)RangeY / 2; double e = 0.4;
                // predicted final stopping state, assuming not output current order.
                double x = this.stopXPred, y = this.stopYPred, rad = this.radPred;
                // predicted state if not stopping.
                double predX = x + Max(spdPred, translate * VDotOverU) * dt * Cos(rad);
                double predY = y + Max(spdPred, translate * VDotOverU) * dt * Sin(rad);
                double predRad = this.radPred;

                // currently outside and going further : stop transition
                if ((Abs(x - CenterX) >= rx || Abs(y - CenterY) >= ry)
                    && (Abs(predX - CenterX) >= rx && Abs(predX - CenterX) >= Abs(x - CenterX)
                        || Abs(predY - CenterY) >= ry && Abs(predY - CenterY) >= Abs(y - CenterY)))
                {
                    // stop
                    translate = 0;

                    // Help rotate back to insider
                    if (Abs(rotate) < 2*Deadzone
                        && (x - CenterX > rx && y - CenterY > ry && (PI - e < rad && rad < PI || PI / 2 - e < -rad && -rad < PI / 2)
                            || x - CenterX > rx && y - CenterY < -ry && (PI - e < -rad && -rad < PI || PI / 2 - e < rad && rad < PI / 2)
                            || x - CenterX < -rx && y - CenterY < -ry && (0 < -rad && -rad < 0 + e || PI / 2 < rad && rad < PI / 2 + e)
                            || x - CenterX < -rx && y - CenterY > ry && (0 < rad && rad < 0 + e || PI / 2 < -rad && -rad < PI / 2 + e)
                            || x - CenterX > rx && Abs(y - CenterY) <= ry && (PI / 2 - e < rad && rad < PI / 2 || PI / 2 - e < -rad && -rad < PI / 2)
                            || x - CenterX < -rx && Abs(y - CenterY) <= ry && (PI / 2 < rad && rad < PI / 2 + e || PI / 2 < -rad && -rad < PI / 2 + e)
                            || y - CenterY > ry && Abs(x - CenterX) <= rx && (0 < rad && rad < e || PI - e < rad && rad < PI)
                            || y - CenterY < -ry && Abs(x - CenterX) <= rx && (0 < -rad && -rad < e || PI - e < -rad && -rad < PI)
                            )
                    ) rotate = 2*Deadzone * Sign(rotate);
                }

                // currently inside : limit duration
                else if (Abs(x - CenterX) < rx && Abs(y - CenterY) < ry)
                {
                    var _dt = 0.05f;
                    var now = Time.time;
                    double spdL = uL * VDotOverU;
                    double spdR = uR * VDotOverU;
                    predX = x; predY = y;
                    for (double t = 0; t < durationMs / 1000f; t += _dt)
                    {
                        predRad += (float)((spdL - spdR) / TireWidthDot) * _dt;
                        predRad = Rad(predRad);
                        predX += Cos(predRad) * (spdL + spdR) / 2 * _dt;
                        predY += Sin(predRad) * (spdL + spdR) / 2 * _dt;
                        if (Abs(predX - CenterX) >= rx || Abs(predY - CenterY) >= ry)
                        {
                            dur = (int)(t * 1000 - _dt * 1000);
                            if (dur < 10) dur = 0;
                            break;
                        }
                    }
                }
            }

            // transform
            uL = translate + rotate / 2;
            uR = translate - rotate / 2;

            MoveRaw(uL, uR, dur, order);
            return new Movement(this, translate, rotate, dur, false, false);
        }

        /// <summary>
        /// Move with Movement.
        /// Return actually sent Movement
        /// </summary>
        public Movement Move(Movement mv, bool border = true)
        {
            if (mv.idle) return mv;
            var mv_ = Move(mv.translate, mv.rotate, mv.durationMs, border, mv.order);
            mv_.reached = mv.reached;
            return mv_;
        }

        /// <summary>
        /// Move with Movement and explicitly given duration.
        /// Return actually sent Movement
        /// </summary>
        public Movement Move(Movement mv, int durationMs, bool border = true)
        {
            if (mv.idle) return mv;
            var mv_ = Move(mv.translate, mv.rotate, durationMs, border, mv.order);
            mv_.reached = mv.reached;
            return mv_;
        }

        /// <summary>
        /// Move with Movement and explicitly given order type.
        /// Return actually sent Movement
        /// </summary>
        public Movement Move(Movement mv, Cube.ORDER_TYPE order, bool border = true)
        {
            if (mv.idle) return mv;
            var mv_ = Move(mv.translate, mv.rotate, mv.durationMs, border, order);
            mv_.reached = mv.reached;
            return mv_;
        }

        /// <summary>
        /// Move with Movement and explicitly given duration and order type.
        /// Return actually sent Movement
        /// </summary>
        public Movement Move(Movement mv, int durationMs, Cube.ORDER_TYPE order, bool border = true)
        {
            if (mv.idle) return mv;
            var mv_ = Move(mv.translate, mv.rotate, durationMs, border, order);
            mv_.reached = mv.reached;
            return mv_;
        }

        /// <summary>
        /// 停止。moveRaw(0,0,100,Cube.ORDER_TYPE.Strong) と等しい。
        /// </summary>
        public void Stop()
        {
            MoveRaw(0, 0, 100, Cube.ORDER_TYPE.Strong);
        }


        //////////////////////////////
        //    Command - Consecutive
        // Closed-Loop Control
        // Call consecutively (e.g. each frame)
        // Return Movement (Call move to excute it)
        //////////////////////////////


        public double p_mv2tar_reach_pred = 0;    // weight of posPred for reach condition
        /// <summary>
        /// Move to target position x, y.
        /// </summary>
        /// <param name="rotateTime">time(ms) is supposed to rotate to target. i.e. the slowness of rotation.</param>
        /// <param name="tolerance">how close should cube be to target, that can be judged "reached".</param>
        public Movement Move2Target(double tarX, double tarY, double maxSpd = 50,
            int rotateTime = 250, double tolerance = 8)
        {
            Vector tar = new Vector(tarX, tarY);
            double dist = this.posPred.distTo(tar);
            double dradPred = Rad((tar - this.posPred).rad - this.radPred);
            double drad = Rad((tar - pos).rad - rad);

            // Reach condition
            if (tar.distTo(this.pos*(1-p_mv2tar_reach_pred)+this.posPred*p_mv2tar_reach_pred) < tolerance)
            {
                return new Movement(this, 0, 0, 100, true);
            }
            else
            {
                // Decelerate at turning.
                // Consider distance
                double endRad = 110 * Min( 1, Max(dist, 20)/Max(1,maxSpd)/VDotOverU /1.5 /(rotateTime/1000.0) );
                double translate = maxSpd * clip(1 - (Abs(drad) - Deg2Rad(10)) / Deg2Rad(endRad), 0, 1);

                // Decelerate as getting close to target.
                translate *= clip(dist, Deadzone * 0.5, Max(maxSpd, Deadzone) * 0.5) / (Max(maxSpd, Deadzone) * 0.5);

                // Angular velocity
                double w = dradPred / (rotateTime / 1000.0);

                double rotate = w * TireWidthDot / VDotOverU;
                // Linearly combine turning raidus method and angular velocity method.
                // Cause turning radius does not work on low "translate",
                // while angular velocity is weak on high "translate".
                double miu = (Abs(translate) / MaxSpd); // weight of turning radius method
                rotate *= Abs(translate / 50) * miu + 1 * (1 - miu);

                return new Movement(this, translate, rotate, rotateTime, false);
            }
        }
        /// <summary>
        /// Move to target position Vector.
        /// </summary>
        public Movement Move2Target(Vector pos, double maxSpd = 50,
            int rotateTime = 250, double tolerance = 8)
        {
            return Move2Target(pos.x, pos.y, maxSpd, rotateTime, tolerance);
        }
        /// <summary>
        /// Move to target position Vector2.
        /// </summary>
        public Movement Move2Target(Vector2 pos, double maxSpd = 50,
            int rotateTime = 250, double tolerance = 8)
        {
            return Move2Target(pos.x, pos.y, maxSpd, rotateTime, tolerance);
        }
        /// <summary>
        /// Move to target position Vector2Int.
        /// </summary>
        public Movement Move2Target(Vector2Int pos, double maxSpd = 50,
            int rotateTime = 250, double tolerance = 8)
        {
            return Move2Target(pos.x, pos.y, maxSpd, rotateTime, tolerance);
        }


        /// <summary>
        /// Rotate to target radian. rotateTime is time supposed to reach target (not equal).
        /// 指定弧度に回転する。rotateTimeは望みの回転時間。
        /// </summary>
        public Movement Rotate2Rad(double tarRad, int rotateTime = 400, double tolerance = 0.1)
        {
            rotateTime = clip(rotateTime, lagMS+dtMS, 2000);

            // Reach condition
            if (AbsRad(tarRad - stopRadPred) < tolerance
                // && AbsRad(tarRad - rad) < tolerance
            )
            {
                return new Movement(this, 0, 0, 100, true);
            }
            else
            {
                double drad = Rad(tarRad - radPred);
                // Angular velocity
                double w = drad / (rotateTime / 1000.0);

                double rotate = w * TireWidthDot / VDotOverU;

                // Deadzone processing
                if (Abs(rotate) < 2 * Deadzone)
                    rotate = 2 * Deadzone * Sign(rotate);

                // Calculate duration
                w = rotate * VDotOverU / TireWidthDot;
                int durationMs = (int)(drad / w * 1000);

                return new Movement(this, 0, rotate, durationMs, false);
            }
        }

        /// <summary>
        /// Rotate to target Degree. rotateTime is time supposed to reach target (not equal).
        /// 指定角度（度）に回転する。rotateTimeは望みの回転時間。
        /// </summary>
        public Movement Rotate2Deg(double tarDeg, int rotateTime = 400, double tolerance = 5)
        {
            return Rotate2Rad(Deg2Rad(tarDeg), rotateTime, Deg2Rad(tolerance));
        }

        /// <summary>
        /// Rotate to face target position. rotateTime is time supposed to reach target (not equal).
        /// 指定座標の方向に回転する。rotateTimeは望みの回転時間。
        /// </summary>
        public Movement Rotate2Target(double tarX, double tarY, int rotateTime = 400, double tolerance = 0.1)
        {
            return Rotate2Rad((new Vector(tarX, tarY) - pos).rad, rotateTime, tolerance);
        }


        //////////////////////////////
        //     Command - One Shot
        // Open-Loop Control
        // Call only once.
        // Return Movement (call Move to excute it)
        //////////////////////////////

        /// <summary>
        /// Advance by given distance and speed(tranlate).
        /// 指定距離、指定前進指令で前進。一回のみ呼んでください。
        /// </summary>
        public Movement TranslateByDist(double dist, double translate)
        {
            if (dist < 0){
                dist = -dist; translate = -translate;
            }
            if (Abs(translate) < Deadzone)
                return new Movement(this, 0, 0, 10, false, order:Cube.ORDER_TYPE.Strong);
            translate = clip(translate, -MaxSpd, MaxSpd);

            // Speed
            var spd = Abs(translate) * VDotOverU;

            // Calculate duration
            int durationMs = (int)(dist / spd * 1000);
            durationMs = Abs(durationMs);

            return new Movement(this, translate, 0, durationMs, false, order:Cube.ORDER_TYPE.Strong);
        }

        /// <summary>
        /// Rotate by given radians and angular velocity (rotate).
        /// 指定角変位（弧度）、指定回転指令で前進。一回のみ呼んでください。
        /// </summary>
        public Movement RotateByRad(double drad, double rotate)
        {
            if (drad < 0){
                drad = -drad; rotate = -rotate;
            }
            if (Abs(rotate) < 2 * Deadzone)
                return new Movement(this, 0, 0, 10, false, order:Cube.ORDER_TYPE.Strong);
            rotate = clip(rotate, -2*MaxSpd, 2*MaxSpd);

            // Angular velocity
            var w = Abs(rotate) * VDotOverU / TireWidthDot;

            // Calculate duration
            int durationMs = (int)(drad / w * 1000);

            return new Movement(this, 0, rotate, durationMs, false, order:Cube.ORDER_TYPE.Strong);
        }

        /// <summary>
        /// Rotate by given degrees and angular velocity (rotate).
        /// 指定角変位（度）、指定回転指令で前進。一回のみ呼んでください。
        /// </summary>
        public Movement RotateByDeg(double ddeg, double rotate)
        {
            return RotateByRad(ddeg / 180 * PI, rotate);
        }
    }


    public struct Movement
    {
        public CubeHandle handle;
        public double translate;
        public double rotate;
        public int durationMs;
        public Cube.ORDER_TYPE order;
        public bool reached;
        public bool idle;  // Movement with idle=true won't be excuted by move()

        public Movement(CubeHandle handle, double translate, double rotate,
            int durationMs = 500, bool reached = false, bool idle = false, Cube.ORDER_TYPE order=Cube.ORDER_TYPE.Weak)
        {
            this.handle = handle;
            this.translate = translate; this.rotate = rotate;
            this.durationMs = durationMs; this.reached = reached; this.idle = idle;
            this.order = order;
        }

        /// <summary>
        /// Execute this movement with/out border
        /// この movement を実行する（ボーダーの有り無しの指定付き）
        /// </summary>
        public Movement Exec(bool border=true)
        {
            if (handle!=null)
                return handle.Move(this, border);
            throw new NullReferenceException("Movement.Exec() called without CubeHandle set.");
        }

        public override string ToString() {
            return $"(hd:{handle}, tr:{translate}, ro:{rotate}, du:{durationMs}, re:{reached}, id:{idle})";
        }
    }
}
