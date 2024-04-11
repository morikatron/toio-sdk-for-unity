using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;
using Cysharp.Threading.Tasks;
using System.Linq;

public class Sample_DigitalTwin : MonoBehaviour
{
    public ConnectType connectType = ConnectType.Real;

    public List<string> localNamesToConnect;


    private List<Cube> cubes = new List<Cube>();
    private List<string> connectingNames = new List<string>();
    private DigitalTwinBinder binder;

    void Start()
    {
        this.binder = GetComponent<DigitalTwinBinder>();

        if (this.connectType != ConnectType.Real){
            this.binder.enabled = false;
        }

        new CubeScanner(this.connectType).StartScan(OnScan).Forget();
    }

    async void OnScan(BLEPeripheralInterface[] peripherals) {
        if (peripherals.Length == 0) return;
        Debug.Log("Scanned: " + string.Join(", ", peripherals.ToList().ConvertAll(p => p.device_name)));

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