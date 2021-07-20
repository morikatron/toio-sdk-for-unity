using System;
using System.Collections.Generic;
using UnityEngine;

namespace toio
{
    using Simulator;

    public class UnityPeripheral : BLEPeripheralInterface
    {
        public GameObject obj { get; private set; }
        public CubeSimulator simulator { get; private set; }
        public string[] serviceUUIDs { get; }
        public string device_address { get; private set; }
        // TODO this differs from real, where deviec_name is constant.
        public string device_name { get { return obj.name; } }
        // TODO this is same to real, where rssi is only set by constructor.
        public float rssi { get; private set; }
        public bool isConnected { get; private set; }
        private TCallbackProvider<BLEPeripheralInterface> callback;

        public UnityPeripheral(GameObject _obj)
        {
            this.obj = _obj;
            this.simulator = this.obj.GetComponent<CubeSimulator>();
            if (this.simulator == null)
                Debug.LogError("The GameObject given to UnityPeripheral does not have a CubeSimulator component!");
            this.device_address = obj.GetInstanceID().ToString();
            var pos = obj.transform.position;
            this.rssi = pos.x+pos.y+pos.z;

            this.callback = new TCallbackProvider<BLEPeripheralInterface>();
        }
        public void Connect(Action<BLECharacteristicInterface> characteristicAction)
        {
            this.simulator.Connect(OnConnected, OnDisconnected);
        }

        public void Disconnect()
        {
            this.simulator.Disconnect();
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


        private void OnConnected()
        {
            if (!this.isConnected)
            {
                this.isConnected = true;
                this.ConnectionNotify(this);
            }
        }

        private void OnDisconnected()
        {
            if (this.isConnected)
            {
                this.isConnected = false;
                this.ConnectionNotify(this);
            }
        }
    }
}