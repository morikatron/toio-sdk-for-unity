using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using toio.Simulator;

namespace toio
{
    public enum ConnectType
    {
        Auto, // ビルド対象に応じて内部実装が自動的に変わる
        Simulator, // ビルド対象に関わらずシミュレータのキューブで動作する
        Real // ビルド対象に関わらずリアル(現実)のキューブで動作する
    }

    public interface CubeScannerInterface
    {
        bool isScanning { get; }
        UniTask<BLEPeripheralInterface> NearestScan();
        UniTask<BLEPeripheralInterface[]> NearScan(int satisfiedNum, float waitSeconds = 3.0f);
        void NearScanAsync(int satisfiedNum, MonoBehaviour coroutineObject, Action<BLEPeripheralInterface> callback, bool autoRunning = true);
    }

    public class CubeScanner : CubeScannerInterface
    {
        /// <summary>
        /// Actual type (real or sim) CubeScanner(ConnectType.Auto) will be, depending on current environment.
        /// </summary>
        public static ConnectType actualTypeOfAuto { get {
#if (UNITY_EDITOR || UNITY_STANDALONE)
            return ConnectType.Simulator;
#elif (UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL)
            return ConnectType.Real;
#endif
        }}

        private CubeScannerInterface impl;
        public CubeScanner(ConnectType type = ConnectType.Auto)
        {
            if (ConnectType.Auto == type)
            {
#if (UNITY_EDITOR || UNITY_STANDALONE)
                this.impl = new SimImpl();
#elif (UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL)
                this.impl = new RealImpl();
#endif
            }
            else if (ConnectType.Simulator == type)
            {
                this.impl = new SimImpl();
            }
            else if (ConnectType.Real == type)
            {
                this.impl = new RealImpl();
            }
        }
        public bool isScanning { get { return this.impl.isScanning; } }
        public async UniTask<BLEPeripheralInterface> NearestScan() { return await this.impl.NearestScan(); }
        public async UniTask<BLEPeripheralInterface[]> NearScan(int satisfiedNum, float waitSeconds) { return await this.impl.NearScan(satisfiedNum, waitSeconds); }
        public void NearScanAsync(int satisfiedNum, MonoBehaviour coroutineObject, Action<BLEPeripheralInterface> callback, bool autoRunning) { this.impl.NearScanAsync(satisfiedNum, coroutineObject, callback, autoRunning); }

        /// <summary>
        /// Impl for UnitySimulator.
        /// </summary>
        public class SimImpl : CubeScannerInterface
        {
            public bool isScanning { get; private set; }
            private Dictionary<string, BLEPeripheralInterface> peripheralDatabase = new Dictionary<string, BLEPeripheralInterface>();
            private Dictionary<string, BLEPeripheralInterface> connectedPeripheralTable = new Dictionary<string, BLEPeripheralInterface>();
            // connection async
            private int satisfiedNumForAsync;
            private MonoBehaviour coroutineObject;
            private Action<BLEPeripheralInterface> callback;
            private bool autoRunning;

            public SimImpl()
            {
                this.isScanning = false;
            }

            public async UniTask<BLEPeripheralInterface> NearestScan()
            {
                List<BLEPeripheralInterface> peripheralList = new List<BLEPeripheralInterface>();
                List<GameObject> foundObjs = new List<GameObject>();

                Action<GameObject> foundCallback = obj =>
                {
                    var peripheral = new UnityPeripheral(obj) as BLEPeripheralInterface;
                    if (!this.connectedPeripheralTable.ContainsKey(peripheral.device_address))
                    {
                        if (this.peripheralDatabase.ContainsKey(peripheral.device_address))
                        {
                            peripheral = this.peripheralDatabase[peripheral.device_address];
                        }
                        else
                        {
                            this.peripheralDatabase.Add(peripheral.device_address, peripheral);
                            peripheral.AddConnectionListener("CubeScanner.SimImpl", this.OnConnectionEvent);
                        }
                        peripheralList.Add(peripheral);
                    }
                };

                isScanning = true;

                // Scanning Loop
                while (true)
                {
                    // Search for new cube object
                    var objs = Array.ConvertAll<CubeSimulator, GameObject>(GameObject.FindObjectsOfType<CubeSimulator>(), sim => sim.gameObject);
                    foreach (var obj in objs)
                    {
                        if (!obj.GetComponent<CubeSimulator>().isRunning) continue;
                        if (!foundObjs.Contains(obj))
                        {
                            foundObjs.Add(obj);
                            foundCallback.Invoke(obj);
                        }
                    }

                    if (0 < peripheralList.Count) break;

                    // Searching Period
                    await UniTask.Delay(200);
                }

                peripheralList.Sort((a, b) => b.rssi > a.rssi ? 1 : -1);
                isScanning = false;
                return peripheralList[0];
            }
            public async UniTask<BLEPeripheralInterface[]> NearScan(int satisfiedNum, float waitSeconds)
            {
                var start_time = Time.time;
                List<BLEPeripheralInterface> peripheralList = new List<BLEPeripheralInterface>();
                List<GameObject> foundObjs = new List<GameObject>();

                Action<GameObject> foundCallback = obj =>
                {
                    var peripheral = new UnityPeripheral(obj) as BLEPeripheralInterface;
                    if (this.isScanning && peripheralList.Count < satisfiedNum &&
                        !this.connectedPeripheralTable.ContainsKey(peripheral.device_address))
                    {
                        if (this.peripheralDatabase.ContainsKey(peripheral.device_address))
                        {
                            peripheral = this.peripheralDatabase[peripheral.device_address];
                        }
                        else
                        {
                            this.peripheralDatabase.Add(peripheral.device_address, peripheral);
                            peripheral.AddConnectionListener("CubeScanner.SimImpl", this.OnConnectionEvent);
                        }
                        peripheralList.Add(peripheral);
                    }
                };

                isScanning = true;

                // Scanning Loop
                while (true)
                {
                    // Search for new cube object
                    var objs = Array.ConvertAll<CubeSimulator, GameObject>(GameObject.FindObjectsOfType<CubeSimulator>(), sim => sim.gameObject);
                    foreach (var obj in objs)
                    {
                        if (!obj.GetComponent<CubeSimulator>().isRunning) continue;
                        if (!foundObjs.Contains(obj))
                        {
                            foundObjs.Add(obj);
                            foundCallback.Invoke(obj);
                        }
                    }

                    // 必要数に達したらスキャン終了
                    if (satisfiedNum <= peripheralList.Count) break;

                    // Searching Period
                    await UniTask.Delay(200);

                    // 待機時間を超えた場合は一旦関数を終了する
                    var elapsed = Time.time - start_time;
                    if (waitSeconds <= elapsed) break;
                }

                peripheralList.Sort((a, b) => b.rssi > a.rssi ? 1 : -1);
                var nearPeripherals = peripheralList.GetRange(0, Mathf.Min(satisfiedNum, peripheralList.Count)).ToArray();
                this.isScanning = false;
                return nearPeripherals;
            }
            public void NearScanAsync(int satisfiedNum, MonoBehaviour coroutineObject, Action<BLEPeripheralInterface> callback, bool autoRunning)
            {
                this.satisfiedNumForAsync = satisfiedNum;
                this.coroutineObject = coroutineObject;
                this.callback = callback;
                this.autoRunning = autoRunning;
                this.coroutineObject.StartCoroutine(this.ScanCoroutine(satisfiedNum, callback));
            }

            // --- private methods ---
            private IEnumerator ScanCoroutine(int satisfiedNum, Action<BLEPeripheralInterface> callback)
            {
                List<GameObject> foundObjs = new List<GameObject>();

                Action<GameObject> foundCallback = obj =>
                {
                    var peripheral = new UnityPeripheral(obj) as BLEPeripheralInterface;
                    if (this.isScanning && foundObjs.Count < satisfiedNum &&
                        !this.connectedPeripheralTable.ContainsKey(peripheral.device_address))
                    {
                        if (this.peripheralDatabase.ContainsKey(peripheral.device_address))
                        {
                            peripheral = this.peripheralDatabase[peripheral.device_address];
                        }
                        else
                        {
                            this.peripheralDatabase.Add(peripheral.device_address, peripheral);
                            peripheral.AddConnectionListener("CubeScanner.SimImpl", this.OnConnectionEvent);
                        }
                        callback?.Invoke(peripheral);
                    }
                };

                isScanning = true;

                // Scanning Loop
                while (true)
                {
                    // Search for new cube object
                    var objs = Array.ConvertAll<CubeSimulator, GameObject>(GameObject.FindObjectsOfType<CubeSimulator>(), sim => sim.gameObject);
                    foreach (var obj in objs)
                    {
                        if (!obj.GetComponent<CubeSimulator>().isRunning) continue;
                        if (!foundObjs.Contains(obj))
                        {
                            foundObjs.Add(obj);
                            foundCallback.Invoke(obj);
                        }
                    }

                    // 必要数に達したらスキャン終了
                    if (satisfiedNum <= foundObjs.Count) break;

                    // Searching Period
                    yield return new WaitForSeconds(0.2f);
                }

                isScanning = false;
            }

            private void OnConnectionEvent(BLEPeripheralInterface peripheral)
            {
                if (peripheral.isConnected)
                {
                    if (!this.connectedPeripheralTable.ContainsKey(peripheral.device_address))
                    {
                        this.connectedPeripheralTable.Add(peripheral.device_address, peripheral);
                    }
                }
                else
                {
                    if (this.connectedPeripheralTable.ContainsKey(peripheral.device_address))
                    {
                        this.connectedPeripheralTable.Remove(peripheral.device_address);
                    }
                    if (!this.isScanning && this.autoRunning)
                    {
                        this.NearScanAsync(this.satisfiedNumForAsync, this.coroutineObject, this.callback, this.autoRunning);
                    }
                }
            }
        }

        /// <summary>
        /// Impl for RealCube.
        /// </summary>
        public class RealImpl : CubeScannerInterface
        {
            public bool isScanning { get; private set; }
            // ble
            private BLEDeviceInterface device;
            private Dictionary<string, BLEPeripheralInterface> peripheralDatabase = new Dictionary<string, BLEPeripheralInterface>();
            private Dictionary<string, BLEPeripheralInterface> connectedPeripheralTable = new Dictionary<string, BLEPeripheralInterface>();
            // connection async
            private int satisfiedNumForAsync;
            private MonoBehaviour coroutineObject;
            private Action<BLEPeripheralInterface> callback;
            private bool autoRunning;

            public RealImpl()
            {
                this.isScanning = false;
#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID)
                if(!BLEService.Instance.hasImplement) { BLEService.Instance.SetImplement(new BLEMobileService()); }
#elif UNITY_WEBGL
                if(!BLEService.Instance.hasImplement) { BLEService.Instance.SetImplement(new BLEWebService()); }
#endif
            }

            public async UniTask<BLEPeripheralInterface> NearestScan()
            {
                Dictionary<string, BLEPeripheralInterface> peripheralTable = new Dictionary<string, BLEPeripheralInterface>();
                List<BLEPeripheralInterface> peripheralList = new List<BLEPeripheralInterface>();

                bool errorflg = false;
                Action foundCallback = (() =>
                {
                    string[] uuids = { CubeReal.SERVICE_ID };
                    this.device.Scan(uuids, true, (peripheral) =>
                    {
                        if (null == peripheral)
                        {
                            errorflg = true;
                            return;
                        }
                        if (!this.connectedPeripheralTable.ContainsKey(peripheral.device_address) && !peripheralTable.ContainsKey(peripheral.device_address))
                        {
                            if (this.peripheralDatabase.ContainsKey(peripheral.device_address))
                            {
                                peripheral = this.peripheralDatabase[peripheral.device_address];
                            }
                            else
                            {
                                this.peripheralDatabase.Add(peripheral.device_address, peripheral);
                                peripheral.AddConnectionListener("CubeScanner.RealImpl", this.OnConnectionEvent);
                            }
                            peripheralList.Add(peripheral);
                            peripheralTable.Add(peripheral.device_address, peripheral);
                        }
                    });
                });
                this.StartScanning(foundCallback);

                await UniTask.Delay(1000);

                while(true)
                {
                    await UniTask.Delay(300);
                    if (errorflg)
                    {
                        this.isScanning = false;
                        if (null != device)
                        {
                            device.StopScan();
                        }
                        return null;
                    }
                    if (0 < peripheralList.Count)
                    {
                        break;
                    }
                }
                peripheralList.Sort((a, b) => 0 < (b.rssi - a.rssi) ? 1 : -1);
                this.isScanning = false;
                if (null != device)
                {
                    device.StopScan();
                }

                return peripheralList[0];
            }
            public async UniTask<BLEPeripheralInterface[]> NearScan(int satisfiedNum, float waitSeconds)
            {
#if !UNITY_EDITOR && UNITY_WEBGL
            Debug.Log("[CubeScanner]]NearScan doesn't run on the web");
#endif
                var start_time = Time.time;
                Dictionary<string, BLEPeripheralInterface> peripheralTable = new Dictionary<string, BLEPeripheralInterface>();
                List<BLEPeripheralInterface> peripheralList = new List<BLEPeripheralInterface>();

                Action foundCallback = (() =>
                {
                    string[] uuids = { CubeReal.SERVICE_ID };
                    this.device.Scan(uuids, true, (peripheral) =>
                    {
                        if (this.isScanning && peripheralList.Count < satisfiedNum && !peripheralTable.ContainsKey(peripheral.device_address) && !this.connectedPeripheralTable.ContainsKey(peripheral.device_address))
                        {
                            if (this.peripheralDatabase.ContainsKey(peripheral.device_address))
                            {
                                peripheral = this.peripheralDatabase[peripheral.device_address];
                            }
                            else
                            {
                                this.peripheralDatabase.Add(peripheral.device_address, peripheral);
                                peripheral.AddConnectionListener("CubeScanner.RealImpl", this.OnConnectionEvent);
                            }

                            peripheralList.Add(peripheral);
                            peripheralTable.Add(peripheral.device_address, peripheral);
                        }
                    });
                });

                this.StartScanning(foundCallback);

                await UniTask.Delay(1000);

                while (true)
                {
                    await UniTask.Delay(300);

                    // 必要数に達したらスキャン終了
                    if (satisfiedNum <= peripheralList.Count)
                    {
                        this.isScanning = false;
                        if (null != this.device)
                        {
                            this.device.StopScan();
                        }
                        await UniTask.Delay(100);
                        break;
                    }

                    // 待機時間を超えた場合は一旦関数を終了する
                    var elapsed = Time.time - start_time;
                    if (waitSeconds <= elapsed)
                    {
                        this.isScanning = false;
                        if (null != this.device)
                        {
                            this.device.StopScan();
                        }
                        await UniTask.Delay(100);
                        break;
                    }
                }
                peripheralList.Sort((a, b) => (int)(b.rssi*100) - (int)(a.rssi*100));
                var nearPeripherals = peripheralList.GetRange(0, Mathf.Min(satisfiedNum, peripheralList.Count)).ToArray();
                return nearPeripherals;

            }
            public void NearScanAsync(int satisfiedNum, MonoBehaviour coroutineObject, Action<BLEPeripheralInterface> callback, bool autoRunning)
            {
                this.satisfiedNumForAsync = satisfiedNum;
                this.coroutineObject = coroutineObject;
                this.callback = callback;
                this.autoRunning = autoRunning;
                this.coroutineObject.StartCoroutine(this.ScanCoroutine(satisfiedNum, callback));
            }

            // --- private methods ---
            private IEnumerator ScanCoroutine(int satisfiedNum, Action<BLEPeripheralInterface> callback)
            {
                Dictionary<string, BLEPeripheralInterface> peripheralTable = new Dictionary<string, BLEPeripheralInterface>();

                Action foundCallback = (() =>
                {
                    string[] uuids = { CubeReal.SERVICE_ID };
                    this.device.Scan(uuids, true, (peripheral) =>
                    {
                        if (this.isScanning && peripheralTable.Count < satisfiedNum && !peripheralTable.ContainsKey(peripheral.device_address) && !this.connectedPeripheralTable.ContainsKey(peripheral.device_address))
                        {
                            if (this.peripheralDatabase.ContainsKey(peripheral.device_address))
                            {
                                peripheral = this.peripheralDatabase[peripheral.device_address];
                            }
                            else
                            {
                                this.peripheralDatabase.Add(peripheral.device_address, peripheral);
                                peripheral.AddConnectionListener("CubeScanner.RealImpl", this.OnConnectionEvent);
                                peripheral.AddConnectionListener("CubeScanner.RealImplForAsync", ((peri) => { if (!peri.isConnected) { peripheralTable.Remove(peri.device_address); } }));
                            }

                            peripheralTable.Add(peripheral.device_address, peripheral);
                            callback?.Invoke(peripheral);
                        }
                    });
                });

                this.StartScanning(foundCallback);

                while (true)
                {
                    //Debug.Log("scanning");
                    yield return new WaitForSeconds(0.3f);
                    // 必要数に達したら終了
                    if (satisfiedNum <= peripheralTable.Count)
                    {
                        //Debug.Log("stop scanning");
                        this.isScanning = false;
                        if (null != this.device)
                        {
                            this.device.StopScan();
                        }
                        yield return new WaitForSeconds(0.1f);
                        yield break;
                    }
                }
            }

            private void StartScanning(Action foundCallback)
            {
                this.isScanning = true;
                if (null == this.device)
                {
                    BLEService.Instance.RequestDevice((device) =>
                    {
                        this.device = device;
                        foundCallback?.Invoke();
                    }
                    );
                }
                else
                {
                    foundCallback?.Invoke();
                }
            }

            private void OnConnectionEvent(BLEPeripheralInterface peripheral)
            {
                if (peripheral.isConnected)
                {
                    if (!this.connectedPeripheralTable.ContainsKey(peripheral.device_address))
                    {
                        this.connectedPeripheralTable.Add(peripheral.device_address, peripheral);
                    }
                }
                else
                {
                    if (this.connectedPeripheralTable.ContainsKey(peripheral.device_address))
                    {
                        this.connectedPeripheralTable.Remove(peripheral.device_address);
                    }
                    if (!this.isScanning && this.autoRunning)
                    {
                        this.NearScanAsync(this.satisfiedNumForAsync, this.coroutineObject, this.callback, this.autoRunning);
                    }
                }
            }
        }
    }
}