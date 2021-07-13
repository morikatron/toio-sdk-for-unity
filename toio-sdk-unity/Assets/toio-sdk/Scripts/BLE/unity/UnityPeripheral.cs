using System;
using System.Collections.Generic;
using UnityEngine;

namespace toio
{
    public class UnityPeripheral : BLEPeripheralInterface
    {
        public GameObject obj;
        public string[] serviceUUIDs { get; }
        public string device_address { get { return obj.GetInstanceID().ToString(); } }
        public string device_name { get { return obj.name; } }
        public float rssi { get { var p = obj.transform.position; return p.x+p.y+p.z; } }
        public bool isConnected { get; private set; }
        private TCallbackProvider<BLEPeripheralInterface> callback;
        public List<BLECharacteristicInterface> connectedcharacteristics { get; private set; }

        public UnityPeripheral(GameObject _obj)
        {
            this.obj = _obj;
            this.callback = new TCallbackProvider<BLEPeripheralInterface>();
            this.connectedcharacteristics = new List<BLECharacteristicInterface>();
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


        private void OnConnected(string device_address)
        {
            if (!this.isConnected)
            {
                this.isConnected = true;
                this.ConnectionNotify(this);
            }
        }

        private void OnDisconnected(string device_address)
        {
            if (this.isConnected)
            {
                this.isConnected = false;
                this.connectedcharacteristics.Clear();
                this.ConnectionNotify(this);
            }
        }
    }
}