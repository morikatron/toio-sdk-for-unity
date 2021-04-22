using System.Collections.Generic;
using UnityEngine;
using Vector = toio.MathUtils.Vector;
using static System.Math;


namespace toio.Navigation{

    public class CubeEntity : Entity
    {
        public void Update(bool usePred=false){
            if (usePred)
            {
                x = handle.xPred;
                y = handle.yPred;
                pos = new Vector(x, y);
                rad = handle.radPred;
                spdL = handle.spdPredL;
                spdR = handle.spdPredR;
                spd = handle.spdPred;
                w = handle.wPred;
                v = handle.vPred;
            }
            else{
                x = handle.x;
                y = handle.y;
                pos = handle.pos;
                rad = handle.rad;
                spdL = handle.spdL;
                spdR = handle.spdR;
                spd = handle.spd;
                w = handle.w;
                v = handle.v;
            }
        }

        CubeHandle handle;

        /// <summary>
        /// margin >= 16, which is the real size (dot)
        /// </summary>
        public CubeEntity(CubeHandle handle, double margin=16)
        {
            this.handle = handle; this.margin = margin;
        }

    }

    /// <summary>
    /// Subclass of Navigator for easy use.
    /// - Pack up CubeHandle's and Navigator's update.
    /// - Accessable to Cubehandle and Cube.
    /// - Pack up Navigator's waypoint calc. and Cubehandle's Movement calc.
    /// - Auto appending of others.
    /// </summary>
    public class CubeNavigator : Navigator
    {
        public Cube cube{ get; private set; }
        public CubeHandle handle{ get; private set; }
        public NaviResult result{ get; private set; }
        private static List<Navigator> gNavigators = new List<Navigator>();

        public bool usePred = false;

        public CubeNavigator(Cube cube, Mode mode=Mode.AVOID)
        {
            this.cube = cube; this.mode = mode;
            this.handle = new CubeHandle(cube);
            this.ego = new CubeEntity(handle);
            this.boids = new Boids(ego); this.avoid = new HLAvoid(ego);

            // Add borders
            AddBorder(20);

            // Auto appending of others' _other
            foreach (var o in gNavigators)
                o.AddOther(this);
            this.AddOther(gNavigators);
            gNavigators.Add(this);
        }
        public CubeNavigator(CubeHandle handle, Mode mode=Mode.AVOID)
        {
            this.cube = handle.cube; this.mode = mode;
            this.handle = handle;
            this.ego = new CubeEntity(handle);
            this.boids = new Boids(ego); this.avoid = new HLAvoid(ego);

            // Add borders
            AddBorder(20);

            // Auto appending of others' _other
            foreach (var o in gNavigators)
                o.AddOther(this);
            this.AddOther(gNavigators);
            gNavigators.Add(this);
        }

        private float updateLastTime = 0;
        /// <summary>
        /// Must run this at each controlling frame to update state of entity.
        /// </summary>
        public void Update(bool usePred)
        {
            if (Time.time - updateLastTime < 0.015) return;
            updateLastTime = Time.time;
            this.handle.Update();
            (ego as CubeEntity).Update(usePred);
        }
        /// <summary>
        /// Must run this at each controlling frame to update state of entity.
        /// </summary>
        public void Update()
        {
            if (Time.time - updateLastTime < 0.015) return;
            updateLastTime = Time.time;
            this.handle.Update();
            (ego as CubeEntity).Update(this.usePred);
        }


        /// <summary>
        /// Navigate to target position with x, y
        /// </summary>
        public virtual Movement Navi2Target(double x, double y, int maxSpd=70, int rotateTime=250, double tolerance=20)
        {
            this.result = base.GetWaypointTo(x, y);
            var spd = Min(this.result.speedLimit, maxSpd*this.result.speedRatio);
            var mv = handle.Move2Target(this.result.waypoint,
                maxSpd:spd, rotateTime:rotateTime, tolerance:8
            );

            if (ego.pos.distTo(new Vector(x,y)) <= tolerance && mv.reached)
                return mv;
            else{
                mv.reached = false; return mv;
            }
        }
        /// <summary>
        /// Navigate to target position Vector
        /// </summary>
        public virtual Movement Navi2Target(Vector pos, int maxSpd=70, int rotateTime=250, double tolerance=20)
        {
            return Navi2Target(pos.x, pos.y,
                maxSpd:maxSpd, rotateTime:rotateTime, tolerance:tolerance);
        }
        /// <summary>
        /// Navigate to target position Vector2
        /// </summary>
        public virtual Movement Navi2Target(Vector2 pos, int maxSpd=70, int rotateTime=250, double tolerance=20)
        {
            return Navi2Target(pos.x, pos.y,
                maxSpd:maxSpd, rotateTime:rotateTime, tolerance:tolerance);
        }
        /// <summary>
        /// Navigate to target position Vector2Int
        /// </summary>
        public virtual Movement Navi2Target(Vector2Int pos, int maxSpd=70, int rotateTime=250, double tolerance=20)
        {
            return Navi2Target(pos.x, pos.y,
                maxSpd:maxSpd, rotateTime:rotateTime, tolerance:tolerance);
        }

        /// <summary>
        /// Navigate away from target position x, y
        /// </summary>
        public virtual Movement NaviAwayTarget(double x, double y, int maxSpd=70, int rotateTime=250)
        {
            this.result = base.GetWaypointAway(x, y);
            var spd = Min(this.result.speedLimit, maxSpd*this.result.speedRatio);
            var mv = handle.Move2Target(this.result.waypoint,
                maxSpd:spd, rotateTime:rotateTime, tolerance:8);
            return mv;
        }
        /// <summary>
        /// Navigate away from target position Vector
        /// </summary>
        public virtual Movement NaviAwayTarget(Vector pos, int maxSpd=70, int rotateTime=250)
        {
            return NaviAwayTarget(pos.x, pos.y,
                maxSpd:maxSpd, rotateTime:rotateTime);
        }
        /// <summary>
        /// Navigate away from target position Vector2
        /// </summary>
        public virtual Movement NaviAwayTarget(Vector2 pos, int maxSpd=70, int rotateTime=250)
        {
            return NaviAwayTarget(pos.x, pos.y,
                maxSpd:maxSpd, rotateTime:rotateTime);
        }
        /// <summary>
        /// Navigate away from target position Vector2Int
        /// </summary>
        public virtual Movement NaviAwayTarget(Vector2Int pos, int maxSpd=70, int rotateTime=250)
        {
            return NaviAwayTarget(pos.x, pos.y,
                maxSpd:maxSpd, rotateTime:rotateTime);
        }

        /// <summary>
        /// Get waypoint towards target position Vector2.
        /// </summary>
        public NaviResult GetWaypointTo(Vector2 pos)
        {
            return GetWaypointTo(pos.x, pos.y);
        }
        /// <summary>
        /// Get waypoint towards target position Vector2Int.
        /// </summary>
        public NaviResult GetWaypointTo(Vector2Int pos)
        {
            return GetWaypointTo(pos.x, pos.y);
        }

        /// <summary>
        /// Get waypoint away from target position Vector2.
        /// </summary>
        public NaviResult GetWaypointAway(Vector2 pos)
        {
            return GetWaypointAway(pos.x, pos.y);
        }
        /// <summary>
        /// Get waypoint away from target position Vector2Int.
        /// </summary>
        public NaviResult GetWaypointAway(Vector2Int pos)
        {
            return GetWaypointAway(pos.x, pos.y);
        }

        /// <summary>
        /// Inform navigator the existance of other navigators, so as to apply avoiding or boids.
        /// </summary>
        public void AddOther(List<CubeNavigator> others, Relation relation=Relation.BOIDS)
        {
            foreach (var other in others){
                AddOther(other, relation);
            }
        }


        /// <summary>
        /// Set the relation between self and others, to decide wether to apply boids alg..
        /// </summary>
        public void SetRelation(List<CubeNavigator> others, Relation relation){
            foreach (var o in others)
                SetRelation(o, relation);
        }

        /// <summary>
        /// Clear global (static) Navigator list.
        /// Use this when discarding created Navigators.
        /// </summary>
        public static void ClearGNavigators()
        {
            gNavigators.Clear();
        }

        /// <summary>
        /// Create Default Walls (Borders)
        /// </summary>
        public void AddBorder(int width, RectInt rect){
            Vector lt = new Vector(rect.xMin, rect.yMin);
            Vector rt = new Vector(rect.xMax, rect.yMin);
            Vector lb = new Vector(rect.xMin, rect.yMax);
            Vector rb = new Vector(rect.xMax, rect.yMax);
            walls.Add(new Wall(lt, lb, width));    // left
            walls.Add(new Wall(rt, rb, width));    // right
            walls.Add(new Wall(lt, rt, width));    // top
            walls.Add(new Wall(lb, rb, width));    // bottom
        }

    }

}