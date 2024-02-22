using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Linq;

public class Sample_ConnectName : MonoBehaviour
{
    public GameObject cubeItemPrefab;
    public ConnectType connectType = ConnectType.Real;
    public Button scanButton;
    public RectTransform listContent;

    private CubeScanner scanner;
    private CubeConnecter connecter;
    private Dictionary<string, GameObject> cubeItems = new Dictionary<string, GameObject>();
    private List<string> connectedAddrs = new List<string>();

    void Start()
    {
        this.scanner = new CubeScanner(this.connectType);
        this.connecter = new CubeConnecter(this.connectType);
    }

    public void StartScan()
    {
        // Clear list (except connected items)
        foreach (var addr in this.cubeItems.Keys.ToArray())
        {
            if (this.connectedAddrs.Contains(addr))
                continue;
            Destroy(this.cubeItems[addr]);
            this.cubeItems.Remove(addr);
        }

        // Start scan
        this.scanButton.interactable = false;
        this.scanButton.GetComponentInChildren<Text>().text = "Scanning...";
        this.scanner.StartScan(OnScanUpdate, OnScanEnd, 10).Forget();
    }

    void OnScanEnd()
    {
        this.scanButton.interactable = true;
        this.scanButton.GetComponentInChildren<Text>().text = "Scan";
    }

    void OnScanUpdate(BLEPeripheralInterface[] peripherals)
    {
        for (int i = 0; i < peripherals.Length; i++)
        {
            var peripheral = peripherals[i];
            if (peripheral == null) continue;

            // Add connection listener
            peripheral.AddConnectionListener("Sample_ConnectName", this.OnConnection);

            // If not paired: Create new item
            if (!this.cubeItems.ContainsKey(peripheral.device_address))
            {
                var item = Instantiate(this.cubeItemPrefab, this.listContent);
                item.GetComponent<Button>().onClick.AddListener(async () => await OnItemClick(item, peripheral));
                item.GetComponentInChildren<Text>().text = peripheral.device_name + (peripheral.isConnected? ": connected" : ": paired");
                this.cubeItems.Add(peripheral.device_address, item);
            }
        }
    }

    async UniTask OnItemClick(GameObject item, BLEPeripheralInterface peripheral)
    {
        if (peripheral.isConnected) {
#if !(UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
            this.connecter.Disconnect(peripheral);
            await UniTask.Delay(200);
            item.GetComponentInChildren<Text>().text = peripheral.device_name + ": paired";
#endif
        }
        else{
            item.GetComponentInChildren<Button>().interactable = false;
            item.GetComponentInChildren<Text>().text = peripheral.device_name + ": connecting...";
            await this.connecter.Connect(peripheral);
            item.GetComponentInChildren<Button>().interactable = true;
        }
    }

    void OnConnection(BLEPeripheralInterface peripheral)
    {
        if (peripheral.isConnected)
        {
            if (!this.connectedAddrs.Contains(peripheral.device_address))
                this.connectedAddrs.Add(peripheral.device_address);

            if (this.cubeItems.ContainsKey(peripheral.device_address))
                this.cubeItems[peripheral.device_address].GetComponentInChildren<Text>().text = peripheral.device_name + ": connected";
        }
        else
        {
            if (this.connectedAddrs.Contains(peripheral.device_address))
                this.connectedAddrs.Remove(peripheral.device_address);

            if (this.cubeItems.ContainsKey(peripheral.device_address))
                this.cubeItems[peripheral.device_address].GetComponentInChildren<Text>().text = peripheral.device_name + ": paired";
        }
    }
}
