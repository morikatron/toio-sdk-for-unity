using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class BLEWebDevice : BLEDeviceInterface
    {
        public List<BLEWebPeripheral> peripherals = new List<BLEWebPeripheral>();

        public BLEWebDevice()
        {
        }
        public void Scan(String[] serviceUUIDs, bool rssiOnly, Action<BLEPeripheralInterface> action)
        {
#if UNITY_WEBGL
            WebBluetoothScript.Instance.RequestDevice(serviceUUIDs[0].ToLower(), (deviceID, uuid, name) => {
                var peripheral = new BLEWebPeripheral(serviceUUIDs, deviceID, uuid, name);
                peripherals.Add(peripheral);
                action.Invoke(peripheral);
            }, (errMsg) => {
                Debug.LogFormat("[BLEWebDevice.Scan]Error: {0}", errMsg);
                action.Invoke(null);
            });
#endif
        }
        public void StopScan()
        {
            //Debug.Log("[BLEWebDevice.StopScan]not implemented");
        }
        public UniTask Disconnect(Action action)
        {
#if UNITY_WEBGL
            foreach(var peri in this.peripherals)
            {
                WebBluetoothScript.Instance.Disconnect(peri.serverID);
            }
            this.peripherals.Clear();
#endif
            return UniTask.FromResult<object>(null);
        }
        public UniTask Enable(bool enable, Action action)
        {
            //Debug.Log("[BLEWebDevice.Enable]not implemented");
            return UniTask.FromResult<object>(null);
        }
    }
}