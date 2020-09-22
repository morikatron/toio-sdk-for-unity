using System.Collections.Generic;
using System;
using static toio.MathUtils.Utils;
using Vector = toio.MathUtils.Vector;


namespace toio.Navigation
{

    public class Boids
    {
        public double fov = Deg2Rad(120);
        public double range = 150;
        public double margin = 25;

        public double p_weight_attraction = 50;
        public double p_weight_separation = 1;
        public double p_max_separation = 100;
        public double p_weight_cohesion = 0.3;
        public double p_max_cohesion = 50;
        public double p_weight_alignment = 0.3;
        public double p_max_alignment = 30;
        public double p_max_all = 100;

        Entity ego;

        public Boids(Entity ego)
        {
            this.ego = ego;
        }

        Vector Attract(Vector tarPos)
        {
            var dPos = tarPos - ego.pos;
            return dPos.unit;
        }

        Vector Separate(List<Navigator> others)
        {
            Vector f = Vector.zero;
            foreach (var o in others)
            {
                var dPos = ego.pos - o.entity.pos;
                var rawDist = dPos.mag;
                var dist = Math.Max(1, rawDist - margin - o.boids.margin);

                f += dPos.unit * (range / dist - 1);
            }
            return f;
        }

        Vector Cohesion(List<Navigator> others)
        {
            if (others.Count == 0)
                return Vector.zero;
            var dPos = Vector.zero;
            foreach (var o in others)
            {
                dPos += (o.entity.pos - ego.pos) / others.Count;
            }
            return dPos; //.unit;
        }

        Vector Alignment(List<Navigator> others)
        {
            if (others.Count == 0)
                return Vector.zero;
            var v = Vector.zero;
            foreach (var o in others)
            {
                v += o.entity.v / others.Count;
            }
            return v;
        }

        public Vector Run(List<Navigator> others, Vector tarPos)
        {
            List<Navigator> othersSeen = new List<Navigator>();
            foreach (var o in others)
            {
                if (ego.pos.distTo(o.entity.pos) - margin - o.boids.margin < range &&
                    ego.Rad2Pos(o.entity.pos) < fov)
                    othersSeen.Add(o);
            }

            var att = Attract(tarPos) * p_weight_attraction;

            var sep = Separate(othersSeen) * p_weight_separation;
            sep = sep.clip(p_max_separation);

            var coh = Cohesion(othersSeen) * p_weight_cohesion;
            coh = coh.clip(p_max_cohesion);

            var ali = Alignment(othersSeen) * p_weight_alignment;
            ali = ali.clip(p_max_alignment);

            var f = att + sep + coh + ali;
            f = f.clip(p_max_all);

            return f;
        }

        public Vector Run(List<Navigator> others)
        {
            List<Navigator> othersSeen = new List<Navigator>();
            foreach (var o in others)
            {
                if (ego.pos.distTo(o.entity.pos) - margin - o.boids.margin < range &&
                    ego.Rad2Pos(o.entity.pos) < fov)
                    othersSeen.Add(o);
            }

            var sep = Separate(othersSeen) * p_weight_separation;
            sep = sep.clip(p_max_separation);

            var coh = Cohesion(othersSeen) * p_weight_cohesion;
            coh = coh.clip(p_max_cohesion);

            var ali = Alignment(othersSeen) * p_weight_alignment;
            ali = ali.clip(p_max_alignment);

            var f = sep + coh + ali;
            f = f.clip(p_max_all);

            return f;
        }
    }


}