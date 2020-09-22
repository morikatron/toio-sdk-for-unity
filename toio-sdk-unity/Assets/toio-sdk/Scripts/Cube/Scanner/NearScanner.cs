using System;
using System.Collections;
//using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    /// <summary>
    /// Interface.
    /// </summary>
    public interface NearScannerInterface
    {
        int satisfiedNum { get; }
        int scanningNum { get; }
        bool isScanning { get; }

        UniTask<BLEPeripheralInterface[]> Scan(float waitSeconds = 3.0f);
        void ScanAsync(MonoBehaviour coroutineObject, Action<BLEPeripheralInterface> callback, bool autoRunning = true);
    }

    /// <summary>
    /// Abstraction in bridge pattern.
    /// </summary>
    public class NearScanner : NearScannerInterface
    {
        // --- public fields ---
        public int satisfiedNum { get { return this.impl.satisfiedNum; } }
        public int scanningNum { get { return this.impl.scanningNum; } }
        public bool isScanning { get { return this.impl.isScanning; } }

        // --- private fields ---
        private NearScannerInterface impl;

        // --- public methods ---
        public NearScanner(int satisfiedNum, NearScannerInterface impl = null)
        {
#if UNITY_WEBGL
            Debug.Log("[NearScanner]NearScanner doesn't run on the web");
#endif
            if (null != impl)
            {
                // NearScannerの内部実装を外部入力から変更
                this.impl = impl;
            }
            else
            {
                // プリセットで用意したマルチプラットフォーム内部実装(UnityEditor/Mobile/WebGL)
                this.impl = new Impl(satisfiedNum);
            }
        }

        public async UniTask<BLEPeripheralInterface[]> Scan(float waitSeconds = 3.0f)
        {
            return await this.impl.Scan(waitSeconds);
        }

        public void ScanAsync(MonoBehaviour coroutineObject, Action<BLEPeripheralInterface> callback, bool autoRunning=true)
        {
            this.impl.ScanAsync(coroutineObject, callback, autoRunning);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Impl for Unity.
        /// </summary>
        public class Impl : NearScannerInterface
        {
            // --- public fields ---
            public int satisfiedNum { get; }
            public int scanningNum { get { return this.peripheralList.Count; } }
            public bool isScanning { get { return false; } }
            private HashSet<int> IDHash = new HashSet<int>();

            // --- private fields ---
            private List<UnityPeripheral> peripheralList = new List<UnityPeripheral>();

            // --- public methods ---
            public Impl(int satisfiedNum)
            {
                this.satisfiedNum = satisfiedNum;
            }

            public UniTask<BLEPeripheralInterface[]> Scan(float waitSeconds = 3.0f)
            {
                if (this.satisfiedNum <= this.scanningNum) { return default; }

                var objs = GameObject.FindGameObjectsWithTag("Cube");
                foreach (var obj in objs)
                {
                    if (!this.IDHash.Contains(obj.GetInstanceID()) && this.peripheralList.Count < this.satisfiedNum)
                    {
                        this.IDHash.Add(obj.GetInstanceID());
                        var peri = new UnityPeripheral(obj);
                        this.AddPeripheral(peri);
                    }
                }
                return new UniTask<BLEPeripheralInterface[]>(this.peripheralList.ToArray() as BLEPeripheralInterface[]);
            }

            public void ScanAsync(MonoBehaviour coroutineObject, Action<BLEPeripheralInterface> callback, bool autoRunning)
            {
                if (this.satisfiedNum <= this.scanningNum) { return; }

                var objs = GameObject.FindGameObjectsWithTag("Cube");
                foreach (var obj in objs)
                {
                    if (!this.IDHash.Contains(obj.GetInstanceID()) && this.peripheralList.Count < this.satisfiedNum)
                    {
                        this.IDHash.Add(obj.GetInstanceID());
                        var peri = new UnityPeripheral(obj);
                        this.AddPeripheral(peri);
                        callback(peri);
                    }
                }
            }

            // --- private methods ---
            private void AddPeripheral(UnityPeripheral peri)
            {
                this.peripheralList.Add(peri);
            }
        }
#elif (UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL)
        /// <summary>
        /// Impl for Mobile(iOS, Android) and WebGL
        /// </summary>
        public class Impl : NearScannerInterface
        {
            // --- public fields ---
            public int satisfiedNum { get; private set; }
            public int scanningNum { get { return this.peripheralList.Count; } }
            public bool isScanning { get; private set; }

            // --- private fields ---
            // ble
            private BLEDeviceInterface device;
            private Dictionary<string, BLEPeripheralInterface> peripheralDatabase = new Dictionary<string, BLEPeripheralInterface>();
            private Dictionary<string, BLEPeripheralInterface> peripheralTable = new Dictionary<string, BLEPeripheralInterface>();
            private List<BLEPeripheralInterface> peripheralList = new List<BLEPeripheralInterface>();
            // connection async
            private MonoBehaviour coroutineObject;
            private Action<BLEPeripheralInterface> callback;
            private bool autoRunning;

            // --- public methods ---
            public Impl(int satisfiedNum)
            {
                this.satisfiedNum = satisfiedNum;
                this.isScanning = false;
                this.autoRunning = false;

#if (UNITY_IOS || UNITY_ANDROID)
                if(!BLEService.Instance.hasImplement)
                {
                    BLEService.Instance.SetImplement(new BLEMobileService());
                }
#elif UNITY_WEBGL
                if(!BLEService.Instance.hasImplement)
                {
                    BLEService.Instance.SetImplement(new BLEWebService());
                }
#endif
            }

            public async UniTask<BLEPeripheralInterface[]> Scan(float waitSeconds = 3.0f)
            {
                var start_time = Time.time;

                this.StartScanning();

                await UniTask.Delay(1000);

                while (true)
                {
                    await UniTask.Delay(300);

                    // 必要数に達したらスキャン終了
                    if (this.satisfiedNum <= this.scanningNum)
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
                this.peripheralList.Sort((a, b) => (int)(b.rssi*100) - (int)(a.rssi*100));
                var nearPeripherals = this.peripheralList.GetRange(0, this.scanningNum).ToArray();
                return nearPeripherals;
            }

            public void ScanAsync(MonoBehaviour coroutineObject, Action<BLEPeripheralInterface> callback, bool autoRunning)
            {
                this.coroutineObject = coroutineObject;
                this.callback = callback;
                this.autoRunning = autoRunning;
                this.coroutineObject.StartCoroutine(this.ScanCoroutine(callback));
            }

            // --- private methods ---
            private IEnumerator ScanCoroutine(Action<BLEPeripheralInterface> callback)
            {
                this.StartScanning(callback);

                while (true)
                {
                    //Debug.Log("scanning");
                    yield return new WaitForSeconds(0.3f);
                    // 必要数に達したら終了
                    if (this.satisfiedNum <= this.scanningNum)
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

            private void StartScanning(Action<BLEPeripheralInterface> callback = null)
            {
                this.isScanning = true;
                if (null == this.device)
                {
                    BLEService.Instance.RequestDevice((device) =>
                    {
                        this.device = device;
                        this.Scanning(callback);
                    }
                    );
                }
                else
                {
                    this.Scanning(callback);
                }
            }

            private void Scanning(Action<BLEPeripheralInterface> callback = null)
            {
                string[] uuids = { CubeReal.SERVICE_ID };
                this.device.Scan(uuids, true, (peripheral) =>
                {
                    if (this.isScanning && this.scanningNum < this.satisfiedNum && !this.peripheralTable.ContainsKey(peripheral.device_address))
                    {
                        if (this.peripheralDatabase.ContainsKey(peripheral.device_address))
                        {
                            peripheral = this.peripheralDatabase[peripheral.device_address];
                        }
                        else
                        {
                            this.peripheralDatabase.Add(peripheral.device_address, peripheral);
                            peripheral.AddConnectionListener("NearScanner", this.OnConnectionEvent);
                        }

                        this.peripheralList.Add(peripheral);
                        this.peripheralTable.Add(peripheral.device_address, peripheral);

                        if (null != callback)
                        {
                            callback(peripheral);
                        }
                    }
                });
            }

            private void OnConnectionEvent(BLEPeripheralInterface peripheral)
            {
                if (!peripheral.isConnected)
                {
                    var instance = this.peripheralTable[peripheral.device_address];
                    this.peripheralTable.Remove(instance.device_address);
                    this.peripheralList.Remove(instance);

                    if (!this.isScanning && this.autoRunning)
                    {
                        this.ScanAsync(this.coroutineObject, this.callback, this.autoRunning);
                    }
                }
            }
        }
#endif
    }
}