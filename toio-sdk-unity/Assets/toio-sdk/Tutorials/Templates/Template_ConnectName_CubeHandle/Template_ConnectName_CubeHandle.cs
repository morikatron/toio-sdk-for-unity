using System.Collections;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using toio.Simulator;


namespace toio.tutorial.Template_ConnectName_CubeHandle
{
    public class Template_ConnectName_CubeHandle : MonoBehaviour
    {
        public ConnectType connectType = ConnectType.Auto;
        public GameObject cubeItemPrefab;
        public RectTransform panelConnect;
        public RectTransform listContent;
        public Button buttonConnect;
        public Text textStatus;
        public Stage stage;

        CubeScanner scanner;
        CubeConnecter connecter;
        Cube cube;
        CubeHandle handle;
        float elapsedTime = 0;
        Dictionary<string, GameObject> cubeItems = new Dictionary<string, GameObject>();

        public bool connected => cube != null && cube.isConnected;

        void Start()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            this.scanner = new CubeScanner(this.connectType);
            this.connecter = new CubeConnecter(this.connectType);

            // Hide cubes in the simulator
            if (this.connectType == ConnectType.Real ||
                this.connectType == ConnectType.Auto && CubeScanner.actualTypeOfAuto == ConnectType.Real)
            {
                foreach (var cube in GameObject.FindGameObjectsWithTag("t4u_Cube"))
                    cube.SetActive(false);
            }
        }

        void Update()
        {
            this.elapsedTime += Time.deltaTime;
            if (this.handle != null && this.elapsedTime > 0.05f)
            {
                this.handle.Update();
                if (stage.targetPoleActive)
                    handle.Move2Target(stage.targetPoleCoord).Exec();
                else
                    handle.MoveRaw(0, 0, 0);

                this.elapsedTime = 0;
            }
        }

        public async void OnBtnConnect()
        {
            if (!connected) {
                panelConnect.gameObject.SetActive(true);
                this.scanner.StartScan(OnScan).Forget();
            } else {
                this.connecter.Disconnect(cube);
                this.cube = null;
                this.handle = null;
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
                peri.AddConnectionListener("Template_ConnectName_CubeHandle", this.OnConnection);

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
                // Connect to the selected cube
                this.cube = await this.connecter.Connect(peripheral);
                // Create a CubeHandle
                if (this.cube != null)
                    this.handle = new CubeHandle(this.cube);

                // Update GUI
                this.OnBtnCancel();

                if (this.cube == null)
                    throw new System.Exception("Connection Timeout.");

                this.textStatus.text = this.cube.localName +  " connected";
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

    }
}
