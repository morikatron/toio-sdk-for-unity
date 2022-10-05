using System;

namespace toio
{
    public class BLEWebCharacteristic : BLECharacteristicInterface
    {
        public string deviceAddress { get; private set; }
        public string serviceUUID { get; private set; }
        public string characteristicUUID { get; }

        private int serviceID;
        private int characteristicID;

        public BLEWebCharacteristic(int serviceID, string deviceAddress, string serviceUUID, int characteristicID, string characteristicUUID)
        {
            this.serviceID = serviceID;
            this.deviceAddress = deviceAddress;
            this.serviceUUID = serviceUUID.ToUpper();
            this.characteristicID = characteristicID;
            this.characteristicUUID = characteristicUUID.ToUpper();
        }

        public void ReadValue(Action<string, byte[]> action)
        {
#if UNITY_WEBGL
            WebBluetoothScript.ReadValue(this.characteristicID, (id, bytes) => {
                action(this.characteristicUUID, bytes);
            });
#endif
        }

        public void WriteValue(byte[] data, bool withResponse)
        {
#if UNITY_WEBGL
            WebBluetoothScript.WriteValue(this.characteristicID, data);
#endif
        }

        public void StartNotifications(Action<byte[]> action)
        {
#if UNITY_WEBGL
            WebBluetoothScript.StartNotifications(this.characteristicID, action);
#endif
        }

        public void StopNotifications()
        {
#if UNITY_WEBGL
            WebBluetoothScript.StopNotifications(this.characteristicID);
#endif
        }
    }
}
