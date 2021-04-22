using UnityEngine;
using toio;
using toio.Navigation;
using toio.MathUtils;
using static toio.MathUtils.Utils;

public class Sample_MultiMat : MonoBehaviour
{
    CubeManager cm;
    async void Start()
    {
        cm = new CubeManager();
        await cm.MultiConnect(16);

        cm.handles.Clear();
        cm.navigators.Clear();
        foreach (var cube in cm.cubes)
        {
            var handle = new HandleMats(cube);
            cm.handles.Add(handle);
            var navi = new CubeNavigator(handle);
            navi.usePred = true;
            navi.mode = Navigator.Mode.BOIDS_AVOID;
            cm.navigators.Add(navi);

            handle.borderRect = new RectInt(95, 95, 720, 720);
            navi.ClearWall();
            navi.AddBorder(30, x1:0, x2:910, y1:0, y2:910);
        }
    }

    void Update()
    {
        var tar = Vector.fromRadMag(Time.time/2, 220) + new Vector(455, 455);
        if (cm.synced){

            for (int i=0; i<cm.navigators.Count; i++)
            {
                var navi = cm.navigators[i];
                var mv = navi.Navi2Target(tar, maxSpd:60).Exec();
            }
        }
    }

    public class HandleMats : CubeHandle
    {
        public HandleMats(Cube _cube) : base(_cube)
        {}

        public int matX = 0;
        public int matY = 0;
        protected float lastX=250;
        protected float lastY=250;
        public override void Update()
        {
            var frm = Time.frameCount;
            if (frm == updateLastFrm) return;
            updateLastFrm = frm;

            var rawX = cube.x;
            var rawY = cube.y;

            // update matX, matY
            if (lastX > 455 - 50 && rawX < 45 + 50)
                matX += 1;
            else if (rawX > 455 - 50 && lastX < 45 + 50)
                matX -= 1;

            if (lastY > 455 - 50 && rawY < 45 + 50)
                matY += 1;
            else if (rawY > 455 - 50 && lastY < 45 + 50)
                matY -= 1;

            if (matX < 0) matX = 0;
            if (matY < 0) matY = 0;

            lastX = rawX; lastY = rawY;

            // update x, y
            x = rawX + matX*411;
            y = rawY + matY*411;
            deg = Deg(cube.angle);
            UpdateProperty();

            Predict();
        }
    }
}
