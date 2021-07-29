using System;
using System.Collections.Generic;

namespace toio
{
    public class BLEWebPeripheral : BLEPeripheralInterface
    {
        public string[] serviceUUIDs { get; private set; }
        public string device_address { get; private set; }
        public string device_name { get; private set; }
        public float rssi { get; private set; }
        public bool isConnected { get; private set; }

        public int serverID { get; private set; }
        public int deviceID { get; private set; }
        private TCallbackProvider<BLEPeripheralInterface> callback = new TCallbackProvider<BLEPeripheralInterface>();

        public BLEWebPeripheral(string[] serviceUUIDs, int deviceID, string uuid, string name)
        {
            this.serviceUUIDs = serviceUUIDs;
            this.device_address = uuid;
            this.device_name = name;
            this.rssi = 0.0f;
            this.isConnected = false;
            this.deviceID = deviceID;
        }

        /// peripheralに接続
        /// [peripheral:1 - characteristic:多]の関係なので、characteristicActionが複数回呼び出される
        public void Connect(Action<BLECharacteristicInterface> characteristicAction)
        {
#if UNITY_WEBGL
            WebBluetoothScript.Instance.Connect(this.deviceID, this.serviceUUIDs[0].ToLower(),
                (serverID, serviceID, serviceUUID) => {
                    this.isConnected = true;
                    this.serverID = serverID;
                    this.ConnectionNotify(this);
                    WebBluetoothScript.Instance.GetCharacteristics(serviceID, (characteristicID, uuid) => {
                        var instance = new BLEWebCharacteristic(serviceID, this.device_address, serviceUUID, characteristicID, uuid);
                        characteristicAction.Invoke(instance);
                    });
                },
                (deviceID) => {
                    this.isConnected = false;
                    this.ConnectionNotify(this);
                }
            );
#endif
        }

        /// <summary>
        /// peripheralを切断
        /// </summary>
        public void Disconnect()
        {
#if UNITY_WEBGL
            WebBluetoothScript.Instance.Disconnect(this.serverID);
#endif
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
        /// 通信実装を追加した場合、明示的にこの関数を呼び出さなければコールバックは呼び出されない
        /// </summary>
        public void ConnectionNotify(BLEPeripheralInterface peri)
        {
            this.callback.Notify(peri);
        }
    }
}