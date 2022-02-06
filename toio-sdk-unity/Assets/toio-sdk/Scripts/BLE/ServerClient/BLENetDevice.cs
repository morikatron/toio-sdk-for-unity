using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using toio.ble.net;
using UnityEngine;

namespace toio
{
    public class BLENetDevice : BLEDeviceInterface
    {
        public BLENetService service { get; private set; }
        public BLENetServer server { get; private set; }
        private Dictionary<string, BLENetPeripheral> peripheralDataBase = new Dictionary<string, BLENetPeripheral>();

        public BLENetDevice(BLENetService _service, BLENetServer _server)
        {
            this.service = _service;
            this.server = _server;
        }

        public void Scan(String[] serviceUUIDs, bool rssiOnly, Action<BLEPeripheralInterface> action)
        {
            this.server.Start();
            this.server.RegisterJoinPeripheralCallback("device", (remoteHost, localIndex, deviceAddr, deviceName, charaList) =>
            {
                BLENetPeripheral peripheral = null;
                Debug.Log(deviceName);
                if ("<null>" == deviceName)
                {
                    Debug.LogError("deviceName is null. please update the firmware version 2.3.0 or higher.");
                    return;
                }
                if (this.peripheralDataBase.ContainsKey(deviceName))
                {
                    peripheral = this.peripheralDataBase[deviceName];
                    peripheral.remoteHost.RemovePeripheral(deviceName);
                }
                else
                {
                    peripheral = new BLENetPeripheral(serviceUUIDs, deviceAddr, deviceName, this.server, charaList);
                    this.peripheralDataBase.Add(deviceName, peripheral);
                }
                peripheral.RegisterRemoteHost(remoteHost, localIndex);
                remoteHost.RegisterPeripheral(deviceName, localIndex, peripheral);
                action(peripheral);
            });
        }

        public void StopScan()
        {

        }

        public UniTask Disconnect(Action action)
        {
            this.server.Close();
            return UniTask.FromResult<object>(null);
        }

        public UniTask Enable(bool enable, Action action)
        {
            if (!enable) this.server.Close();
            return UniTask.FromResult<object>(null);
        }
    }
}