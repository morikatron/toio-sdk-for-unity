using UnityEngine;
using toio.Simulator;


namespace toio.Samples.Sample_DigitalTwin
{
    public class DigitalTwinBinder: MonoBehaviour
    {
        public enum Method {
            Direct,
            AddForce,
        }


        /// <summary>
        /// The target simulator cube to represent the real cube.
        /// </summary>
        [Tooltip("List of CubeSimulator objects to bind to the real Cubes")]
        public CubeSimulator[] simulators;

        /// <summary>
        /// Mat to place the digitalTwin cube.
        /// </summary>
        public Mat mat;

        /// <summary>
        /// Method to map the real cube to the digitalTwin cube.
        /// </summary>
        public Method mappingMethod = Method.AddForce;

        /// <summary>
        /// Real cube to represent.
        /// </summary>
        internal Cube[] cubes;


        public void FixedUpdate()
        {
            if (this.mat == null) return;
            if (this.cubes == null) return;

            for (int i = 0; i < this.cubes.Length; i++) {
                if (this.simulators.Length <= i) break;
                var sim = this.simulators[i];
                var cube = this.cubes[i];
                if (!cube.isConnected || !this.mat.IsUnityCoordInside(cube.x, cube.y)) continue;
                // Disable simulator interaction
                sim.GetComponent<CubeInteraction>().enabled = false;

                var rb = sim.GetComponent<Rigidbody>();
                rb.useGravity = false;

                var pos = this.mat.MatCoord2UnityCoord(cube.x, cube.y);
                var deg = this.mat.MatDeg2UnityDeg(cube.angle);

                if(cube.isGrounded){
                    pos.y = 0f;
                    if (this.mappingMethod == Method.Direct) {
                        rb.MovePosition(pos);
                        rb.MoveRotation(Quaternion.Euler(0, deg, 0));
                    }
                    else if (this.mappingMethod == Method.AddForce) {
                        var dpos = pos - sim.transform.position;
                        var ddeg = (deg - sim.transform.eulerAngles.y + 540) % 360 - 180;

                        rb.AddForce(dpos / Time.fixedDeltaTime * 4e-3f, ForceMode.Impulse);
                        rb.AddTorque(0, ddeg / Time.fixedDeltaTime * 2e-8f, 0, ForceMode.Impulse);
                    }
                }
                else {
                    pos.y = 0.05f;
                    rb.MovePosition(pos);
                }
            }
        }
    }
}