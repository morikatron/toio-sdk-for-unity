using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace toio
{
    public class BLENetDevice : BLEDeviceInterface
    {
        public BLENetService service { get; private set; }
        public BLENetServer server { get; private set; }

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
                var peripheral = new BLENetPeripheral(serviceUUIDs, deviceAddr, deviceName, this.server, remoteHost, localIndex, charaList);
                remoteHost.AddPeripheral(localIndex, peripheral);
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