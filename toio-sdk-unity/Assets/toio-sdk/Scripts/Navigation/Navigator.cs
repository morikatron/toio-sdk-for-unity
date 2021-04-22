using System.Collections.Generic;
using Vector = toio.MathUtils.Vector;
using static System.Math;


namespace toio.Navigation
{

    public class Navigator
    {
        protected Entity ego;
        public Entity entity => ego;
        public Mode mode = Mode.AVOID;
        // other navigators that can be seen by ego.
        protected List<Navigator> _others = new List<Navigator>();
        protected Dictionary<Navigator, Relation> relations = new Dictionary<Navigator, Relation>();
        // target to run towards or run away from.
        public Boids boids { get; protected set; }
        public HLAvoid avoid { get; protected set; }
        protected List<Wall> walls = new List<Wall>();
        // other entities that will be applied boids alg.
        protected List<Navigator> others {
            get{
                List<Navigator> res = new List<Navigator>();
                foreach (var o in _others){
                    res.Add(o);
                }
                return res;
            }
        }
        protected List<Navigator> boidOthers {
            get{
                List<Navigator> res = new List<Navigator>();
                foreach (var o in _others){
                    if (relations[o] == Relation.BOIDS)
                        res.Add(o);
                }
                return res;
            }
        }

        /// <summary>
        /// Alg. to use.
        /// </summary>
        public enum Mode : byte
        {
            AVOID = 0,
            BOIDS = 1,
            BOIDS_AVOID = 2,
        }
        /// <summary>
        /// relation of ego and other.
        /// </summary>
        public enum Relation : byte
        {
            NONE = 0,
            BOIDS = 1,       // flag to apply boids alg., nouse on AVOID mode
        }

        protected Navigator(Mode mode=Mode.AVOID){this.mode = mode;} // Don't use this.
        public Navigator(Entity ego, Mode mode=Mode.AVOID)
        {
            this.ego = ego; this.mode = mode;
            boids = new Boids(ego); avoid = new HLAvoid(ego);
            AddBorder();
        }

        /// <summary>
        /// Create Default Walls (Borders)
        /// </summary>
        public void AddBorder(int width=20, int x1=40, int x2=460, int y1=40, int y2=460)
        {
            Vector lt = new Vector(x1, y1);
            Vector rt = new Vector(x2, y1);
            Vector lb = new Vector(x1, y2);
            Vector rb = new Vector(x2, y2);
            walls.Add(new Wall(lt, lb, width));    // left
            walls.Add(new Wall(rt, rb, width));    // right
            walls.Add(new Wall(lt, rt, width));    // top
            walls.Add(new Wall(lb, rb, width));    // bottom
        }

        /// <summary>
        /// Inform navigator the existance of other navigators, so as to apply avoiding or boids.
        /// </summary>
        public void AddOther(List<Navigator> others, Relation relation=Relation.BOIDS){
            foreach (var other in others){
                AddOther(other, relation);
            }
        }
        /// <summary>
        /// Inform navigator the existance of other navigator, so as to apply avoiding or boid.
        /// </summary>
        public void AddOther(Navigator other, Relation relation=Relation.BOIDS){
            if (!_others.Contains(other) && this!= other){
                _others.Add(other);
                if (!this.relations.ContainsKey(other)) this.relations.Add(other, relation);
                else this.relations[other] = relation;
            }
        }
        /// <summary>
        /// Inform navigator the existance of other navigators, so as to apply avoiding or boids.
        /// </summary>
        public void RemoveOther(Navigator other){
            if (other == null) return;
            if (_others.Contains(other))
                _others.Remove(other);
            if (this.relations.ContainsKey(other))
                this.relations.Remove(other);
        }
        /// <summary>
        /// Clear Others.
        /// </summary>
        public void ClearOther(){
            _others.Clear();
        }

        /// <summary>
        /// Set the relation between self and other, to decide wether to apply boids alg..
        /// </summary>
        public void SetRelation(Navigator other, Relation relation){
            if (other != null && this.relations.ContainsKey(other))
                this.relations[other] = relation;
        }
        /// <summary>
        /// Set the relation between self and others, to decide wether to apply boids alg..
        /// </summary>
        public void SetRelation(List<Navigator> others, Relation relation){
            foreach (var o in others)
                SetRelation(o, relation);
        }

        /// <summary>
        /// IEnumerable for walls
        /// </summary>
        public System.Collections.Generic.IEnumerable<Wall> Walls()
        {
            foreach (var wall in walls) yield return wall;
        }

        /// <summary>
        /// Add Wall.
        /// </summary>
        public void AddWall(Wall wall){
            if (!walls.Contains(wall))
                walls.Add(wall);
        }
        /// <summary>
        /// Add Walls.
        /// </summary>
        public void AddWall(List<Wall> walls){
            foreach (var wall in walls)
                this.walls.Add(wall);
        }
        /// <summary>
        /// Add Walls.
        /// </summary>
        public void RemoveWall(Wall wall){
            if (wall!=null && walls.Contains(wall))
                walls.Remove(wall);
        }
        /// <summary>
        /// Add Walls.
        /// </summary>
        public void ClearWall(){
            walls.Clear();
        }


        public double p_surrogate_target = 1;
        public double p_speedratio_boidsavoid = 1;
        public double p_speedratio_boids = 1;
        /// <summary>
        /// Get waypoint towards target Entity.
        /// </summary>
        public NaviResult GetWaypointTo(Entity target)
        {
            // AVOID only mode
            switch (mode){
                case Mode.BOIDS:
                {
                    var boidsVector = boids.Run(boidOthers, target.pos);

                    var spdRatio = boidsVector.mag/75*p_speedratio_boids;
                    return new NaviResult(mode, ego.pos, Vector.zero, boidsVector, speedRatio:spdRatio);
                }
                case Mode.BOIDS_AVOID:
                {
                    Vector avoidVector; bool isCol; double spdLimit;

                    // Run Boids to get target position of Boids
                    var boidsVector = boids.Run(boidOthers);
                    var tarPosBoids = target.pos + boidsVector * (target.pos - ego.pos).mag / 100*p_surrogate_target;  // NOTE untuned parameter

                    // Run HLAvoid
                    (avoidVector, isCol, spdLimit) = avoid.RunTowards(
                        others, new Entity(tarPosBoids, target.margin), walls);

                    var spdRatio = Max(0, boidsVector*avoidVector.unit/100*p_speedratio_boidsavoid + 1);
                    return new NaviResult(mode, ego.pos, avoidVector, boidsVector, spdLimit, spdRatio, isCol);
                }
                case Mode.AVOID:
                default:
                {
                    Vector avoidVector; bool isCol; double spdLimit;

                    // Run HLAvoid
                    (avoidVector, isCol, spdLimit) = avoid.RunTowards(others, target, walls);

                    return new NaviResult(mode, ego.pos, avoidVector, Vector.zero, spdLimit, 1, isCol);
                }
            }
        }

        /// <summary>
        /// Get waypoint towards target position x, y.
        /// </summary>
        public NaviResult GetWaypointTo(double x, double y)
        {
            return GetWaypointTo(new Entity(new Vector(x, y)));
        }
        /// <summary>
        /// Get waypoint towards target position Vector.
        /// </summary>
        public NaviResult GetWaypointTo(Vector pos)
        {
            return GetWaypointTo(new Entity(new Vector(pos.x, pos.y)));
        }

        /// <summary>
        /// Get waypoint towards target Navigator.
        /// </summary>
        public NaviResult GetWaypointTo(Navigator navigator)
        {
            return GetWaypointTo(navigator.entity);
        }

        /// <summary>
        /// Get waypoint away from target Entity.
        /// </summary>
        public NaviResult GetWaypointAway(Entity target)
        {
            Vector avoidVector; bool isCol; double spdLimit;

            // Run HLAvoid
            (avoidVector, isCol, spdLimit) = avoid.RunAway(others, target, walls);

            return new NaviResult(mode, ego.pos, avoidVector, Vector.zero, spdLimit, 1, isCol);
        }

        /// <summary>
        /// Get waypoint away from target position x, y.
        /// </summary>
        public NaviResult GetWaypointAway(double x, double y)
        {
            return GetWaypointAway(new Entity(new Vector(x, y)));
        }
        /// <summary>
        /// Get waypoint away from target position Vector.
        /// </summary>
        public NaviResult GetWaypointAway(Vector pos)
        {
            return GetWaypointAway(new Entity(new Vector(pos.x, pos.y)));
        }

        /// <summary>
        /// Get waypoint away from target Navigator.
        /// </summary>
        public NaviResult GetWaypointAway(Navigator navigator)
        {
            return GetWaypointAway(navigator.entity);
        }


        // ==================== Result =====================

        public struct NaviResult
        {
            public Mode mode { get; private set; }
            public Vector avoidVector { get; private set; }
            public Vector boidsVector { get; private set; }

            /// <summary>
            /// Absolute position of waypoint.  Used like: cubeHandle.Move2Target(pos)
            /// </summary>
            public Vector waypoint { get; private set; }

            public double speedRatio { get; private set; }
            public double speedLimit { get; private set; }
            public bool isCollision { get; private set; }

            public NaviResult(Mode mode, Vector egoPos, Vector avoidVector, Vector boidsVector,
                double speedLimit=double.MaxValue, double speedRatio=1, bool isCollision=false)
            {
                this.mode = mode;
                this.avoidVector = avoidVector;
                this.boidsVector = boidsVector;
                this.isCollision = isCollision;

                switch (mode){
                    case Mode.AVOID:
                    case Mode.BOIDS_AVOID:
                        this.waypoint =  egoPos + avoidVector; break;
                    case Mode.BOIDS:
                        this.waypoint =  egoPos + boidsVector; break;
                    default:
                        this.waypoint =  egoPos + Vector.zero; break;
                }

                this.speedRatio = speedRatio;
                this.speedLimit = speedLimit;
            }

        }


    }


}