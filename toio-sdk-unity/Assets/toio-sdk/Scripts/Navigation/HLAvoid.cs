using System.Collections.Generic;
using System;
using static toio.MathUtils.Utils;
using Vector = toio.MathUtils.Vector;
using static System.Math;


namespace toio.Navigation
{

    public class HLAvoid
    {
        /// <summary>
        /// FOV of scanning. For smooth motion, fov is set constant.
        /// </summary>
        public readonly double fov = 2*PI-0.02;

        /// <summary>
        /// Range of scanning.
        /// </summary>
        public double range = 200;

        /// <summary>
        /// Number of samples of scanning. odd num suggested.
        /// </summary>
        public int nsample {
            get{ return Max(2, _nsample); }
            set{ _nsample = Max(2, value); }
        }
        private int _nsample = 19;

        public double margin = 22;

        Entity ego;

        public HLAvoid(Entity ego)
        {
            this.ego = ego;
        }

        /// <summary>
        /// Properties: rads, dists, points
        /// Methods: print
        /// </summary>
        public struct ScanResult
        {
            public bool isCollision;

            public double[] rads;
            public double[] dists;
            public double[] safety;
            public Vector[] points;

            private ScanResult(int n)
            {
                isCollision = false;
                rads = new double[n];
                dists = new double[n];
                safety = new double[n];
                points = new Vector[n];
            }
            public static ScanResult init(double[] rads, double maxRange){
                var res = new ScanResult(rads.Length);
                res.rads = rads;

                for (int i=0; i<rads.Length; ++i){
                    res.dists[i] = maxRange;
                    res.safety[i] = 1;
                }
                return res;
            }

            public void Print(Action<string> func){
                System.Text.StringBuilder info = new System.Text.StringBuilder ();
                for (int i=0; i<rads.Length; ++i){
                    info.AppendFormat("{0}, rad={1:#0.00}, dist={2:#0.0}, point={3}, safety={4:#0.00}\n", i, rads[i], dists[i], points[i], safety[i]);
                }
                func(info.ToString());
            }

            public void calcPoints(){
                points = new Vector[rads.Length];
                for (int i=0; i<rads.Length; ++i){
                    points[i] = Vector.fromRadMag(rads[i], Abs(dists[i]));
                }
            }
        }

        /// <summary>
        /// Scan One other entity
        /// </summary>
        private ScanResult _ScanEntity(Navigator other, double[] rads){
            ScanResult res = ScanResult.init(rads, range+1);
            var o = other.entity;
            var marginSum = margin + other.avoid.margin;
            var vmag = Max(ego.spd, 20);

            // Scan for safe distance to other
            for (int i=0; i<rads.Length; ++i)
            {
                var rad = rads[i];
                // var v = Vector.fromRadMag(rad, Max(ego.spd, 10));
                var vx = vmag * Cos(rad); var vy = vmag * Sin(rad);

                // var vUnit = Vector.fromRadMag(rad, 1);

                // var dv = v - o.v;
                var dvx = vx - o.v.x; var dvy = vy - o.v.y;

                // var dPos = ego.pos - o.pos;
                var dPosx = ego.x - o.x;
                var dPosy = ego.y - o.y;
                var dPosmag2 = dPosx * dPosx + dPosy * dPosy;

                // var a = dv * dv;
                var a = dvx * dvx + dvy * dvy;

                // var b = 2 * dv * dPos;
                var b = 2 * (dvx * dPosx + dvy * dPosy);

                // var c = dPos * dPos - marginSum * marginSum;
                var c = dPosmag2 - marginSum * marginSum;

                var delta = b * b - 4 * a * c;

                double dist;
                if (delta < 0 || Abs(a) < 1)    // No collision
                {
                    dist = range+1;
                }
                else
                {
                    var t1 = (-b - Sqrt(delta)) / 2 / a;
                    var t2 = (-b + Sqrt(delta)) / 2 / a;

                    if (t2 <= 0)            // No collision in future
                        dist = range+1;
                    else if (t1 > 0)        // Collsion in future
                        // dist = t1 * v.mag;
                        dist = t1 * vmag;
                    else {                   // Collsion Now
                        // dist = Max(0.1, vUnit * dPos.unit * 100);    // another method
                        // dist = (v*dPos)/vmag/dPos.mag * 100;
                        dist = (vx*dPosx+vy*dPosy)/vmag/Math.Sqrt(dPosmag2) * 100;
                        res.isCollision = true;
                    }
                }

                res.dists[i] = dist;
            }

            // When Collison, Find available rads.
            if (res.isCollision){
                var dPos = o.pos - ego.pos;
                var colSeverity = Max(0, margin+o.margin - dPos.mag)/margin;

                for (int i=0; i<rads.Length; ++i){
                    res.safety[i] = Cos(AbsRad(rads[i] - (-dPos).rad)) * (1 + colSeverity) ;
                }
            }

            return res;
        }

        /// <summary>
        /// Scan list of others. Return 2 scanresults of not-colliding others and colliding others.
        /// </summary>
        private (ScanResult, ScanResult) ScanOthers(List<Navigator> others, double[] rads)
        {
            List<Navigator> othersSeen = new List<Navigator>();
            foreach (var o in others)
            {
                var oe = o.entity;
                if ((ego.pos - oe.pos).mag < range + margin + o.avoid.margin
                    && Abs(ego.Rad2Pos(oe.pos)) < fov/2
                )
                    othersSeen.Add(o);
            }

            List<ScanResult> ress = new List<ScanResult>();
            List<ScanResult> resCols = new List<ScanResult>();

            foreach (var o in othersSeen){
                var reso = _ScanEntity(o, rads);

                if (reso.isCollision) resCols.Add(reso);
                else ress.Add(reso);
            }

            var res = CombineScanRes(ress, false, rads);
            var resCol = CombineScanRes(resCols, true, rads);

            return (res, resCol);
        }

        /// <summary>
        /// Scan one wall.
        /// </summary>
        private ScanResult _ScanWall(Wall wall, double[] rads){

            ScanResult res = ScanResult.init(rads, range+1);
            var egopos = ego.pos;

            var castTry = new CircleCast(egopos, 0, margin);
            var hitTry = castTry.CastTo(wall);

            // currently in collision
            if (hitTry.isOriginHit)
            {
                res.isCollision = true;

                var dist2wall = wall.DistToPoint_TruncatedByEnds(egopos);
                var colSeverity = Max(0, margin + wall.margin - dist2wall) / margin;

                for (int i=0; i<rads.Length; ++i){
                    var rad = rads[i];
                    var repulsionDir = wall.RepulsionDirToPoint(egopos);

                    if (useSafety){
                        res.safety[i] = Cos(AbsRad(rads[i] - repulsionDir.rad)) * (1 + colSeverity);
                    }
                    else{
                        var dist = Max(0.1, Vector.fromRadMag(rad,1) * repulsionDir * 100);
                        res.dists[i] = dist;
                    }
                }
            }
            // not in collision
            else{
                for (int i=0; i<rads.Length; ++i){
                    var rad = rads[i];
                    var cast = new CircleCast(egopos, rad, margin);
                    var hit = cast.CastTo(wall);

                    // No hit
                    if (!hit.isHit) continue;

                    // Hit
                    var dist = hit.dist;
                    if (dist < res.dists[i]){
                        res.dists[i] = dist;
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Scan list of walls. Return 2 scanresults of not-colliding walls and colliding walls.
        /// </summary>
        private (ScanResult, ScanResult) ScanWalls(List<Wall> walls, double[] rads)
        {
            List<ScanResult> ress = new List<ScanResult>();
            List<ScanResult> resCols = new List<ScanResult>();

            foreach (var wall in walls){
                var lineDist = wall.DistToPoint(ego.pos);

                if (lineDist-wall.margin > range+1) continue;      // out of range

                var reso = _ScanWall(wall, rads);

                if (reso.isCollision) resCols.Add(reso);
                else ress.Add(reso);
            }

            var res = CombineScanRes(ress, false, rads);
            var resCol = CombineScanRes(resCols, true, rads);

            return (res, resCol);
        }

        /// <summary>
        /// Scan target.
        /// </summary>
        private ScanResult ScanTar(Entity target, double[] rads)
        {
            // target as a Point
            var res = ScanResult.init(rads, range+1);
            var dPos = target.pos - ego.pos;
            var tarRad = dPos.rad;

            for (int i=0; i<rads.Length; ++i)
            {
                if (rads[i] == tarRad)
                {
                    res.dists[i] = dPos.mag;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// sampling radians
        /// </summary>
        private double[] SampleRads(Entity target)
        {
            double[] rads = new double[nsample+1];
            var radTar = (target.pos - ego.pos).rad;

            for (int i=0; i<nsample; ++i)
            {
                var radi = ego.rad - fov/2 + fov / (nsample - 1) * i;

                var dradTar = Rad(radTar-ego.rad);
                var dradi = Rad(radi - ego.rad);
                if ( dradTar > dradi )
                    rads[i] = radi;
                else if (dradTar + fov / (nsample - 1) > dradi){
                    rads[i] = radTar;
                    rads[i+1] = radi;
                }
                else
                    rads[i+1] = radi;
            }

            return rads;
        }

        /// <summary>
        /// Scan others, target, walls.
        /// </summary>
        private (ScanResult, ScanResult) Scan(List<Navigator> others, Entity target, List<Wall> walls, double[] rads){

            // scan target
            var resTar = ScanTar(target, rads);

            // scan others
            var (resOthers, resOthersCol) = ScanOthers(others, rads);

            // scan walls
            var (resWalls, resWallsCol) = ScanWalls(walls, rads);

            // Combine scanning results : min between tar dist and other dist
            List<ScanResult> ress = new List<ScanResult>();
            ress.Add(resTar); ress.Add(resOthers); ress.Add(resWalls);
            List<ScanResult> resCols = new List<ScanResult>();
            resCols.Add(resOthersCol); resCols.Add(resWallsCol);

            var res = CombineScanRes(ress, false, rads);
            var resCol = CombineScanRes(resCols, true, rads);

            return (res, resCol);
        }

        /// <summary>
        /// Combine list of ScanResult to one ScanResult
        /// </summary>
        private ScanResult CombineScanRes(List<ScanResult> results, bool isCol, double[] rads){
            if (isCol){
                if (results.Count == 0) return ScanResult.init(rads, range+1);
                ScanResult resCol = ScanResult.init(rads, range+1);

                foreach (var reso in results){
                    if (reso.isCollision){
                        resCol.isCollision = true;
                        for (int i=0; i<rads.Length; ++i){
                            if (resCol.dists[i] > reso.dists[i]){
                                resCol.dists[i] = reso.dists[i];
                            }
                            if (resCol.safety[i] > reso.safety[i])
                                resCol.safety[i] = reso.safety[i];
                        }
                    }
                }
                return resCol;
            }
            else{
                if (results.Count == 0) return ScanResult.init(rads, range+1);
                ScanResult res = ScanResult.init(rads, range+1);

                foreach (var reso in results){
                    for (int i=0; i<rads.Length; ++i){
                        if (reso.dists[i] < res.dists[i]){
                            res.dists[i] = reso.dists[i];
                        }
                    }
                }
                return res;
            }
        }


        //////////////////////////
        ///      RUNNER        ///
        //////////////////////////

        // Save the results for extended analysis from outside.
        public ScanResult scanResult;
        public int waypointIndex = 0;
        public bool useSafety = true;

        public double p_waypoint_safety_threshold = 0.15;

        /// <summary>
        /// Run towards target.
        /// </summary>
        public (Vector, bool, double) RunTowards(List<Navigator> others, Entity target, List<Wall> walls)
        {
            // ====  Scan  ====
            var rads = SampleRads(target);

            var (res, resCol) = Scan(others, target, walls, rads);
            res.calcPoints(); resCol.calcPoints();


            // ====  Choose Waypoint  ====
            // find waypoint by res, which is the scan result of objects NOT in collision
            waypointIndex = 0;
            Vector waypoint = Vector.zero;

            // evaluate "distance" for each rad
            List<double> dists = new List<double>();
            for (int i=0; i<rads.Length; ++i)
            {
                // nearest point at certain rad
                // var targetDist = ego.pos.distTo(target.pos);
                var targetDist = Math.Sqrt((ego.x-target.x)*(ego.x-target.x) + (ego.y-target.y)*(ego.y-target.y));
                var nearestMag = Max( Cos((target.pos-ego.pos).rad - rads[i]) * targetDist, 1 );
                var mv = res.points[i].clip(nearestMag);
                res.points[i] = mv;

                // distance of waypoint to target
                var dist = (ego.pos + mv).distTo(target.pos);

                // penalty of turning
                // NOTE may cause not turning when target right behind ego
                dist += Max(AbsRad(ego.rad - rads[i]), 0.5) * 0.1;
                dist += Max((AbsRad(ego.rad - rads[i]) - PI/2), 0) * 2;

                dists.Add(dist);
            }

            // choose best waypoint
            if (useSafety)  // combine with safety
            {
                double minDist = double.MaxValue;
                for (int i=0; i<rads.Length; ++i)
                {
                    var dist = dists[i];

                    // Search Nearest waypoint away from target, with SAFE direction
                    if (dist < minDist && resCol.safety[i] > p_waypoint_safety_threshold)
                    {
                        minDist = dist; waypointIndex = i;
                    }
                }

                if (minDist < double.MaxValue){
                    waypoint = res.points[waypointIndex];
                }
                else{
                    // Debug.Log("Stuck");
                }
            }
            else    // with force from collided entity
            {
                if (!resCol.isCollision)   // Yet no collision.
                {
                    double minDist = double.MaxValue;
                    for (int i=0; i<rads.Length; ++i)
                    {
                        var dist = dists[i];

                        // Search Nearest waypoint away from target
                        if (dist < minDist)
                        {
                            minDist = dist; waypointIndex = i;
                        }
                    }
                    waypoint = res.points[waypointIndex];
                }
                else                    // Now collision
                {
                    var maxDist = resCol.dists[0];
                    for (int i=0; i<rads.Length; ++i)
                    {
                        var d = resCol.dists[i];
                        if (d > maxDist)
                        {
                            maxDist = d; waypointIndex = i;
                        }
                    }
                    waypoint = resCol.points[waypointIndex];
                }
            }



            // ====  Speed Limit  ====
            double speedLimit = double.MaxValue;
            var waypointRad = rads[waypointIndex];
            // slow down when colliders are close around.
            {
                double minRadius = 1e10;
                for (int i=0; i<rads.Length; ++i){
                    var rad = rads[i];
                    if (res.dists[i] < 50 &&
                        (Rad(rad-ego.rad)>0.1 && Rad(waypointRad-rad)>0.1 ||
                         Rad(ego.rad-rad)>0.1 && Rad(rad-waypointRad)>0.1 ))
                    {
                        // minimal maximal turning radius
                        minRadius = Min(minRadius, (res.dists[i]+20) / (2* Sin(Abs(rad-ego.rad))) );
                    }
                }
                speedLimit = Max(8, minRadius * Max(1, Abs(ego.w)));
            }
            // stop when ego.rad is pointing insides colliders.
            if (resCol.isCollision)
            {
                for (int i=0; i<rads.Length; ++i){
                    var rad = rads[i];
                    if (AbsRad(rad - ego.rad) < PI*2/nsample
                        && resCol.safety[i] <= 0
                    ){
                        speedLimit = 100 * Max(0, resCol.safety[i] * 2 +1);
                        break;
                    }
                }
            }


            // ==== make result ====
            res.rads = rads;
            res.isCollision = resCol.isCollision;
            if (resCol.isCollision && useSafety) Array.Copy(resCol.safety, res.safety, res.safety.Length);
            scanResult = res;
            return (waypoint, resCol.isCollision, speedLimit);
        }


        public double p_runaway_penalty_away_k = 5;
        public double p_runaway_penalty_keeprad_k = 10;
        public double p_runaway_range = 250;
        /// <summary>
        /// Runaway from target.
        /// </summary>
        public (Vector, bool, double) RunAway(List<Navigator> others, Entity target, List<Wall> walls)
        {
            // ====  Scan  ====
            var rads = SampleRads(target);

            double oppoRad = (ego.pos-target.pos).rad;
            int oppoIndex = (int)((oppoRad - (ego.rad-fov/2))/(fov/(nsample-1)));

            var (res, resCol) = Scan(others, target, walls, rads);
            res.calcPoints(); resCol.calcPoints();


            // ====  Choose Waypoint  ====
            // find waypoint by res, which is the scan result of objects NOT in collision
            waypointIndex = 0;
            Vector waypoint = Vector.zero;

            {
                double maxDist = 0;
                for (int i=0; i<rads.Length; ++i)
                {
                    // distance of waypoint to target
                    var point = res.points[i] * p_runaway_range/range + ego.pos;
                    var dist = (target.pos + target.v*0.5).distTo(point);
                    var dist_penalty = dist;

                    // Soft penalty : away from target (predicted)
                    dist_penalty -= (-PI + AbsRad(rads[i] - (ego.pos-target.pos-target.v*0.2).rad)) * p_runaway_penalty_away_k;

                    // Soft penalty : keep self's direction
                    dist_penalty -= (-PI + AbsRad(rads[i] - ego.rad)) * p_runaway_penalty_keeprad_k;

                    // Hard penalty
                    var drad = AbsRad((rads[i] -  (ego.pos-target.pos-target.v*0.0).rad));
                    double danger_fov = PI/4-0.15;// + 5/(Max(ego.pos.distTo(target.pos)-20, 20)) ;   // greater than pi/4 make ego not able to leave corner
                    if ( drad - PI + danger_fov > 0 ) dist_penalty = 0;

                    // Search Furthest waypoint away from target, with SAFE direction
                    if (maxDist < dist_penalty && resCol.safety[i] > p_waypoint_safety_threshold){
                        maxDist = dist_penalty;
                        waypointIndex = i;
                    }
                }

                if (maxDist > 0 ){
                    waypoint = res.points[waypointIndex];
                }
                else{
                    // Debug.Log("Stuck");
                }
            }


            // ====  Speed Limit  ====
            double speedLimit = double.MaxValue;
            var waypointRad = rads[waypointIndex];
            // slow down when colliders are close around.
            {
                double minRadius = 1e10;
                for (int i=0; i<rads.Length; ++i){
                    var rad = rads[i];
                    if (Rad(rad-ego.rad)>0.1 && Rad(waypointRad-rad)>0.1 ||
                        Rad(ego.rad-rad)>0.1 && Rad(rad-waypointRad)>0.1 )
                    {
                        // minimal maximal turning radius
                        minRadius = Min(minRadius, (res.dists[i]+20) / (2* Sin(Abs(rad-ego.rad))) );
                    }
                }
                speedLimit = Max(8, minRadius * Max(1, Abs(ego.w)));
            }
            // stop when ego.rad is pointing insides colliders.
            if (resCol.isCollision)
            {
                for (int i=0; i<rads.Length; ++i){
                    var rad = rads[i];
                    if (AbsRad(rad - ego.rad) < PI*2/nsample
                        && resCol.safety[i] <= 0
                    ){
                        speedLimit = 100 * Max(0, resCol.safety[i] * 2 +1);
                        break;
                    }
                }
            }


            // ==== make result ====
            res.rads = rads;
            res.isCollision = resCol.isCollision;
            if (resCol.isCollision && useSafety) Array.Copy(resCol.safety, res.safety, res.safety.Length);
            scanResult = res;
            return (waypoint, resCol.isCollision, speedLimit);
        }

    }

}
