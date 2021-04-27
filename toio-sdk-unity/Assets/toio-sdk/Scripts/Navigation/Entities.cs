using System;
using static toio.MathUtils.Utils;
using Vector = toio.MathUtils.Vector;
using UnityEngine;

namespace toio.Navigation
{

    public class Entity
    {
        public double x;
        public double y;
        public double rad;
        public double spdL;
        public double spdR;
        public double w;

        public double margin;

        public Vector pos;
        public double spd;
        public Vector v;

        public Entity(){}
        public Entity(Vector _pos){
            pos = _pos; x = _pos.x; y = _pos.y;
        }
        public Entity(Vector _pos, double _margin){
            pos = _pos; x = _pos.x; y = _pos.y;
            margin = _margin;
        }

        public double Rad2Pos(Vector tarPos)
        {
            var radAbs = (tarPos - pos).rad;
            return Rad(radAbs - rad);
        }
    }


    public struct CircleCastHit
    {
        public bool isHit;
        public bool isOriginHit;
        public Vector originCirclePos;
        public Vector hitCirclePos;
        public Vector contactPos;
        public double dist { get {
            if (isHit) return originCirclePos.distTo(hitCirclePos);
            else return -1;
        }}

        public CircleCastHit(
            bool isHit, bool isOriginHit,
            Vector originCirclePos, Vector hitCirclePos, Vector contactPos
        ){
            this.isHit = isHit;
            this.isOriginHit = isOriginHit;
            this.originCirclePos = originCirclePos;
            this.hitCirclePos = hitCirclePos;
            this.contactPos = contactPos;
        }
        public static CircleCastHit NotHit(Vector origin)
        {
            return new CircleCastHit(
                isHit: false, isOriginHit: false,
                originCirclePos: origin,
                hitCirclePos:Vector.Null, contactPos:Vector.Null
            );
        }
        public bool EarlierThan(CircleCastHit hit)
        {
            if (!isHit) return false;
            if (!hit.isHit) return true;
            return dist < hit.dist;
        }
    }

    public struct CircleCast
    {
        public Vector origin;
        public double rad;
        public double r;
        public Vector dir { get { return new Vector(Math.Cos(rad), Math.Sin(rad));} }

        public CircleCast(Vector origin, double rad, double r)
        {
            this.origin = origin;
            this.rad = rad;
            this.r = r;
        }

        public CircleCastHit CastTo(IDetectable obj)
        {
            return obj.CircleCastHandle(this);
        }

        public LineBase GetLineBase()
        {
            return new LineBase(origin, origin+dir, point2end:false);
        }
    }


    public interface IDetectable
    {
        CircleCastHit CircleCastHandle(CircleCast cast);
    }


    public class LineBase
    {
        public double a {get; protected set;}
        public double b {get; protected set;}
        public double c {get; protected set;}
        public Vector point1 {get; protected set;}
        public Vector point2 {get; protected set;}
        public bool point1end {get; protected set;}
        public bool point2end {get; protected set;}
        public Vector norm { get{ return new Vector(a, b); } }

        public LineBase(Vector point1, Vector point2, bool point1end=true, bool point2end=true){
            this.point1 = point1;
            this.point2 = point2;
            this.point1end = point1end;
            this.point2end = point2end;

            // Calculate a,b,c of ax+by+c=0
            if (point1 == point2) throw new ArgumentException("point1 and point2 cannot be the same");
            else if (point1.x == point2.x){
                b = 0;
                if (point1.x == 0){
                    a = 1; c = 0;
                }
                else {
                    c = -point1.x;
                    a = 1;
                }
            }
            else if (point1.y == point2.y){
                a = 0;
                if (point1.y == 0){
                    b = 1; c = 0;
                }
                else {
                    c = -point1.y;
                    b = 1;
                }
            }
            else {
                c = point1.x*point2.y - point2.x*point1.y;
                a = point1.y-point2.y;
                b = point2.x-point1.x;
                var mag = Math.Sqrt(a*a+b*b);
                a /= mag; b /= mag; c /= mag;
            }
        }

        // public LineBase(double a, double b, double c){
        //     if (a ==0 && b == 0) throw new ArgumentException("a, b cannot be both 0");
        //     this.a = a; this.b = b; this.c = c;
        //     var mag = Math.Sqrt(a*a+b*b);
        //     this.a /= mag; this.b /= mag; this.c /= mag;
        // }

        public bool FootOnThis(Vector point)
        {
            bool insideOfEnd1 = !this.point1end || (point-this.point1)*(this.point2-this.point1)>=0;
            bool insideOfEnd2 = !this.point2end || (point-this.point2)*(this.point1-this.point2)>=0;
            return insideOfEnd1 && insideOfEnd2;
        }

        public double DistToPoint(Vector point){
            return Math.Abs(norm * point + c);
        }
        public double SignedDistToPoint(Vector point){
            return norm * point + c;
        }
        public double DistToPoint_TruncatedByEnds(Vector point){
            if (FootOnThis(point)) return DistToPoint(point);
            double d1=-1, d2=-1;
            if (point1end) d1 = point1.distTo(point);
            if (point2end) d2 = point2.distTo(point);
            if (d1 < d2 && d1 >= 0 || d2 < 0) return d1;
            return d2;
        }

        public Vector RepulsionDirToPoint(Vector point)
        {
            if (FootOnThis(point))
                return Math.Sign(SignedDistToPoint(point)) * norm;
            double d1=-1, d2=-1;
            if (point1end) d1 = point1.distTo(point);
            if (point2end) d2 = point2.distTo(point);
            if (d1 < d2 && d1 >= 0 || d2 < 0)
                return (point-point1).unit;
            return (point-point2).unit;
        }

        public virtual Vector Intersect(LineBase line){
            // Calc. intersection if 2 lines are both infinite.
            var cc = a * line.b - line.a * b;
            if (cc == 0) return Vector.Null;
            var aa = b*line.c - line.b*c;
            var bb = line.a*c - a*line.c;
            var sec = new Vector(aa/cc, bb/cc);

            // Parallel
            if (sec.isNull) return sec;
            // Intersection on both line
            if (this.FootOnThis(sec) && line.FootOnThis(sec))
                return sec;
            // No Intersection
            return Vector.Null;
        }

        public Vector FootPoint(Vector point){
            var n = norm;
            return point - (n * point + c) * n;
        }

        protected LineBase MovedAlongNormToPointSide(Vector point, Double displacement)
        {
            // on which side of wall ego is
            var normSign = Math.Sign(norm*point + c);
            var step = normSign * displacement * norm;
            return new LineBase(point1+step, point2+step, point1end, point2end);
        }
    }

    public class Point: IDetectable
    {
        public double x, y;
        public Vector pos { get {return new Vector(x, y); }}
        public Point(double x, double y)
        {
            this.x = x; this.y = y;
        }
        public Point(Vector pos)
        {
            this.x = pos.x; this.y = pos.y;
        }

        public virtual CircleCastHit CircleCastHandle(CircleCast cast)
        {
            // Already collided at origin position
            if (pos.distTo(cast.origin) < cast.r)
            {
                return new CircleCastHit
                (
                    isHit:true, isOriginHit:true,
                    originCirclePos:cast.origin, hitCirclePos:cast.origin, contactPos:pos
                );
            }
            // Detect
            var castLine = cast.GetLineBase();
            if (castLine.FootOnThis(pos)){
                var dist = castLine.DistToPoint(pos);
                if (dist <= cast.r)
                {
                    var distContactFoot = Math.Sqrt(cast.r*cast.r - dist*dist);
                    var foot1 = castLine.FootPoint(pos);
                    var distOriginFoot = (foot1-cast.origin) * cast.dir;
                    var distOriginContact = distOriginFoot - distContactFoot;
                    var hit = cast.origin + cast.dir * distOriginContact;
                    return new CircleCastHit
                    (
                        isHit:true, isOriginHit:false, originCirclePos:cast.origin,
                        hitCirclePos:hit, contactPos:pos
                    );
                }
            }

            // No Collision
            return CircleCastHit.NotHit(cast.origin);
        }
    }

    public class Line: LineBase, IDetectable
    {
        public Line(Vector point1, Vector point2) : base(point1, point2) {}

        protected CircleCastHit CircleCastHandle_ExceptEnds(CircleCast cast)
        {
            if (this.DistToPoint(cast.origin) < cast.r)
            {
                // Already collided at origin position
                if (this.FootOnThis(cast.origin))
                    return new CircleCastHit
                    (
                        isHit:true, isOriginHit:true, originCirclePos:cast.origin,
                        hitCirclePos:cast.origin, contactPos:FootPoint(cast.origin)
                    );
                // Except Collide Inside out
                else
                    return CircleCastHit.NotHit(cast.origin);
            }

            // Detect collision between ends
            var marginLine = this.MovedAlongNormToPointSide(cast.origin, cast.r);
            var castLine = cast.GetLineBase();
            var sec = castLine.Intersect(marginLine);
            if (!sec.isNull)
            {
                return new CircleCastHit
                (
                    isHit:true, isOriginHit:false, originCirclePos:cast.origin,
                    hitCirclePos:sec, contactPos:FootPoint(sec)
                );
            }
            // No Collision
            return CircleCastHit.NotHit(cast.origin);
        }
        protected CircleCastHit CircleCastHandle_Ends(CircleCast cast)
        {
            Point p1 = new Point(point1);
            Point p2 = new Point(point2);
            var hit1 = p1.CircleCastHandle(cast);
            var hit2 = p2.CircleCastHandle(cast);
            if (hit1.EarlierThan(hit2)) return hit1;
            return hit2;
        }

        public virtual CircleCastHit CircleCastHandle(CircleCast cast)
        {
            var hit_between = CircleCastHandle_ExceptEnds(cast);
            if (hit_between.isHit) return hit_between;

            var hit_ends = CircleCastHandle_Ends(cast);
            return hit_ends;
        }
    }

    // public class Circle: IDetectable
    // {
    //     public double x, y;
    //     public double r;
    //     public Vector pos { get {return new Vector(x, y); }}
    //     public Circle(double x, double y, double r)
    //     {
    //         this.x = x; this.y = y; this.r = r;
    //     }
    //     public Circle(Vector pos, double r)
    //     {
    //         this.x = pos.x; this.y = pos.y; this.r = r;
    //     }

    //     public CircleCastHit CirleCastHandle(CircleCast cast)
    //     {

    //     }
    // }

    // TODO
    // public class Rect: IDetectable

    public class Wall: Line    // TODO Rect
    {
        public double margin {get; set;}

        public Wall(Vector point1, Vector point2, double margin=0) : base(point1, point2){
            this.margin = margin;
        }
        public Wall(double x1, double y1, double x2, double y2, double margin=0) :
            base(new Vector(x1, y1), new Vector(x2, y2)){
            this.margin = margin;
        }
        // public Wall(double a, double b, double c, double margin=0) : base(a, b, c){
        //     this.margin = margin;
        // }

        public override CircleCastHit CircleCastHandle(CircleCast cast)
        {
            CircleCast surrCast = new CircleCast(cast.origin, cast.rad, cast.r + margin);

            var hit = base.CircleCastHandle(surrCast);

            if (!hit.isHit) return hit;

            var contactVec = hit.contactPos - hit.hitCirclePos;
            var contact = (contactVec.mag - margin) * contactVec.unit + hit.hitCirclePos;

            return new CircleCastHit(
                isHit: true, isOriginHit: hit.isOriginHit,
                originCirclePos: cast.origin,
                hitCirclePos: hit.hitCirclePos,
                contactPos: contact
            );
        }

    }


}