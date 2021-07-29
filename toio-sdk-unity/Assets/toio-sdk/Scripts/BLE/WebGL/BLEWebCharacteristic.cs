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
            WebBluetoothScript.Instance.ReadValue(this.characteristicID, (id, bytes) => {
                action(this.characteristicUUID, bytes);
            });
#endif
        }

        public void WriteValue(byte[] data, bool withResponse)
        {
#if UNITY_WEBGL
            WebBluetoothScript.Instance.WriteValue(this.characteristicID, data);
#endif
        }

        public void StartNotifications(Action<byte[]> action)
        {
#if UNITY_WEBGL
            WebBluetoothScript.Instance.StartNotifications(this.characteristicID, action);
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
