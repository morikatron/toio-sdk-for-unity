using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;


namespace toio.Samples.Sample_DigitalTwin
{
    public class DigitalTwinConnecter : MonoBehaviour
    {
        public ConnectType connectType = ConnectType.Real;

        public List<string> localNamesToConnect;
        public bool logScanned = true;


        public List<Cube> cubes {get; private set;} = new List<Cube>();
        private List<string> connectingNames = new List<string>();
        private DigitalTwinBinder binder;

        void Start()
        {
            this.binder = GetComponent<DigitalTwinBinder>();

            if (this.connectType != ConnectType.Real){
                this.binder.enabled = false;
            }

            var scanner = new CubeScanner(this.connectType);
            scanner.StartScan(OnScan).Forget();

            Debug.Log("Scanning " + (scanner.actualType == ConnectType.Real ? "real" : "simulator") + " cubes.");
        }

        async void OnScan(BLEPeripheralInterface[] peripherals) {
            if (peripherals.Length == 0) return;
            if (logScanned) {
                Debug.Log(
                    "Scanned: " + string.Join(", ", peripherals.ToList().ConvertAll(p => p.device_name))
                );
            }

            foreach (var peri in peripherals) {
                if (!this.localNamesToConnect.Contains(peri.device_name)) continue;
                if (this.connectingNames.Contains(peri.device_name)) continue;

                // Connecting
                this.connectingNames.Add(peri.device_name);
                var cube = await new CubeConnecter(this.connectType).Connect(peri);
                this.connectingNames.Remove(peri.device_name);
                this.cubes.Add(cube);

                // Bind real cubes
                if (this.connectType == ConnectType.Real){
                    this.binder.cubes = this.cubes.ToArray();
                }
            }
        }
    }
}