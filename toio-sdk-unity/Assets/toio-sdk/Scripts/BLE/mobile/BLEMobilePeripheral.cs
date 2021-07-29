using System;
using System.Collections.Generic;

namespace toio
{
    public class BLEMobilePeripheral : BLEPeripheralInterface
    {
        public string[] serviceUUIDs { get; private set; }
        public string device_address { get; private set; }
        public string device_name { get; private set; }
        public float rssi { get; private set; }
        public bool isConnected { get; private set; }

        private TCallbackProvider<BLEPeripheralInterface> callback;

        public BLEMobilePeripheral(string[] serviceUUIDs, string device_address, string device_name, float rssi)
        {
            device_address = device_address.ToUpper();
            this.serviceUUIDs = serviceUUIDs;
            this.device_address = device_address;
            this.device_name = device_name;
            this.rssi = rssi;
            this.callback = new TCallbackProvider<BLEPeripheralInterface>();
            this.isConnected = false;
        }

        /// <summary>
        /// peripheralに接続
        /// [peripheral:1 - characteristic:多]の関係なので、characteristicActionが複数回呼び出される
        /// </summary>
        public void Connect(Action<BLECharacteristicInterface> characteristicAction)
        {
            Ble.ConnectToPeripheral(this.device_address, OnConnected, null, (address, serviceUUID, characteristicUUID) =>
            {
                //Debug.Log("address=" + address + ". uuid=" + serviceUUID + ". chara=" + characteristicUUID);
                var instance = new BLEMobileCharacteristic(this.device_address, serviceUUID, characteristicUUID);
                characteristicAction(instance);
            }, this.OnDisconnected);
        }

        /// <summary>
        /// peripheralを切断
        /// </summary>
        public void Disconnect()
        {
            Ble.DisconnectPeripheral(this.device_address);
        }

        /// <summary>
        /// 接続/切断コールバックを登録
        /// </summary>
        public void AddConnectionListener(string key, Action<BLEPeripheralInterface> action)
        {
            this.callback.AddListener(key, action);
        }

        /// <summary>
        /// 接続/切断コールバックを解除
        /// </summary>
        public void RemoveConnectionListener(string key)
        {
            this.callback.RemoveListener(key);
        }

        /// <summary>
        /// 接続/切断コールバックを呼び出し
        /// </summary>
        public void ConnectionNotify(BLEPeripheralInterface peri)
        {
            this.callback.Notify(peri);
        }

        /// <summary>
        /// 通信コールバック(接続時)
        /// </summary>
        private void OnConnected(string device_address)
        {
            if (!this.isConnected)
            {
                this.isConnected = true;
                this.ConnectionNotify(this);
            }
        }

        /// <summary>
        /// 通信コールバック(切断時)
        /// </summary>
        private void OnDisconnected(string device_address)
        {
            if (this.isConnected)
            {
                this.isConnected = false;
                this.ConnectionNotify(this);
            }
        }
    }
}
