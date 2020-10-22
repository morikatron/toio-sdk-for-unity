using static System.Math;

namespace toio.MathUtils{

    public struct Vector{
        public double x;
        public double y;
        public bool isNull;

        public Vector(double x, double y){
            this.x = x; this.y = y; isNull = false;
        }
        public Vector(Vector v)
        {
            x = v.x; y = v.y; isNull = v.isNull;
        }
        private Vector(bool isNull)
        {
            x = 0; y = 0; this.isNull = isNull;
        }

        // Static
        public static Vector zero { get { return new Vector(0, 0); } }
        public static Vector Null { get { return new Vector(true); } }
        public static Vector fromRadMag(double rad, double mag)
        {
            return new Vector(mag * Cos(rad), mag * Sin(rad));
        }

        // Operators
        public static Vector operator +(Vector v) => v;
        public static Vector operator -(Vector v) => new Vector(-v.x, -v.y);

        public static Vector operator +(Vector v, Vector u) => new Vector(v.x+u.x, v.y+u.y);
        public static Vector operator -(Vector v, Vector u) => new Vector(v.x-u.x, v.y-u.y);
        public static double operator *(Vector v, Vector u) => v.x*u.x + v.y*u.y;


        public static Vector operator +(double c, Vector v) => new Vector(v.x+c, v.y+c);
        public static Vector operator +(Vector v, double c) => new Vector(v.x+c, v.y+c);
        public static Vector operator -(double c, Vector v) => new Vector(c-v.x, c-v.y);
        public static Vector operator -(Vector v, double c) => new Vector(v.x-c, v.y-c);
        public static Vector operator *(Vector v, double c) => new Vector(v.x*c, v.y*c);
        public static Vector operator *(double c, Vector v) => new Vector(v.x*c, v.y*c);
        public static Vector operator /(Vector v, double c) => new Vector(v.x/c, v.y/c);

        public static bool operator ==(Vector v, Vector u) =>
            v.isNull==true && u.isNull==true || v.isNull==u.isNull && v.x==u.x && v.y==u.y;
        public static bool operator !=(Vector v, Vector u) => !(v==u);
        public override bool Equals(object obj){
            if (obj == null) return false;
            var o = (Vector) obj;
            return o == this;
        }
        public override int GetHashCode(){ return (int)x;}

        public override string ToString(){if (!isNull) return string.Format("({0:#.0}, {1:#.0})", x, y); else return "NULL";}


        // Properties
        public double mag{get{return Sqrt(x*x+y*y);}}
        public Vector copy{get{return new Vector(x, y);}}
        public double rad{get{return Utils.Rad(Atan2(y, x));}}
        public double deg{get{return Utils.Rad2Deg(rad);}}
        public Vector unit{get{
            if (x==0 && y==0) return new Vector(0, 0);
            else return this/mag;
            }}

        public double radTo(Vector o){
           return Utils.Rad(o.rad-rad);
        }
        public double degTo(Vector o){
            return Utils.Deg(o.deg-deg);
        }

        public Vector rotateByRad(double r){
            return new Vector(Cos(r)*x - Sin(r)*y, Sin(r)*x + Cos(r)*y);
        }
        public Vector rotateByDeg(double d){
            return rotateByRad(Utils.Deg2Rad(d));
        }

        public double distTo(Vector o){
            var dx = o.x-x; var dy = o.y-y;
            return Sqrt(dx*dx+dy*dy);
        }
        public Vector clip(double max){
            if (mag > max){
                if (x==0 && y==0) return new Vector(0,0);
                else{
                    var m = Sqrt(x*x+y*y);
                    return new Vector(x*max/m, y*max/m);
                }
            }
            else return new Vector(x, y);
        }
        public Vector withMin(double min){
            if (mag == 0 || mag >= min) return this.copy;
            return unit*min;
        }
        public Vector scaleTo(double mag){
            if (mag == 0) return Vector.zero;
            else return unit*mag;
        }
    }

    public struct Utils{
        public static double clip(double x, double min, double max){
            if (x < min) return min;
            if (x > max) return max;
            return x;
        }
        public static int clip(int x, int min, int max){
            if (x < min) return min;
            if (x > max) return max;
            return x;
        }


        public static double Rad(double r){
            return (r + PI*3) % (PI*2) - PI;
        }
        public static double Deg(double d){
            return (d + 540) % 360 - 180;
        }
        public static double Rad2Deg(double r){
            return Deg(r * 180 / PI);
        }
        public static double Deg2Rad(double d){
            return Rad(d * PI / 180);
        }
        public static double AbsRad(double r){
            return Abs(Rad(r));
        }
        public static double AbsDeg(double d){
            return Abs(Deg(d));
        }

        public static int ToInt(double x){
            return (int)(Round(x));
        }
        public static int ToInt(float x){
            return (int)(Round(x));
        }
    }
}