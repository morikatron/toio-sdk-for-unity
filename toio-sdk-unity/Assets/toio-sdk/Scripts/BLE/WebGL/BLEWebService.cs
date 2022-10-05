using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace toio
{
    public class BLEWebService : BLEServiceInterface
    {
        private BLEWebDevice device;

        public BLEWebService()
        {
        }

        public void RequestDevice(Action<BLEDeviceInterface> action)
        {
            WebBluetoothScript.Init();
            if (this.device == null)
            {
                this.device = new BLEWebDevice();
            }
            action.Invoke(this.device);
        }

        public void DisconnectAll()
        {
            this.device.Disconnect(() => {});
        }

        public UniTask Enable(bool enable, Action action)
        {
            //Debug.Log("[BLEWebDevice.Enable]not implemented");
            return UniTask.FromResult<object>(null);
        }
    }
}