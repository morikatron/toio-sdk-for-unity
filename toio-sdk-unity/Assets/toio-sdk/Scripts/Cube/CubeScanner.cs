
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_ANDROID && !UNITY_EDITOR
#define SCANNER_RETURN_LIST
#endif

// #define TEST_RETURN_SINGLE

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
        UniTask StartScan(Action<BLEPeripheralInterface[]> onScanUpdate, Action onScanEnd = null, float waitSeconds = 10f);
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

        public async UniTask StartScan(Action<BLEPeripheralInterface[]> onScanUpdate, Action onScanEnd = null, float waitSeconds = 10f)
        {
            await this.impl.StartScan(onScanUpdate, onScanEnd, waitSeconds);
        }

        /// <summary>
        /// Impl for UnitySimulator.
        /// </summary>
        public class SimImpl : CubeScannerInterface
        {
            public bool isScanning { get; private set; }
            private Dictionary<string, BLEPeripheralInterface> peripheralDatabase = new Dictionary<string, BLEPeripheralInterface>();
            private List<string> scannedAddrs = new List<string>();
            private readonly object locker = new object();

            public SimImpl()
            {
                this.isScanning = false;
            }

            public List<BLEPeripheralInterface> peripheralList
            {
                get
                {
                    return this.scannedAddrs.ConvertAll(addr => this.peripheralDatabase[addr]);
                }
            }

            public async UniTask<BLEPeripheralInterface> NearestScan(float waitSeconds)
            {
                if (this.isScanning) return null;
                isScanning = true;

                this.Scan().Forget();

                await UniTask.Delay(1000);
                await UniTask.WhenAny(
                    UniTask.Delay((int)Mathf.Max(0, waitSeconds * 1000 - 1100)),
                    UniTask.WaitUntil(() => {
                        return this.scannedAddrs.Count(addr=>!peripheralDatabase[addr].isConnected) > 0;
                    })
                );

                this.isScanning = false;
                await UniTask.Delay(200);

                peripheralList.Sort((a, b) => b.rssi > a.rssi ? 1 : -1);
                return peripheralList.Count > 0 ? peripheralList[0] : null;
            }
            public async UniTask<BLEPeripheralInterface[]> NearScan(int satisfiedNum, float waitSeconds)
            {
                if (this.isScanning) return null;
                isScanning = true;

                this.Scan().Forget();

                await UniTask.Delay(1000);
                await UniTask.WhenAny(
                    UniTask.Delay((int)Mathf.Max(0, waitSeconds * 1000 - 1100)),
                    UniTask.WaitUntil(() => {
                        return this.scannedAddrs.Count(addr=>!peripheralDatabase[addr].isConnected) >= satisfiedNum;
                    })
                );

                this.isScanning = false;
                await UniTask.Delay(200);

                var peripherals = this.peripheralList;
                peripherals.Sort((a, b) => b.rssi > a.rssi ? 1 : -1);
                var nearPeripherals = peripherals.GetRange(0, Mathf.Min(satisfiedNum, peripherals.Count)).ToArray();
                return nearPeripherals;
            }
            public async UniTask StartScan(Action<BLEPeripheralInterface[]> onScanUpdate, Action onScanEnd = null, float waitSeconds = 10f)
            {
                if (this.isScanning) return;
                isScanning = true;

                this.Scan(onScanUpdate).Forget();

                await UniTask.Delay((int)Mathf.Max(0, waitSeconds * 1000));
                this.isScanning = false;
                await UniTask.Delay(200);
                onScanEnd?.Invoke();
            }

            // --- private methods ---
            private async UniTask Scan(Action<BLEPeripheralInterface[]> onScanUpdate = null)
            {
                List<string> addrs = new List<string>();
                while (this.isScanning)
                {
                    addrs.Clear();
                    // Search for new cube object
                    var objs = Array.ConvertAll(GameObject.FindObjectsOfType<CubeSimulator>(), sim => sim.gameObject);
                    foreach (var obj in objs)
                    {
                        if (!obj.GetComponent<CubeSimulator>().isRunning) continue;
                        if (obj.GetComponent<CubeSimulator>().isConnected) continue;

                        var peripheral = new UnityPeripheral(obj) as BLEPeripheralInterface;

                        if (this.peripheralDatabase.ContainsKey(peripheral.device_address))
                        {
                            peripheral = this.peripheralDatabase[peripheral.device_address];
                        }
                        else
                        {
                            this.peripheralDatabase.Add(peripheral.device_address, peripheral);
                        }
                        addrs.Add(peripheral.device_address);
                    }

                    lock (locker) {
                        this.scannedAddrs.Clear();
                        this.scannedAddrs.AddRange(addrs);
                    }
                    onScanUpdate?.Invoke(this.peripheralList.ToArray());

                    // Searching Period
                    await UniTask.Delay(200);
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
                        return this.scannedAddrTimes.Count(kv=>!this.peripheralDatabase[kv.Key].isConnected) >= satisfiedNum;
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

            public async UniTask StartScan(Action<BLEPeripheralInterface[]> onScanUpdate, Action onScanEnd = null, float waitSeconds = 10f)
            {
                if (this.isScanning) return;
                this.isScanning = true;
                await this.RequestDevice().Timeout(TimeSpan.FromSeconds(1));
                this.Scan(onScanUpdate);

                await UniTask.Delay((int)Mathf.Max(0, waitSeconds * 1000));
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