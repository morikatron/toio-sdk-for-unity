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
        public void Scan(String[] serviceUUIDs, bool rssiOnly, Action<BLEPeripheralInterface[]> action, Action<string> errorAction = null)
        {
#if UNITY_WEBGL
            WebBluetoothScript.RequestDevice(serviceUUIDs[0].ToLower(), (deviceID, uuid, name) => {
                var _peripherals = new BLEWebPeripheral[1];
                var peripheral = new BLEWebPeripheral(serviceUUIDs, deviceID, uuid, name);
                _peripherals[0] = peripheral;
                peripherals.Add(peripheral);
                action.Invoke(_peripherals);
            }, (errMsg) => {
                Debug.LogFormat("[BLEWebDevice.Scan]Error: {0}", errMsg);
                errorAction?.Invoke(errMsg);
                action.Invoke(new BLEPeripheralInterface[0]);
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
                WebBluetoothScript.Disconnect(peri.serverID);
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