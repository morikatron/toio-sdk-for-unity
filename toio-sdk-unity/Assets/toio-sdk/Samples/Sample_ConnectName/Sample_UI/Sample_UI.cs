using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


namespace toio.Samples.Sample_ConnectName
{
    public class Sample_UI : MonoBehaviour
    {
        public GameObject cubeItemPrefab;
        public RectTransform panelConnect;
        public RectTransform listContent;
        public Button buttonConnect;
        public Text textStatus;

        CubeScanner scanner;
        CubeConnecter connecter;
        Cube cube;
        Dictionary<string, GameObject> cubeItems = new Dictionary<string, GameObject>();

        public bool connected => cube != null && cube.isConnected;

        void Start()
        {
            this.scanner = new CubeScanner();
            this.connecter = new CubeConnecter();
        }

        public async void OnBtnConnect()
        {
            if (!connected) {
                panelConnect.gameObject.SetActive(true);
                this.scanner.StartScan(OnScan).Forget();
            } else {
                this.connecter.Disconnect(cube);
                this.cube = null;
                await UniTask.Delay(100);
                this.buttonConnect.GetComponentInChildren<Text>().text = "Connect";
            }
        }
        public void OnBtnCancel () {
            this.scanner.StopScan();
            panelConnect.gameObject.SetActive(false);
            // Clear list
            foreach (var addr in this.cubeItems.Keys.ToArray())
            {
                Destroy(this.cubeItems[addr]);
                this.cubeItems.Remove(addr);
            }
        }
        void OnScan(BLEPeripheralInterface[] peris) {
            if (peris.Length == 0) return;
            foreach (var peri in peris) {
                if (peri == null) continue;
                if (this.cubeItems.ContainsKey(peri.device_address)) continue;
                peri.AddConnectionListener("Sample_ConnectName", this.OnConnection);

                // Create list item
                var item = Instantiate(this.cubeItemPrefab, this.listContent);
                item.GetComponent<Button>().onClick.AddListener(async () => await OnItemClick(peri));
                item.GetComponentInChildren<Text>().text = peri.device_name;
                this.cubeItems.Add(peri.device_address, item);
            }
        }

        async UniTask OnItemClick(BLEPeripheralInterface peripheral)
        {
            try {
                this.cube = await this.connecter.Connect(peripheral);
                this.OnBtnCancel();
                this.textStatus.text = this.cube.localName +  " connected";

                if (this.cube == null)
                    throw new System.Exception("Connection Timeout.");
                this.buttonConnect.GetComponentInChildren<Text>().text = "Disconnect";
            }
            catch (System.Exception e) {
                Debug.LogError(e);
            }
        }

        void OnConnection(BLEPeripheralInterface peri) {
            if (!peri.isConnected) {
                if (!this.textStatus.IsDestroyed())
                    this.textStatus.text = "";
            }
        }


        // 持続時間(durationMs):0にする事で時間無制限となり、一度呼び出すだけで動作し続ける事が出来る。
        // 命令の優先度(order):Cube.ORDER_TYPE.Strongにすることで、一度きりの命令を安全に送信。
        // 【詳細】:
        // 命令の優先度をStrongにすると、内部で命令を命令キューに追加して命令可能フレーム時に順次命令を送る仕組みになっている。
        // 通常は命令前に cubeManager.IsControllable(cube) を呼ぶことで命令可能フレームの確認を行うが、
        // 今回は命令の優先度をStrongにしているため、cubeManager.IsControllable(cube) を呼ばずにそのまま命令キューに追加する。
        // ※ちなみにcubeManager.IsControllable(cube) を呼んで事前に命令可能フレームの確認を行った場合は、パケロスならぬ命令ロスとなる。
        public void Forward() { cube?.Move(60, 60, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
        public void Backward() { cube?.Move(-40, -40, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
        public void TurnRight() { cube?.Move(60, 30, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
        public void TurnLeft() { cube?.Move(30, 60, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
        public void Stop() { cube?.Move(0, 0, durationMs:0, order:Cube.ORDER_TYPE.Strong); }
        public void PlayPresetSound1() { cube?.PlayPresetSound(1); }
        public void PlayPresetSound2() { cube?.PlayPresetSound(2); }
        public void LedOn()
        {
            List<Cube.LightOperation> scenario = new List<Cube.LightOperation>();
            float rad = (Mathf.Deg2Rad * (360.0f / 29.0f));
            for (int i = 0; i < 29; i++)
            {
                byte r = (byte)Mathf.Clamp((128 + (Mathf.Cos(rad * i) * 128)), 0, 255);
                byte g = (byte)Mathf.Clamp((128 + (Mathf.Sin(rad * i) * 128)), 0, 255);
                byte b = (byte)Mathf.Clamp(((Mathf.Abs(Mathf.Cos(rad * i) * 255))), 0, 255);
                scenario.Add(new Cube.LightOperation(100, r, g, b));
            }
            cube?.TurnOnLightWithScenario(0, scenario.ToArray());
        }
        public void LedOff() { cube?.TurnLedOff(); }
    }
}