using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
        /// Table of local names and corresponding CubeSimulator objects to bind.
        /// </summary>
        [Tooltip("Table of local names and corresponding CubeSimulator objects to bind")]
        public DigitalTwinBindingTable bindingTable = new DigitalTwinBindingTable();

        /// <summary>
        /// Mat to place the digitalTwin cube.
        /// </summary>
        public Mat mat;

        /// <summary>
        /// Method to map the real cube to the digitalTwin cube.
        /// </summary>
        public Method mappingMethod = Method.AddForce;

        public bool logScanned = false;

        /// <summary>
        /// Real cube to represent.
        /// </summary>
        internal List<Cube> cubes = new List<Cube>();
        private List<string> connectingNames = new List<string>();

        void Start()
        {
            var scanner = new CubeScanner(ConnectType.Real);
            scanner.StartScan(OnScan).Forget();
        }

        public void FixedUpdate()
        {
            if (this.mat == null) return;

            foreach (var cube in this.cubes) {
                if (!this.bindingTable.ContainsKey(cube.localName)) continue;
                var sim = this.bindingTable[cube.localName];
                if (sim == null) continue;
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

        async void OnScan(BLEPeripheralInterface[] peripherals) {
            if (peripherals.Length == 0) return;
            if (logScanned) {
                Debug.Log(
                    "Scanned: " + string.Join(", ", peripherals.ToList().ConvertAll(p => p.device_name))
                );
            }

            foreach (var peri in peripherals) {
                if (!this.bindingTable.ContainsKey(peri.device_name)) continue;
                if (this.connectingNames.Contains(peri.device_name)) continue;

                // Connecting
                this.connectingNames.Add(peri.device_name);
                var cube = await new CubeConnecter(ConnectType.Real).Connect(peri);
                this.connectingNames.Remove(peri.device_name);
                this.cubes.Add(cube);
            }
        }
    }


    [Serializable]
    public class DigitalTwinBindingTable : Dictionary<string, CubeSimulator>, ISerializationCallbackReceiver
    {
        [Serializable]
        public class Pair
        {
            public string localName;
            public CubeSimulator digitalTwin;

            public Pair(string localName, CubeSimulator digitalTwin)
            {
                this.localName = localName;
                this.digitalTwin = digitalTwin;
            }
        }

        [SerializeField]
        private List<Pair> _localNameSimulatorPairList = null;

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();
            foreach (Pair pair in _localNameSimulatorPairList)
            {
                if (ContainsKey(pair.localName))
                {
                    continue;
                }
                Add(pair.localName, pair.digitalTwin);
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
    }

}
