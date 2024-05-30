using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Linq;

namespace toio.Samples.Sample_ConnectName
{
    public class Sample_ConnectName_BasicUI : MonoBehaviour
    {
        public GameObject cubeItemPrefab;
        public ConnectType connectType = ConnectType.Real;
        public Button scanButton;
        public RectTransform listContent;

        private CubeScanner scanner;
        private CubeConnecter connecter;
        private Dictionary<string, GameObject> cubeItems = new Dictionary<string, GameObject>();

        private enum BLEStatus
        {
            Disconnected,
            Connecting,
            Connected,
        }
        private Dictionary<BLEPeripheralInterface, BLEStatus> bleStatus = new Dictionary<BLEPeripheralInterface, BLEStatus>();


        void Start()
        {
            this.scanner = new CubeScanner(this.connectType);
            this.connecter = new CubeConnecter(this.connectType);
        }

        void Update ()
        {
            int idx = 0;

            var addrsToRemove = this.cubeItems.Keys.ToList();

            // Display items
            foreach (var peripheral in this.bleStatus.Keys) {
                var item = TryGetCubeItem(peripheral);
                item.transform.SetSiblingIndex(idx++);

                var status = this.bleStatus[peripheral];
                if (status == BLEStatus.Disconnected) {
                    item.GetComponentInChildren<Button>().interactable = true;
                    item.GetComponentInChildren<Text>().text = peripheral.device_name + ": Disconnected";
                }
                else if (status == BLEStatus.Connecting) {
                    item.GetComponentInChildren<Button>().interactable = false;
                    item.GetComponentInChildren<Text>().text = peripheral.device_name + ": Connecting...";
                }
                else if (status == BLEStatus.Connected) {
                    item.GetComponentInChildren<Button>().interactable = true;
                    item.GetComponentInChildren<Text>().text = peripheral.device_name + ": Connected";
                }

                addrsToRemove.Remove(peripheral.device_address);
            }

            // Remove disappeared items
            foreach (var addr in addrsToRemove)
            {
                Destroy(this.cubeItems[addr]);
                this.cubeItems.Remove(addr);
            }

        }

        GameObject TryGetCubeItem (BLEPeripheralInterface peripheral)
        {
            if (!this.cubeItems.ContainsKey(peripheral.device_address))
            {
                var item = Instantiate(this.cubeItemPrefab, this.listContent);
                item.GetComponent<Button>().onClick.AddListener(async () => await OnItemClick(item, peripheral));
                item.GetComponentInChildren<Text>().text = peripheral.device_name + (peripheral.isConnected? ": connected" : ": disconnected");
                this.cubeItems.Add(peripheral.device_address, item);
                return item;
            }
            return this.cubeItems[peripheral.device_address];
        }

        public void StartScan()
        {
            // Clear list (except connected items)
            foreach (var addr in this.cubeItems.Keys.ToArray())
            {
                Destroy(this.cubeItems[addr]);
                this.cubeItems.Remove(addr);
            }

            // Start scan
            this.scanButton.interactable = false;
            this.scanButton.GetComponentInChildren<Text>().text = "Scanning...";
            this.scanner.StartScan(OnScanUpdate, OnScanEnd, 20).Forget();
        }

        void OnScanEnd()
        {
            this.scanButton.interactable = true;
            this.scanButton.GetComponentInChildren<Text>().text = "Scan";
        }

        void OnScanUpdate(BLEPeripheralInterface[] peripherals)
        {
            foreach (var peri in peripherals) {
                if (peri == null) continue;
                if (this.bleStatus.ContainsKey(peri) && this.bleStatus[peri] != BLEStatus.Connecting)
                    this.bleStatus[peri] = BLEStatus.Disconnected;
                if (!this.bleStatus.ContainsKey(peri))
                    this.bleStatus.Add(peri, BLEStatus.Disconnected);
            }
            foreach (var peri in this.bleStatus.Keys.ToArray()) {
                if (!peripherals.Contains(peri) && this.bleStatus[peri] == BLEStatus.Disconnected) {
                    this.bleStatus.Remove(peri);
                }
            }

            // Add connection listener
            foreach (var peripheral in peripherals)
            {
                if (peripheral == null) continue;
                peripheral.AddConnectionListener("Sample_ConnectName", this.OnConnection);
            }
        }

        async UniTask OnItemClick(GameObject item, BLEPeripheralInterface peripheral)
        {
            if (peripheral.isConnected) {
    #if !(UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
                // NOTE: On Windows, disconnecting a device causes crash
                this.connecter.Disconnect(peripheral);
                this.bleStatus[peripheral] = BLEStatus.Disconnected;
    #endif
            }
            else{
                this.bleStatus[peripheral] = BLEStatus.Connecting;
                try {
                    var cube = await this.connecter.Connect(peripheral);
                    if (cube == null)
                        throw new System.Exception("Connection Timeout.");
                }
                catch (System.Exception e) {
    #if UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    // Connectition fail twice causes BLE host device lost issue on iOS/macOS
                    this.bleStatus.Remove(peripheral);
    #else
                    this.bleStatus[peripheral] = BLEStatus.Disconnected;
    #endif
                    Debug.LogError(e);
                }
            }
        }

        void OnConnection(BLEPeripheralInterface peripheral)
        {
            this.bleStatus[peripheral] = peripheral.isConnected ? BLEStatus.Connected : BLEStatus.Disconnected;
            if (this.cubeItems.ContainsKey(peripheral.device_address))
                this.cubeItems[peripheral.device_address].GetComponentInChildren<Text>().text = peripheral.device_name + (peripheral.isConnected? ": connected" : ": disconnected");
        }
    }
}
