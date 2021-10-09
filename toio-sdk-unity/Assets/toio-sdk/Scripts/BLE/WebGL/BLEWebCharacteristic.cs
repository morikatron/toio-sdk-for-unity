using System;

namespace toio
{
    public class BLEWebCharacteristic : BLECharacteristicInterface
    {
        public string deviceAddress { get; private set; }
        public string serviceUUID { get; private set; }
        public string characteristicUUID { get; }
        public TCallbackProvider<string, byte[], bool> writeDataCallback { get; private set; }
        public TCallbackProvider<string, byte[]> readDataCallback { get; private set; }
        public TCallbackProvider<string, byte[]> notifiedCallback { get; private set; }

        private int serviceID;
        private int characteristicID;

        public BLEWebCharacteristic(int serviceID, string deviceAddress, string serviceUUID, int characteristicID, string characteristicUUID)
        {
            this.serviceID = serviceID;
            this.deviceAddress = deviceAddress;
            this.serviceUUID = serviceUUID.ToUpper();
            this.characteristicID = characteristicID;
            this.characteristicUUID = characteristicUUID.ToUpper();
            this.writeDataCallback = new TCallbackProvider<string, byte[], bool>();
            this.readDataCallback = new TCallbackProvider<string, byte[]>();
            this.notifiedCallback = new TCallbackProvider<string, byte[]>();
        }

        public void ReadValue(Action<string, byte[]> action)
        {
#if UNITY_WEBGL
            WebBluetoothScript.Instance.ReadValue(this.characteristicID, (id, bytes) =>
            {
                action(this.characteristicUUID, bytes);
                this.readDataCallback.notifiedCallback(this.characteristicUUID, bytes);
            });
#endif
        }

        public void WriteValue(byte[] data, bool withResponse)
        {
#if UNITY_WEBGL
            WebBluetoothScript.Instance.WriteValue(this.characteristicID, data);
            this.writeDataCallback.Notify(this.characteristicUUID, data, withResponse);
#endif
        }

        public void StartNotifications(Action<byte[]> action)
        {
#if UNITY_WEBGL
            WebBluetoothScript.Instance.StartNotifications(this.characteristicID, (data)=> { action?.Invoke(data); this.notifiedCallback.Notify(this.characteristicUUID, data); });
#endif
        }

        public void StopNotifications()
        {
#if UNITY_WEBGL
            WebBluetoothScript.Instance.StopNotifications(this.characteristicID);
#endif
        }
    }
}
