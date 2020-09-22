using System;
using UnityEngine;

namespace toio
{
    public class UnityPeripheral : BLEPeripheralInterface
    {
        public GameObject obj;
        public string[] serviceUUIDs { get; }
        public string device_address { get { return obj.GetInstanceID().ToString(); } }
        public string device_name { get { return obj.name; } }
        public float rssi { get; }
        public bool isConnected { get { return true; } }
        private TCallbackProvider<BLEPeripheralInterface> callback;

        public UnityPeripheral(GameObject _obj)
        {
            this.obj = _obj;
            this.callback = new TCallbackProvider<BLEPeripheralInterface>();
        }
        public void Connect(Action<BLECharacteristicInterface> characteristicAction)
        {

        }

        /// <summary>
        /// peripheralを切断
        /// </summary>
        public void Disconnect()
        {
        }

        public void AddConnectionListener(string key, Action<BLEPeripheralInterface> action)
        {
            this.callback.AddListener(key, action);
        }
        public void RemoveConnectionListener(string key)
        {
            this.callback.RemoveListener(key);
        }
        public void ConnectionNotify(BLEPeripheralInterface peri)
        {
            this.callback.Notify(peri);
        }
    }
}