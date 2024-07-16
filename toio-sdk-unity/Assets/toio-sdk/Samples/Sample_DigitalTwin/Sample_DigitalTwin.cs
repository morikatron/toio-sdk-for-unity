using System.Collections.Generic;
using UnityEngine;


namespace toio.Samples.Sample_DigitalTwin
{
    public class Sample_DigitalTwin : MonoBehaviour
    {
        public DigitalTwinBinder binder;

        Dictionary<string, CubeHandle> handles = new Dictionary<string, CubeHandle>();
        float elapsedTime = 0;

        void Update()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime < 0.05f) return;
            elapsedTime = 0;

            foreach (var cube in this.binder.cubes)
            {
                // Create CubeHandle for new cube
                if (!this.handles.ContainsKey(cube.addr))
                    this.handles.Add(cube.addr, new CubeHandle(cube));

                // Control cube with CubeHandle
                var handle = this.handles[cube.addr];
                handle.Update();
                handle.Move2Target(255, 255).Exec();
            }
        }
    }
}