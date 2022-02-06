using System;
using System.Collections.Generic;
using UnityEngine;
using toio.ble.net;

namespace toio
{
    public class BLENetPeripheral : BLEPeripheralInterface
    {
        public string[] serviceUUIDs { get; private set; }
        public string device_address { get; private set; }
        public string device_name { get; private set; }
        public float rssi { get; private set; }
        public bool isConnected { get { return this.remoteHost.udpClient.IsConnected; } }

        public BLENetServer server { get; private set; }
        public BLENetRemoteHost remoteHost { get; private set; }
        public int localIndex { get; private set; }
        private Dictionary<string, BLENetCharacteristic> characteristicTable = new Dictionary<string, BLENetCharacteristic>();
        private TCallbackProvider<BLEPeripheralInterface> callback = new TCallbackProvider<BLEPeripheralInterface>();

        public BLENetPeripheral(string[] serviceUUIDs, string device_address, string device_name, BLENetServer server, string[] charaNames)
        {
            this.serviceUUIDs = serviceUUIDs;
            this.device_address = device_address.ToUpper();
            this.device_name = device_name;
            this.rssi = 1.0f;

            this.server = server;

            foreach(var chara in charaNames)
            {
                var c = new BLENetCharacteristic(device_address, serviceUUIDs[0], chara, BLENetProtocol.Characteristic2ShortID(chara), this);
                this.characteristicTable.Add(chara, c);
            }
        }

        public void RegisterRemoteHost(BLENetRemoteHost remoteHost, int localIndex)
        {
            this.remoteHost = remoteHost;
            this.localIndex = localIndex;
            Func<bool> checkConnection = () => {
                if (!this.remoteHost.IsConnected)
                {
                    this.ConnectionNotify(this);
                    return true;
                }
                return false;
             };
            this.server.FuncTable.Add(this.GetHashCode(), checkConnection);
        }

        /// <summary>
        /// peripheralに接続
        /// [peripheral:1 - characteristic:多]の関係なので、characteristicActionが複数回呼び出される
        /// </summary>
        public void Connect(Action<BLECharacteristicInterface> characteristicAction)
        {
            this.ConnectionNotify(this);
            foreach(var chara in this.characteristicTable)
            {
                characteristicAction(chara.Value);
            }
        }

        /// <summary>
        /// peripheralを切断
        /// </summary>
        public void Disconnect()
        {

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
    }
}