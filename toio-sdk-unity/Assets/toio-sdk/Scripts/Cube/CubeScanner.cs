
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_ANDROID && !UNITY_EDITOR
#define SCANNER_RETURN_LIST
#endif

#define TEST_RETURN_SINGLE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using toio.Simulator;
using System.Linq;

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
        UniTask<BLEPeripheralInterface> NearestScan(float waitSeconds = 0f);
        UniTask<BLEPeripheralInterface[]> NearScan(int satisfiedNum, float waitSeconds = 3.0f);
        UniTask StartScan(Action<BLEPeripheralInterface[]> onScanUpdate, Action onScanEnd = null, float timeoutSeconds = 10f);
    }

    public class CubeScanner : CubeScannerInterface
    {
        /// <summary>
        /// Actual type (real or sim) CubeScanner(ConnectType.Auto) will be, depending on current environment.
        /// </summary>
        public static ConnectType actualTypeOfAuto
        {
            get
            {
#if (UNITY_EDITOR || UNITY_STANDALONE)
                return ConnectType.Simulator;
#elif (UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL)
            return ConnectType.Real;
#endif
            }
        }

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

        /// <summary>
        /// Scan for 1 nearest cube.
        /// 0 or negative waitSeconds stands for infinite.
        /// </summary>
        /// <param name="waitSeconds"></param>
        /// <returns></returns>
        public async UniTask<BLEPeripheralInterface> NearestScan(float waitSeconds = 0f) { return await this.impl.NearestScan(waitSeconds); }

        /// <summary>
        /// Scan for 1 nearest cube.
        /// 0 or negative waitSeconds does NOT stand for infinite but the exact value.
        /// </summary>
        /// <param name="waitSeconds"></param>
        /// <returns></returns>
        public async UniTask<BLEPeripheralInterface[]> NearScan(int satisfiedNum, float waitSeconds) { return await this.impl.NearScan(satisfiedNum, waitSeconds); }

        public async UniTask StartScan(Action<BLEPeripheralInterface[]> onScanUpdate, Action onScanEnd = null, float timeoutSeconds = 10f)
        {
            await this.impl.StartScan(onScanUpdate, onScanEnd, timeoutSeconds);
        }

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

            public async UniTask<BLEPeripheralInterface> NearestScan(float waitSeconds)
            {
                float start_time = Time.time;
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

                this.isScanning = true;

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

                    // Scanned
                    if (0 < peripheralList.Count) break;

                    // Time out
                    var elapsed = Time.time - start_time;
                    if (waitSeconds > 0 && waitSeconds <= elapsed)
                        return null;

                    // Searching Period
                    await UniTask.Delay(200);
                }

                peripheralList.Sort((a, b) => b.rssi > a.rssi ? 1 : -1);
                this.isScanning = false;
                return peripheralList.Count > 0 ? peripheralList[0] : null;
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

            public async UniTask StartScan(Action<BLEPeripheralInterface[]> onPeripheralsUpdate, Action onScanEnd = null, float timeoutSeconds = 10f)
            {

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
                        foundObjs.Add(obj);
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
                        if (this.coroutineObject != null)
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
            private Dictionary<string, float> scannedAddrTimes = new Dictionary<string, float>();
            private readonly object locker = new object();

            public RealImpl()
            {
                this.isScanning = false;
#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID)
                if (!BLEService.Instance.hasImplement) { BLEService.Instance.SetImplement(new BLEMobileService()); }
#elif UNITY_WEBGL
                if(!BLEService.Instance.hasImplement) { BLEService.Instance.SetImplement(new BLEWebService()); }
#endif
            }

            public List<BLEPeripheralInterface> peripheralList
            {
                get
                {
                    return this.scannedAddrTimes.Keys.ToList().ConvertAll(addr => this.peripheralDatabase[addr]);
                }
            }

            public async UniTask<BLEPeripheralInterface> NearestScan(float waitSeconds)
            {
                if (this.isScanning) return null;
                this.isScanning = true;
                await this.RequestDevice().Timeout(TimeSpan.FromSeconds(1));
                this.Scan();

                await UniTask.Delay(1000);
                await UniTask.WhenAny(
                    UniTask.Delay((int)Mathf.Max(0, waitSeconds * 1000 - 1100)),
                    UniTask.WaitUntil(() => {
                        return this.scannedAddrTimes.Count(kv=>!this.peripheralDatabase[kv.Key].isConnected) > 0;
                    })
                );

                this.device?.StopScan();
                await UniTask.Delay(100);
                this.isScanning = false;

                // Sort by rssi
                var peris = this.peripheralList.Where(p=>!p.isConnected).ToList();
                peris.Sort((a, b) => 0 < (b.rssi - a.rssi) ? 1 : -1);
                return peris.Count > 0 ? peris[0] : null;
            }

            public async UniTask<BLEPeripheralInterface[]> NearScan(int satisfiedNum, float waitSeconds)
            {
#if !UNITY_EDITOR && UNITY_WEBGL
                Debug.LogWarning("[CubeScanner]]NearScan doesn't run on the web");
#endif
                if (this.isScanning) return null;
                this.isScanning = true;
                await this.RequestDevice().Timeout(TimeSpan.FromSeconds(1));
                this.Scan();

                await UniTask.Delay(1000);
                await UniTask.WhenAny(
                    UniTask.Delay((int)Mathf.Max(0, waitSeconds * 1000 - 1100)),
                    UniTask.WaitUntil(() => {
                        return this.scannedAddrTimes.Count(kv=>!this.peripheralDatabase[kv.Key].isConnected) > satisfiedNum;
                    })
                );

                this.device?.StopScan();
                await UniTask.Delay(100);
                this.isScanning = false;

                // Sort by rssi
                var peris = this.peripheralList.Where(p=>!p.isConnected).ToList();
                peris.Sort((a, b) => 0 < (b.rssi - a.rssi) ? 1 : -1);
                var nearPeripherals = peris.GetRange(0, Mathf.Min(satisfiedNum, peripheralList.Count)).ToArray();
                return nearPeripherals;

            }

            public async UniTask StartScan(Action<BLEPeripheralInterface[]> onScanUpdate, Action onScanEnd = null, float timeoutSeconds = 10f)
            {
                if (this.isScanning) return;
                this.isScanning = true;
                await this.RequestDevice().Timeout(TimeSpan.FromSeconds(1));
                this.Scan(onScanUpdate);

                await UniTask.Delay((int)Mathf.Max(0, timeoutSeconds * 1000));
                this.device?.StopScan();
                this.isScanning = false;
                onScanEnd?.Invoke();
            }

            // --- private methods ---
            private void Scan(Action<BLEPeripheralInterface[]> onScanUpdate = null)
            {
                this.scannedAddrTimes.Clear();
                string[] uuids = { CubeReal.SERVICE_ID };

                this.device.Scan(uuids, true, (peripherals) =>
                {
                    if (!isScanning) return;
#if SCANNER_RETURN_LIST && !TEST_RETURN_SINGLE
                    // Plugin for windows and android returns whole list of peripherals
                    this.scannedAddrTimes.Clear();
#endif
                    foreach (var peri in peripherals) {
                        var peripheral = peri;
                        if (null == peripheral)
                            continue;

                        if (this.peripheralDatabase.ContainsKey(peripheral.device_address))
                        {
                            peripheral = this.peripheralDatabase[peripheral.device_address];
                        }
                        else
                        {
                            this.peripheralDatabase.Add(peripheral.device_address, peripheral);
                        }

#if !SCANNER_RETURN_LIST || TEST_RETURN_SINGLE
                        // Update scannedAddrTimes
                        lock(locker) {
                            if (!scannedAddrTimes.ContainsKey(peripheral.device_address)){
                                scannedAddrTimes.Add(peripheral.device_address, Time.realtimeSinceStartup);
                            } else {
                                scannedAddrTimes[peripheral.device_address] = Time.realtimeSinceStartup;
                            }
                        }
#else
                        this.scannedAddrTimes.Add(peripheral.device_address, Time.realtimeSinceStartup);
#endif
                    }
#if !SCANNER_RETURN_LIST || TEST_RETURN_SINGLE
                    // Remove overdated peripherals
                    lock(locker) {
                        foreach (var addr in scannedAddrTimes.Keys.ToArray()){
                            if (Time.realtimeSinceStartup - scannedAddrTimes[addr] > 0.6f)
                                this.scannedAddrTimes.Remove(addr);
                        }
                    }
#endif
                    onScanUpdate?.Invoke(this.peripheralList.ToArray());
                });

                this.CleaningOverdated(onScanUpdate).Forget();
            }

            private async UniTask CleaningOverdated(Action<BLEPeripheralInterface[]> onScanUpdate) {
                while (this.isScanning) {
                    await UniTask.Delay(100);
                    var changed = false;
                    lock(locker) {
                        foreach (var addr in scannedAddrTimes.Keys.ToArray()){
                            if (Time.realtimeSinceStartup - scannedAddrTimes[addr] > 0.6f) {
                                this.scannedAddrTimes.Remove(addr);
                                changed = true;
                            }
                        }
                    }
                    if (changed) onScanUpdate?.Invoke(this.peripheralList.ToArray());
                }
            }

            private async UniTask<BLEDeviceInterface> RequestDevice()
            {
                if (this.device != null)
                    return await UniTask.FromResult(this.device);

                var utcs = new UniTaskCompletionSource<BLEDeviceInterface>();
                BLEService.Instance.RequestDevice((device) =>
                {
                    this.device = device;
                    utcs.TrySetResult(device);
                });
                return await utcs.Task;
            }

        }
    }
}