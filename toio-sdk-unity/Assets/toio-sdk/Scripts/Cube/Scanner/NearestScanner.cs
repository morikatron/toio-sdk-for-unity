//using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    public interface NearestScannerInterface
    {
        UniTask<BLEPeripheralInterface> Scan();
    }

    public class NearestScanner : NearestScannerInterface
    {
        private NearestScannerInterface impl;

        public NearestScanner(NearestScannerInterface impl = null)
        {
            if (null != impl)
            {
                // NearestScannerの内部実装を外部入力から変更
                this.impl = impl;
            }
            else
            {
                // プリセットで用意したマルチプラットフォーム内部実装(UnityEditor/Mobile/WebGL)
                this.impl = new Impl();
            }
        }

        public async UniTask<BLEPeripheralInterface> Scan()
        {
            return await this.impl.Scan();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Impl for Unity.
        /// </summary>
        public class Impl : NearestScannerInterface
        {
            private HashSet<int> IDHash = new HashSet<int>();
            public UniTask<BLEPeripheralInterface> Scan()
            {
                var cubeObjects = GameObject.FindGameObjectsWithTag("Cube");
                foreach(var obj in cubeObjects)
                {
                    if(!this.IDHash.Contains(obj.GetInstanceID()))
                    {
                        this.IDHash.Add(obj.GetInstanceID());
                        return new UniTask<BLEPeripheralInterface>(new UnityPeripheral(obj) as BLEPeripheralInterface);
                    }
                }
                return UniTask.FromResult<BLEPeripheralInterface>(null);
            }
        }
#elif (UNITY_ANDROID || UNITY_IPHONE)
        /// <summary>
        /// Impl for Mobile(iOS, Android).
        /// </summary>
        public class Impl : NearestScannerInterface
        {
            private Dictionary<string, BLEPeripheralInterface> peripheralTable = new Dictionary<string, BLEPeripheralInterface>();

            public Impl()
            {
                if(!BLEService.Instance.hasImplement)
                {
                    BLEService.Instance.SetImplement(new BLEMobileService());
                }
            }

            public async UniTask<BLEPeripheralInterface> Scan()
            {
                List<BLEPeripheralInterface> peripheralList = new List<BLEPeripheralInterface>();
                BLEDeviceInterface device = null;
                BLEService.Instance.RequestDevice((_device) =>
                {
                    device = _device;
                    string[] uuids = { CubeReal.SERVICE_ID };
                    device.Scan(uuids, true, (peripheral) =>
                    {
                        if (!this.peripheralTable.ContainsKey(peripheral.device_address))
                        {
                            peripheralList.Add(peripheral);
                            this.peripheralTable.Add(peripheral.device_address, peripheral);
                            peripheral.AddConnectionListener("NearestScanner", this.OnConnectionEvent);
                        }
                    });
                }
                );

                await UniTask.Delay(1000);

                while(true)
                {
                    await UniTask.Delay(300);
                    if (0 < peripheralList.Count)
                    {
                        break;
                    }
                }
                peripheralList.Sort((a, b) => 0 < (b.rssi - a.rssi) ? 1 : -1);
                if (null != device)
                {
                    device.StopScan();
                }

                return peripheralList[0];
            }

            private void OnConnectionEvent(BLEPeripheralInterface peripheral)
            {
                if (!peripheral.isConnected)
                {
                    var instance = this.peripheralTable[peripheral.device_address];
                    this.peripheralTable.Remove(instance.device_address);
                }
            }
        }
#elif UNITY_WEBGL
        /// <summary>
        /// Impl for WebGL.
        /// </summary>
        public class Impl : NearestScannerInterface
        {
            private Dictionary<string, BLEPeripheralInterface> peripheralTable = new Dictionary<string, BLEPeripheralInterface>();

            public Impl()
            {
                if(!BLEService.Instance.hasImplement)
                {
                    BLEService.Instance.SetImplement(new BLEWebService());
                }
            }
            public async UniTask<BLEPeripheralInterface> Scan()
            {
                List<BLEPeripheralInterface> peripheralList = new List<BLEPeripheralInterface>();
                BLEDeviceInterface device = null;
                bool errorflg = false;
                BLEService.Instance.RequestDevice((_device) =>
                {
                    device = _device;
                    string[] uuids = { CubeReal.SERVICE_ID };
                    device.Scan(uuids, true, (peripheral) =>
                    {
                        if (null == peripheral)
                        {
                            errorflg = true;
                            return;
                        }
                        if (!this.peripheralTable.ContainsKey(peripheral.device_address))
                        {
                            peripheralList.Add(peripheral);
                            this.peripheralTable.Add(peripheral.device_address, peripheral);
                            peripheral.AddConnectionListener("NearestScanner", this.OnConnectionEvent);
                        }
                    });
                }
                );

                await UniTask.Delay(1000);

                while(true)
                {
                    await UniTask.Delay(300);
                    if (errorflg)
                    {
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
                if (null != device)
                {
                    device.StopScan();
                }

                return peripheralList[0];
            }

            private void OnConnectionEvent(BLEPeripheralInterface peripheral)
            {
                if (!peripheral.isConnected)
                {
                    var instance = this.peripheralTable[peripheral.device_address];
                    this.peripheralTable.Remove(instance.device_address);
                }
            }
        }
#endif
    }
}
