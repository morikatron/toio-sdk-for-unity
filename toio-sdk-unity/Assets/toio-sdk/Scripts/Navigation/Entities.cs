using System;
using static toio.MathUtils.Utils;
using Vector = toio.MathUtils.Vector;


namespace toio.Navigation{

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

    public class Line
    {
        public double a;
        public double b;
        public double c;
        public Vector norm { get{ return new Vector(a, b); } }

        public Line(Vector point1, Vector point2){
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

        public Line(double a, double b, double c){
            if (a ==0 && b == 0) throw new ArgumentException("a, b cannot be both 0");
            this.a = a; this.b = b; this.c = c;
            var mag = Math.Sqrt(a*a+b*b);
            this.a /= mag; this.b /= mag; this.c /= mag;
        }

        public double DistToPoint(Vector point){
            return Math.Abs(norm * point + c);
        }

        public Vector Intersect(Line line){
            var cc = a * line.b - line.a * b;
            if (cc == 0) return Vector.Null;
            var aa = b*line.c - line.b*c;
            var bb = line.a*c - a*line.c;
            return new Vector(aa/cc, bb/cc);
        }

        public Vector FootPoint(Vector point){
            var n = norm;
            return point - (n * point + c) * n;
        }
    }


    public class Wall : Line
    {
        public double margin {get; set;}

        public Wall(Vector point1, Vector point2, double margin=0) : base(point1, point2){
            this.margin = margin;
        }
        public Wall(double a, double b, double c, double margin=0) : base(a, b, c){
            this.margin = margin;
        }

        public Line GetBorder(Vector egoPos, double egoMargin){
            return new Line(a, b, c - Math.Sign(norm*egoPos+c) * (egoMargin + margin) );
        }

    }




}