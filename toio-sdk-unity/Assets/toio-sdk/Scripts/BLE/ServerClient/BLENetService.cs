using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using toio.ble.net;

namespace toio
{
    public class BLENetService : BLEServiceInterface
    {
        public BLENetServer server { get; private set; }
        public string listenAddr { get; private set; }
        public int listenPort { get; private set; }
        private GameObject workerObject;

        public BLENetService(GameObject workerObject, string listenAddr, int listenPort)
        {
            this.workerObject = workerObject;
            this.listenAddr = listenAddr;
            this.listenPort = listenPort;
        }

        public void RequestDevice(Action<BLEDeviceInterface> action)
        {
            this.server = new BLENetServer(workerObject:this.workerObject, listenAddr:this.listenAddr, listenPort:this.listenPort);
            if (null != action) action(new BLENetDevice(this, server));
        }

        public UniTask Enable(bool enable, Action action)
        {
            if (!enable) this.server.Close();
            return UniTask.FromResult<object>(null);
        }

        public void DisconnectAll()
        {
            this.server.Close();
        }
    }
}
